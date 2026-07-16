using System.ComponentModel.DataAnnotations;

namespace DOMAIN.Model.Usuario
{
    public class RegistroModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string? NomeCompleto { get; set; }

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string? Senha { get; set; }

        [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem.")]
        public string? ConfirmarSenha { get; set; }
    }
}
