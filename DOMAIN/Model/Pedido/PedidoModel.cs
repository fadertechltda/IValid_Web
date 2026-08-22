using Google.Cloud.Firestore;

namespace DOMAIN.Model.Pedido
{
    [FirestoreData]
    public class PedidoModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("userId")]
        public string? UsuarioId { get; set; }

        [FirestoreProperty("endereco")]
        public string? Endereco { get; set; }

        [FirestoreProperty("formaPagamento")]
        public string? FormaPagamento { get; set; }

        [FirestoreProperty("itens")]
        public List<ItemPedidoModel> Itens { get; set; } = [];

        [FirestoreProperty("total")]
        public double Total { get; set; }

        [FirestoreProperty("status")]
        public string? Status { get; set; }

        [FirestoreProperty("timestamp")]
        public DateTime DataPedido { get; set; }

        public string? ClienteNome { get; set; }
    }
}
