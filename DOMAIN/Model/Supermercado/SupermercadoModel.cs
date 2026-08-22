using Google.Cloud.Firestore;

namespace DOMAIN.Model.Supermercado
{
    [FirestoreData]
    public class SupermercadoModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty("nome")]
        public string? Nome { get; set; }

        [FirestoreProperty("cnpj")]
        public string? Cnpj { get; set; }

        [FirestoreProperty("endereco")]
        public string? Endereco { get; set; }

        [FirestoreProperty("codigoAcesso")]
        public string? CodigoAcesso { get; set; }
    }
}
