using Google.Cloud.Firestore;

namespace DOMAIN.Model.Pedido
{
    [FirestoreData]
    public class ItemPedidoModel
    {
        [FirestoreProperty("productId")]
        public string? ProdutoId { get; set; }

        [FirestoreProperty("name")]
        public string? NomeProduto { get; set; }

        [FirestoreProperty("quantity")]
        public int Quantidade { get; set; }

        [FirestoreProperty("subtotal")]
        public double Subtotal { get; set; }
    }
}
