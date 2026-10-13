using DOMAIN.Model.Pedido;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using REPOSITORY.Mapeadores.Cliente;
using SERVICE.Opcoes;

namespace SERVICE.Processo
{
    public interface INotificacaoProcesso
    {
        Task NotificarStatusPedidoAsync(string? usuarioId, string pedidoId, string status);
        Task NotificarOfertasAsync(int quantidadeProdutos);
    }

    public class NotificacaoProcesso(IClienteMapeador clienteMapeador, NotificacaoOpcoes opcoes, ILogger<NotificacaoProcesso> logger) : INotificacaoProcesso
    {
        private readonly IClienteMapeador _clienteMapeador = clienteMapeador;
        private readonly NotificacaoOpcoes _opcoes = opcoes;
        private readonly ILogger<NotificacaoProcesso> _logger = logger;

        public async Task NotificarStatusPedidoAsync(string? usuarioId, string pedidoId, string status)
        {
            if (!PodeEnviar() || string.IsNullOrEmpty(usuarioId)) return;

            try
            {
                var cliente = await _clienteMapeador.ObterAsync(usuarioId);
                var tokens = cliente?.TokensDispositivo ?? [];

                if (tokens.Count == 0) return;

                var mensagem = new MulticastMessage
                {
                    Tokens = tokens,
                    Notification = new Notification
                    {
                        Title = "Atualização do seu pedido",
                        Body = TextoStatus(status)
                    },
                    Data = new Dictionary<string, string>
                    {
                        ["tipo"] = "pedido_status",
                        ["pedidoId"] = pedidoId,
                        ["status"] = status
                    },
                    Android = new AndroidConfig { Priority = Priority.High }
                };

                var resposta = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(mensagem);
                await RemoverTokensInvalidosAsync(usuarioId, tokens, resposta);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao enviar notificação do pedido {PedidoId}", pedidoId);
            }
        }

        public async Task NotificarOfertasAsync(int quantidadeProdutos)
        {
            if (!PodeEnviar() || quantidadeProdutos <= 0) return;

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(new Message
                {
                    Topic = _opcoes.TopicoOfertas,
                    Notification = new Notification
                    {
                        Title = "Novas ofertas de última hora",
                        Body = quantidadeProdutos == 1
                            ? "1 produto entrou na faixa de maior desconto hoje."
                            : $"{quantidadeProdutos} produtos entraram na faixa de maior desconto hoje."
                    },
                    Data = new Dictionary<string, string> { ["tipo"] = "ofertas" },
                    Android = new AndroidConfig { Priority = Priority.Normal }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao enviar notificação de ofertas");
            }
        }

        private bool PodeEnviar()
        {
            if (!_opcoes.Habilitada) return false;

            if (FirebaseApp.DefaultInstance == null)
            {
                _logger.LogWarning("Notificações desativadas: o FirebaseApp não foi inicializado.");
                return false;
            }

            return true;
        }

        private async Task RemoverTokensInvalidosAsync(string usuarioId, IReadOnlyList<string> tokens, BatchResponse resposta)
        {
            for (int i = 0; i < resposta.Responses.Count && i < tokens.Count; i++)
            {
                var codigo = resposta.Responses[i].Exception?.MessagingErrorCode;

                if (codigo is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
                {
                    await _clienteMapeador.RemoverTokenDispositivoAsync(usuarioId, tokens[i]);
                }
            }
        }

        private static string TextoStatus(string status) => status switch
        {
            StatusPedidoUtil.Confirmado => "Seu pedido foi confirmado pela loja.",
            StatusPedidoUtil.Separado => "Seu pedido foi separado e está pronto para retirada.",
            StatusPedidoUtil.EmRota => "Seu pedido saiu para entrega.",
            StatusPedidoUtil.Finalizado => "Pedido concluído. Obrigado por reduzir o desperdício!",
            StatusPedidoUtil.Cancelado => "Seu pedido foi cancelado.",
            _ => "O status do seu pedido mudou."
        };
    }
}
