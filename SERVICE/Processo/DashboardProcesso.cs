using DOMAIN.Model.Dashboard;
using DOMAIN.Model.Pedido;
using DOMAIN.Model.Produto;

namespace SERVICE.Processo
{
    public class DashboardProcesso(ProdutoProcesso produtoProcesso, PedidoProcesso pedidoProcesso)
    {
        private const int QuantidadeProdutosUrgentes = 5;

        private readonly ProdutoProcesso _produtoProcesso = produtoProcesso;
        private readonly PedidoProcesso _pedidoProcesso = pedidoProcesso;

        public async Task<DashboardModel> ObterDashboard(string supermercadoId)
        {
            List<ProdutoModel> produtos = await _produtoProcesso.ListarProdutos(supermercadoId);
            List<PedidoModel> pedidos = await _pedidoProcesso.ListarPedidos(supermercadoId);

            DashboardModel dashboard = new()
            {
                TotalProdutos = produtos.Count,
                TotalVermelho = produtos.Count(p => p.Status?.ToUpper() == "VERMELHO"),
                TotalAmarelo = produtos.Count(p => p.Status?.ToUpper() == "AMARELO"),
                TotalVerde = produtos.Count(p => p.Status?.ToUpper() == "VERDE"),
                TotalVencido = produtos.Count(p => p.Status?.ToUpper() == "VENCIDO"),
                TotalEsgotado = produtos.Count(p => p.Esgotado),

                ValorEmRisco = produtos
                    .Where(p => !p.Esgotado && p.Status?.ToUpper() is "VERMELHO" or "AMARELO")
                    .Sum(p => p.Preco * p.Quantidade),

                ProdutosUrgentes = [.. produtos
                    .Where(p => !p.Esgotado && p.Status?.ToUpper() != "VENCIDO")
                    .OrderBy(p => p.DataVencimento)
                    .Take(QuantidadeProdutosUrgentes)]
            };

            CalcularFaturamentoELucro(dashboard, pedidos, produtos);

            return dashboard;
        }

        private static void CalcularFaturamentoELucro(DashboardModel dashboard, List<PedidoModel> pedidos, List<ProdutoModel> produtos)
        {
            var produtosPorId = produtos
                .Where(p => !string.IsNullOrEmpty(p.Id))
                .ToDictionary(p => p.Id!, p => p);

            var pedidosFinalizados = pedidos
                .Where(p => StatusPedidoUtil.Normalizar(p.Status) == "FINALIZADO")
                .ToList();

            dashboard.TotalPedidos = pedidosFinalizados.Count;
            dashboard.FaturamentoTotal = pedidosFinalizados.Sum(p => p.Total);

            double lucroTotal = 0;
            bool lucroIncompleto = false;

            foreach (var pedido in pedidosFinalizados)
            {
                foreach (var item in pedido.Itens)
                {
                    if (string.IsNullOrEmpty(item.ProdutoId) || !produtosPorId.TryGetValue(item.ProdutoId, out var produto) || !produto.PrecoCusto.HasValue)
                    {
                        lucroIncompleto = true;
                        continue;
                    }

                    lucroTotal += item.Subtotal - (produto.PrecoCusto.Value * item.Quantidade);
                }
            }

            dashboard.LucroTotal = lucroTotal;
            dashboard.LucroIncompleto = lucroIncompleto;
        }
    }
}
