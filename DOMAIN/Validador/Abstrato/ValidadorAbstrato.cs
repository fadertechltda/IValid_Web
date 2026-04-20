using FluentValidation;

namespace DOMAIN.Validador.Abstrato
{
    public abstract class ValidadorAbstrato<T>: AbstractValidator<T> where T : class
    {
        protected ValidadorAbstrato() { }
        public abstract void AssineRegrasInclusao();
        public abstract void AssineRegrasAtualizacao();
        public abstract void AssineRegrasExclusao();
    }
}
