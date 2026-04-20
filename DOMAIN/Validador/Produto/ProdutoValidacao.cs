using DOMAIN.Model;
using DOMAIN.Validador.Abstrato;
using FluentValidation;

namespace DOMAIN.Validador.Produto
{
    public class ProdutoValidacao : ValidadorAbstrato<ProdutoModel>
    {
        public override void AssineRegrasInclusao()
        {
            RuleFor(produto => produto.Nome)
                .NotEmpty()
                .WithMessage("O nome do produto não pode ser vazio");

            RuleFor(produto => produto.Status)
                .Must(status => new[] { "Ativo", "Inativo", "Esgotado" }
                .Contains(status));

            RuleFor(produto => produto.Preco)
                .GreaterThan(0)
                .WithMessage("O valor do produto deve ser maior que 0");

            RuleFor(produto => produto.PrecoPromocao)
                .LessThan(produto => produto.Preco)
                .WithMessage("O preço promocional deve ser menor que o preço original.");

            RuleFor(p => p.PrecoPromocao)
                .GreaterThan(0);

            RuleFor(produto => produto.DataVencimento)
                .GreaterThan(DateTime.Now)
                .WithMessage("O produto já está vencido no momento do cadastro.");

            RuleFor(produto => produto.DiaValidade)
                .GreaterThanOrEqualTo(1);

            RuleFor(produto => produto.Quantidade)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantidade de produtos não pode ser negativa");

            RuleFor(produto => produto.DescricaoPorcentual)
                .InclusiveBetween(0, 100);

            RuleFor(produto => produto.UrlImagem)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(produto => !string.IsNullOrEmpty(produto.UrlImagem));
        }

        public override void AssineRegrasAtualizacao()
        {
            RuleFor(produto => produto.Nome)
               .NotEmpty()
               .WithMessage("O nome do produto não pode ser vazio");

            RuleFor(produto => produto.Status)
                .Must(status => new[] { "Ativo", "Inativo", "Esgotado" }
                .Contains(status));

            RuleFor(produto => produto.Preco)
                .GreaterThan(0)
                .WithMessage("O valor do produto deve ser maior que 0");

            RuleFor(produto => produto.PrecoPromocao)
                .LessThan(produto => produto.Preco)
                .WithMessage("O preço promocional deve ser menor que o preço original.");

            RuleFor(p => p.PrecoPromocao)
                .GreaterThan(0);

            RuleFor(produto => produto.DataVencimento)
                .GreaterThan(DateTime.Now)
                .WithMessage("O produto já está vencido no momento do cadastro.");

            RuleFor(produto => produto.DiaValidade)
                .GreaterThanOrEqualTo(1);

            RuleFor(produto => produto.Quantidade)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantidade de produtos não pode ser negativa");

            RuleFor(produto => produto.DescricaoPorcentual)
                .InclusiveBetween(0, 100);

            RuleFor(produto => produto.UrlImagem)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(produto => !string.IsNullOrEmpty(produto.UrlImagem));
        }

        public override void AssineRegrasExclusao()
        {
            RuleFor(produto => produto.Id)
                .NotEmpty()
                .WithMessage("É necessário informar uma código válido para realizar a exclusão.");
        }        
    }
}
