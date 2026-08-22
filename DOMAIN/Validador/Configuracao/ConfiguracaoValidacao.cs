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
            RuleFor(c => c.DiasAlertaVermelho)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Os dias do alerta vermelho não podem ser negativos.");

            RuleFor(c => c.DiasAlertaAmarelo)
                .GreaterThan(c => c.DiasAlertaVermelho)
                .WithMessage("O alerta amarelo precisa valer para mais dias antes do vencimento do que o alerta vermelho (ele dispara primeiro, antes da faixa mais crítica).");

            RuleFor(c => c.PercentualDescontoAmarelo)
                .InclusiveBetween(0, 100)
                .WithMessage("O desconto do alerta amarelo deve estar entre 0 e 100%.");

            RuleFor(c => c.PercentualDescontoVermelho)
                .InclusiveBetween(0, 100)
                .WithMessage("O desconto do alerta vermelho deve estar entre 0 e 100%.");
        }

        public override void AssineRegrasExclusao()
        {
        }
    }
}
