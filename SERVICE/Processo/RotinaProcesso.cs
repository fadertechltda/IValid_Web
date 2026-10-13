using DOMAIN.Model.Configuracao;
using Microsoft.Extensions.Logging;
using REPOSITORY.Mapeadores.Produto;
using REPOSITORY.Mapeadores.Rotina;

namespace SERVICE.Processo
{
    public class RotinaProcesso(
        IProdutoMapeador produtoMapeador,
        IRotinaMapeador rotinaMapeador,
        VitrineProcesso vitrineProcesso,
        PedidoProcesso pedidoProcesso,
        INotificacaoProcesso notificacaoProcesso,
        ILogger<RotinaProcesso> logger)
    {
        private const string RotinaRecalculo = "recalculo-produtos";
        private const string RotinaOfertas = "notificacao-ofertas";

        private readonly IProdutoMapeador _produtoMapeador = produtoMapeador;
        private readonly IRotinaMapeador _rotinaMapeador = rotinaMapeador;
        private readonly VitrineProcesso _vitrineProcesso = vitrineProcesso;
        private readonly PedidoProcesso _pedidoProcesso = pedidoProcesso;
        private readonly INotificacaoProcesso _notificacaoProcesso = notificacaoProcesso;
        private readonly ILogger<RotinaProcesso> _logger = logger;

        public async Task ExecutarAsync(DateOnly hoje)
        {
            int cancelados = await _pedidoProcesso.CancelarPendentesExpiradosAsync();
            if (cancelados > 0)
            {
                _logger.LogInformation("{Quantidade} pedido(s) pendente(s) de pagamento foram cancelados por expiração.", cancelados);
            }

            var marca = hoje.ToString("yyyy-MM-dd");

            if (await _rotinaMapeador.ObterUltimaExecucaoAsync(RotinaRecalculo) != marca)
            {
                await RecalcularProdutosAsync();
                await _rotinaMapeador.RegistrarExecucaoAsync(RotinaRecalculo, marca);
            }

            if (await _rotinaMapeador.ObterUltimaExecucaoAsync(RotinaOfertas) != marca)
            {
                await NotificarOfertasDoDiaAsync();
                await _rotinaMapeador.RegistrarExecucaoAsync(RotinaOfertas, marca);
            }
        }

        private async Task RecalcularProdutosAsync()
        {
            var produtos = await _produtoMapeador.ListarTodosAsync();
            var originais = produtos.ToDictionary(p => p.Id!, p => (p.DiaValidade, p.PrecoPromocao, p.Status, p.DescricaoPorcentual));

            await _vitrineProcesso.AplicarRegrasAsync(produtos);

            int atualizados = 0;

            foreach (var produto in produtos)
            {
                var original = originais[produto.Id!];

                bool mudou = original.DiaValidade != produto.DiaValidade
                    || Math.Abs(original.PrecoPromocao - produto.PrecoPromocao) > 0.001
                    || original.Status != produto.Status
                    || original.DescricaoPorcentual != produto.DescricaoPorcentual;

                if (!mudou) continue;

                await _produtoMapeador.AtualizarCamposAsync(produto.Id!, new Dictionary<string, object>
                {
                    ["daysValidity"] = produto.DiaValidade,
                    ["newPrice"] = produto.PrecoPromocao,
                    ["status"] = produto.Status ?? string.Empty,
                    ["descricaoPorcentual"] = produto.DescricaoPorcentual
                });

                atualizados++;
            }

            _logger.LogInformation("Recálculo diário: {Atualizados} de {Total} produto(s) atualizados.", atualizados, produtos.Count);
        }

        private async Task NotificarOfertasDoDiaAsync()
        {
            var produtos = await _produtoMapeador.ListarTodosAsync();
            await _vitrineProcesso.AplicarRegrasAsync(produtos);

            int entraramNaFaixa = produtos.Count(p => VitrineProcesso.EstaDisponivel(p) && p.DiaValidade == ConfiguracaoModel.DiasLimiteVermelho);

            await _notificacaoProcesso.NotificarOfertasAsync(entraramNaFaixa);
        }
    }
}
