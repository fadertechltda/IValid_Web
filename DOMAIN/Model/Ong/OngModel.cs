using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace DOMAIN.Model.Ong
{
    [FirestoreData]
    public class OngModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Informe o nome da ONG.")]
        [StringLength(120)]
        [FirestoreProperty("nome")]
        public string? Nome { get; set; }

        [StringLength(500)]
        [FirestoreProperty("descricao")]
        public string? Descricao { get; set; }

        [StringLength(120)]
        [FirestoreProperty("cidade")]
        public string? Cidade { get; set; }

        [FirestoreProperty("urlImagem")]
        public string? UrlImagem { get; set; }

        [FirestoreProperty("ativa")]
        public bool Ativa { get; set; } = true;
    }
}
