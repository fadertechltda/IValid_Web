using System.ComponentModel.DataAnnotations;

namespace DOMAIN.Model.Funcionario
{
    public class CriarFuncionarioModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string? Senha { get; set; }

        [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem.")]
        public string? ConfirmarSenha { get; set; }

        [Required(ErrorMessage = "O perfil é obrigatório.")]
        public PerfilFuncionario Perfil { get; set; }
    }
}
