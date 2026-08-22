using System.ComponentModel.DataAnnotations;

namespace DOMAIN.Model.Funcionario
{
    public class AtualizarFuncionarioModel
    {
        [Required]
        public string? Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "O perfil é obrigatório.")]
        public PerfilFuncionario Perfil { get; set; }

        public bool Ativo { get; set; }

        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string? NovaSenha { get; set; }
    }
}
