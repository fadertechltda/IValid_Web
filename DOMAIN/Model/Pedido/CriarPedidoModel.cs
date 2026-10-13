using System.ComponentModel.DataAnnotations;

namespace DOMAIN.Model.Pedido
{
    public class CriarPedidoModel
    {
        [Required]
        public string? FormaPagamento { get; set; }

        [Range(0, double.MaxValue)]
        public double CashbackUsado { get; set; }

        public string? OngId { get; set; }

        public string? Endereco { get; set; }

        [Required]
        [MinLength(1)]
        public List<ItemCriarPedidoModel> Itens { get; set; } = [];
    }

    public class ItemCriarPedidoModel
    {
        [Required]
        public string? ProdutoId { get; set; }

        [Range(1, 999)]
        public int Quantidade { get; set; }

        public string? Origem { get; set; }
    }

    public class ResumoPedidoModel
    {
        public bool Valido => Problemas.Count == 0;

        public string? SupermercadoId { get; set; }

        public List<ItemPedidoModel> Itens { get; set; } = [];

        public double Subtotal { get; set; }

        public double CashbackDisponivel { get; set; }

        public double CashbackMaximoPermitido { get; set; }

        public double CashbackAplicado { get; set; }

        public double Total { get; set; }

        public List<ProblemaItemPedidoModel> Problemas { get; set; } = [];
    }

    public class ProblemaItemPedidoModel
    {
        public string? ProdutoId { get; set; }

        public string? Nome { get; set; }

        public string? Motivo { get; set; }

        public int Solicitado { get; set; }

        public int Disponivel { get; set; }
    }

    public class AtualizarStatusPedidoModel
    {
        [Required]
        public string? Status { get; set; }
    }

    public class ConfirmarPagamentoModel
    {
        [Required]
        public string? TransactionNsu { get; set; }

        [Required]
        public string? Slug { get; set; }

        public string? CaptureMethod { get; set; }

        public string? ReceiptUrl { get; set; }
    }

    public class LinkPagamentoModel
    {
        public string? PedidoId { get; set; }

        public string? Url { get; set; }

        public double Valor { get; set; }
    }
}
