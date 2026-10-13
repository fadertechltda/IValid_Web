using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace DOMAIN.Model.Categoria
{
    [FirestoreData]
    public class CategoriaModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da categoria.")]
        [StringLength(60)]
        [FirestoreProperty("name")]
        public string? Nome { get; set; }

        [FirestoreProperty("icon")]
        public int? Icone { get; set; }

        [FirestoreProperty("order")]
        public int Ordem { get; set; }
    }
}
