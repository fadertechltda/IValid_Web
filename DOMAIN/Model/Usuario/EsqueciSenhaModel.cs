using System.ComponentModel.DataAnnotations;

namespace DOMAIN.Model.Usuario
{
    public class EsqueciSenhaModel
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string? Email { get; set; }
    }
}
