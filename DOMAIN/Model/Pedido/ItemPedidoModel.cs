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

        [FirestoreProperty("unitPrice")]
        public double PrecoUnitario { get; set; }

        [FirestoreProperty("originalUnitPrice")]
        public double PrecoUnitarioOriginal { get; set; }

        [FirestoreProperty("subtotal")]
        public double Subtotal { get; set; }

        [FirestoreProperty("origin")]
        public string? Origem { get; set; }

        [FirestoreProperty("isDonation")]
        public bool EhDoacao { get; set; }
    }
}
