using Google.Cloud.Firestore;
using DOMAIN.Model.Cliente;

namespace DOMAIN.Model.Pedido
{
    [FirestoreData]
    public class PedidoModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("userId")]
        public string? UsuarioId { get; set; }

        [FirestoreProperty("supermercadoId")]
        public string? SupermercadoId { get; set; }

        [FirestoreProperty("endereco")]
        public string? Endereco { get; set; }

        [FirestoreProperty("formaPagamento")]
        public string? FormaPagamento { get; set; }

        [FirestoreProperty("paymentMethod")]
        public string? FormaPagamentoLegado { get; set; }

        [FirestoreProperty("pagamentoOnline")]
        public bool PagamentoOnline { get; set; }

        [FirestoreProperty("paymentStatus")]
        public string? StatusPagamento { get; set; }

        [FirestoreProperty("payment")]
        public Dictionary<string, object>? Pagamento { get; set; }

        [FirestoreProperty("itens")]
        public List<ItemPedidoModel> Itens { get; set; } = [];

        [FirestoreProperty("subtotal")]
        public double Subtotal { get; set; }

        [FirestoreProperty("cashbackUsed")]
        public double CashbackUsado { get; set; }

        [FirestoreProperty("total")]
        public double Total { get; set; }

        [FirestoreProperty("hasDonations")]
        public bool PossuiDoacoes { get; set; }

        [FirestoreProperty("donationItemsCount")]
        public int QuantidadeItensDoacao { get; set; }

        [FirestoreProperty("donationSubtotal")]
        public double SubtotalDoacao { get; set; }

        [FirestoreProperty("ongId")]
        public string? OngId { get; set; }

        [FirestoreProperty("status")]
        public string? Status { get; set; }

        [FirestoreProperty("timestamp")]
        public DateTime DataPedido { get; set; }

        [FirestoreProperty("atualizadoEm")]
        public DateTime? DataAtualizacao { get; set; }

        [FirestoreProperty("impact")]
        public ImpactoModel? Impacto { get; set; }

        [FirestoreProperty("origemApi")]
        public bool CriadoPelaApi { get; set; }

        [FirestoreProperty("cashbackEarned")]
        public double CashbackGanho { get; set; }

        [FirestoreProperty("beneficiosAplicados")]
        public bool BeneficiosAplicados { get; set; }

        [FirestoreProperty("estoqueDevolvido")]
        public bool EstoqueDevolvido { get; set; }

        [FirestoreProperty("estornoPendente")]
        public bool EstornoPendente { get; set; }

        public string? ClienteNome { get; set; }

        public string? FormaPagamentoExibicao => FormaPagamento ?? FormaPagamentoLegado;

        public string StatusNormalizado => StatusPedidoUtil.Normalizar(Status);
    }
}
