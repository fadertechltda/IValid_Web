using DOMAIN.Model.Pedido;
using Excecoes;
using Microsoft.Extensions.Logging;
using SERVICE.Integracao;

namespace SERVICE.Processo
{
    public class PagamentoProcesso(InfinitePayCliente infinitePayCliente, PedidoProcesso pedidoProcesso, ILogger<PagamentoProcesso> logger)
    {
        private const int TentativasConsulta = 3;
        private static readonly TimeSpan IntervaloConsulta = TimeSpan.FromSeconds(2);

        private readonly InfinitePayCliente _infinitePayCliente = infinitePayCliente;
        private readonly PedidoProcesso _pedidoProcesso = pedidoProcesso;
        private readonly ILogger<PagamentoProcesso> _logger = logger;

        public async Task<LinkPagamentoModel> GerarLinkAsync(string uid, string pedidoId, string? nomeCliente, string? emailCliente)
        {
            var pedido = await _pedidoProcesso.ObterDoClienteAsync(uid, pedidoId);

            if (!pedido.PagamentoOnline || StatusPedidoUtil.Normalizar(pedido.Status) != StatusPedidoUtil.Pendente || pedido.StatusPagamento != StatusPagamentoUtil.AguardandoPagamento)
            {
                throw new IValidExcecao(CodigoExcecao.TransicaoStatusInvalida, "Este pedido não está aguardando pagamento online.");
            }

            int quantidadeItens = pedido.Itens.Sum(i => i.Quantidade);
            string descricao = $"Pedido IValid ({quantidadeItens} {(quantidadeItens == 1 ? "item" : "itens")})";

            var url = await _infinitePayCliente.CriarLinkAsync(pedidoId, EmCentavos(pedido.Total), descricao, nomeCliente, emailCliente);

            return new LinkPagamentoModel { PedidoId = pedidoId, Url = url, Valor = pedido.Total };
        }

        public async Task<PedidoModel> ConfirmarAsync(string uid, string pedidoId, ConfirmarPagamentoModel dados)
        {
            var pedido = await _pedidoProcesso.ObterDoClienteAsync(uid, pedidoId);

            if (pedido.StatusPagamento == StatusPagamentoUtil.Pago) return pedido;

            if (!pedido.PagamentoOnline)
            {
                throw new IValidExcecao(CodigoExcecao.TransicaoStatusInvalida, "Este pedido não usa pagamento online.");
            }

            ResultadoConsultaPagamento? resultado = null;

            for (int tentativa = 1; tentativa <= TentativasConsulta; tentativa++)
            {
                resultado = await _infinitePayCliente.ConsultarAsync(pedidoId, dados.TransactionNsu!, dados.Slug!);
                if (resultado.Pago) break;
                if (tentativa < TentativasConsulta) await Task.Delay(IntervaloConsulta);
            }

            if (resultado == null || !resultado.Pago)
            {
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "O pagamento ainda não foi confirmado pelo InfinitePay. Tente novamente em instantes.");
            }

            long esperado = EmCentavos(pedido.Total);

            if (resultado.ValorPagoCentavos < esperado)
            {
                _logger.LogWarning("Pagamento do pedido {PedidoId} com valor divergente: esperado {Esperado}, pago {Pago}", pedidoId, esperado, resultado.ValorPagoCentavos);
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "O valor pago não confere com o total do pedido. Procure a loja.");
            }

            var dadosPagamento = new Dictionary<string, object>
            {
                ["provedor"] = "InfinitePay",
                ["transactionNsu"] = dados.TransactionNsu!,
                ["slug"] = dados.Slug!,
                ["amountCents"] = resultado.ValorCentavos,
                ["paidAmountCents"] = resultado.ValorPagoCentavos,
                ["installments"] = resultado.Parcelas,
                ["captureMethod"] = resultado.MetodoCaptura ?? dados.CaptureMethod ?? string.Empty,
                ["receiptUrl"] = dados.ReceiptUrl ?? string.Empty,
                ["confirmadoEm"] = DateTime.UtcNow.ToString("O")
            };

            var (pedidoAtualizado, expirado) = await _pedidoProcesso.ConfirmarPagamentoAsync(pedidoId, dadosPagamento);

            if (expirado)
            {
                throw new IValidExcecao(CodigoExcecao.PagamentoNaoConfirmado, "O pedido expirou antes da confirmação do pagamento. A loja fará o estorno.");
            }

            return pedidoAtualizado;
        }

        private static long EmCentavos(double valor) => (long)Math.Round(valor * 100, MidpointRounding.AwayFromZero);
    }
}
