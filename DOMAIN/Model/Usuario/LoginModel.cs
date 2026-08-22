using System.ComponentModel.DataAnnotations;

namespace DOMAIN.Model.Usuario
{
    public class LoginModel
    {
        [Required(ErrorMessage = "O código da loja é obrigatório.")]
        public string? CodigoLoja { get; set; }

        public string? UsuarioChave { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string? Senha { get; set; }
    }
}
