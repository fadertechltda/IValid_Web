using DOMAIN.Model.Configuracao;
using DOMAIN.Validador.Abstrato;
using FluentValidation;

namespace DOMAIN.Validador.Configuracao
{
    public class ConfiguracaoValidacao : ValidadorAbstrato<ConfiguracaoModel>
    {
        public override void AssineRegrasInclusao() => AssineRegrasAtualizacao();

        public override void AssineRegrasAtualizacao()
        {
            RuleFor(c => c.PercentualDescontoAmarelo)
                .InclusiveBetween(0, 100)
                .WithMessage("O desconto do alerta amarelo deve estar entre 0 e 100%.");

            RuleFor(c => c.PercentualDescontoVermelho)
                .InclusiveBetween(0, 100)
                .WithMessage("O desconto do alerta vermelho deve estar entre 0 e 100%.");

            RuleFor(c => c.PercentualDescontoVermelho)
                .GreaterThanOrEqualTo(c => c.PercentualDescontoAmarelo)
                .WithMessage("O desconto do alerta vermelho (vence primeiro) deve ser maior ou igual ao do alerta amarelo.");
        }

        public override void AssineRegrasExclusao()
        {
        }
    }
}
