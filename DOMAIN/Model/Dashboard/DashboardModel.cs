using DOMAIN.Model.Produto;

namespace DOMAIN.Model.Dashboard
{
    public class DashboardModel
    {
        public int TotalProdutos { get; set; }
        public int TotalVermelho { get; set; }
        public int TotalAmarelo { get; set; }
        public int TotalVerde { get; set; }
        public int TotalVencido { get; set; }
        public int TotalEsgotado { get; set; }

        public double ValorEmRisco { get; set; }

        public List<ProdutoModel> ProdutosUrgentes { get; set; } = [];

        public int TotalPedidos { get; set; }
        public double FaturamentoTotal { get; set; }
        public double LucroTotal { get; set; }
        public bool LucroIncompleto { get; set; }
    }
}
