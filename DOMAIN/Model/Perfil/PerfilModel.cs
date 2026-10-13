using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace DOMAIN.Model.Perfil
{
    [FirestoreData]
    public class PerfilModel
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Informe o nome do perfil.")]
        [StringLength(60, ErrorMessage = "O nome do perfil deve ter até 60 caracteres.")]
        [FirestoreProperty("nome")]
        public string? Nome { get; set; }

        [FirestoreProperty("supermercadoId")]
        public string? SupermercadoId { get; set; }

        [FirestoreProperty("permissoes")]
        public List<string> Permissoes { get; set; } = [];

        [FirestoreProperty("chaveLegada")]
        public string? ChaveLegada { get; set; }

        public int QuantidadeFuncionarios { get; set; }
    }
}
