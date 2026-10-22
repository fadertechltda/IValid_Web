using DOMAIN.Model.Pedido;
using DOMAIN.Model.Repasse;
using SERVICE.Opcoes;

namespace SERVICE.Processo
{
    public class RepasseProcesso(PedidoProcesso pedidoProcesso, RepasseOpcoes opcoes)
    {
        // O Brasil não tem horário de verão desde 2019: o fuso de Brasília é fixo em UTC-3.
        private static readonly TimeSpan FusoBrasilia = TimeSpan.FromHours(-3);

        private readonly PedidoProcesso _pedidoProcesso = pedidoProcesso;
        private readonly RepasseOpcoes _opcoes = opcoes;

        public async Task<ResumoRepasseModel> ObterAsync(string supermercadoId)
        {
            var pedidos = await _pedidoProcesso.ListarPedidos(supermercadoId);
            return Calcular(pedidos, DateTime.UtcNow);
        }

        public ResumoRepasseModel Calcular(IEnumerable<PedidoModel> pedidos, DateTime agoraUtc)
        {
            double percentual = Math.Clamp(_opcoes.ComissaoPercentual, 0, 100);
            DateTime hoje = ParaBrasilia(agoraUtc).Date;

            var fechamentos = pedidos
                .Where(Elegivel)
                .Select(p => (Pedido: p, Data: ParaBrasilia(p.DataAtualizacao ?? p.DataPedido)))
                .GroupBy(x => InicioDaSemana(x.Data.Date))
                .OrderByDescending(g => g.Key)
                .Select(g => MontarFechamento(g.Key, g.OrderBy(x => x.Data), percentual, hoje))
                .ToList();

            return new ResumoRepasseModel
            {
                ComissaoPercentual = percentual,
                TotalARepassar = Arredondar(fechamentos.Sum(f => f.ValorARepassar)),
                Fechamentos = fechamentos
            };
        }

        private static FechamentoSemanalModel MontarFechamento(DateTime inicio, IEnumerable<(PedidoModel Pedido, DateTime Data)> itens, double percentual, DateTime hoje)
        {
            var pedidos = itens.Select(x =>
            {
                double comissao = Arredondar(x.Pedido.Total * percentual / 100);

                return new PedidoRepasseModel
                {
                    PedidoId = x.Pedido.Id,
                    Data = x.Data,
                    Total = Arredondar(x.Pedido.Total),
                    Comissao = comissao,
                    Liquido = Arredondar(x.Pedido.Total - comissao)
                };
            }).ToList();

            DateTime fim = inicio.AddDays(6);

            return new FechamentoSemanalModel
            {
                InicioSemana = inicio,
                FimSemana = fim,
                DataPrevistaPagamento = fim.AddDays(3),
                Situacao = hoje > fim ? "fechado" : "aberto",
                QuantidadePedidos = pedidos.Count,
                VendasBrutas = Arredondar(pedidos.Sum(p => p.Total)),
                Comissao = Arredondar(pedidos.Sum(p => p.Comissao)),
                ValorARepassar = Arredondar(pedidos.Sum(p => p.Liquido)),
                Pedidos = pedidos
            };
        }

        /// <summary>Só entra no repasse o pedido pago online, entregue (FINALIZADO) e sem estorno pendente.</summary>
        private static bool Elegivel(PedidoModel pedido) =>
            pedido.PagamentoOnline
            && pedido.StatusPagamento == StatusPagamentoUtil.Pago
            && !pedido.EstornoPendente
            && StatusPedidoUtil.Normalizar(pedido.Status) == "FINALIZADO";

        private static DateTime ParaBrasilia(DateTime data)
        {
            var utc = data.Kind == DateTimeKind.Local ? data.ToUniversalTime() : DateTime.SpecifyKind(data, DateTimeKind.Utc);
            return utc.Add(FusoBrasilia);
        }

        private static DateTime InicioDaSemana(DateTime data) => data.AddDays(-(((int)data.DayOfWeek + 6) % 7));

        private static double Arredondar(double valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
    }
}
