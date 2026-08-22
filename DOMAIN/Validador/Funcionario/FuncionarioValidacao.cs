using DOMAIN.Model.Funcionario;
using DOMAIN.Validador.Abstrato;
using FluentValidation;

namespace DOMAIN.Validador.Funcionario
{
    public class FuncionarioValidacao : ValidadorAbstrato<FuncionarioModel>
    {
        public override void AssineRegrasInclusao()
        {
            RuleFor(funcionario => funcionario.Nome)
                .NotEmpty()
                .WithMessage("O nome do funcionário não pode ser vazio.");

            RuleFor(funcionario => funcionario.SupermercadoId)
                .NotEmpty()
                .WithMessage("O funcionário precisa estar vinculado a um supermercado.");
        }

        public override void AssineRegrasAtualizacao()
        {
            RuleFor(funcionario => funcionario.Id)
                .NotEmpty()
                .WithMessage("O Id do funcionário é obrigatório para atualização.");

            RuleFor(funcionario => funcionario.Nome)
                .NotEmpty()
                .WithMessage("O nome do funcionário não pode ser vazio.");
        }

        public override void AssineRegrasExclusao()
        {
            RuleFor(funcionario => funcionario.Id)
                .NotEmpty()
                .WithMessage("É necessário informar um Id válido para realizar a exclusão.");
        }
    }
}
