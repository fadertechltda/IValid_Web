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
                .WithMessage("O nome do produto não pode ser vazio.");

            RuleFor(produto => produto.Preco)
                .GreaterThan(0)
                .WithMessage("O valor do produto deve ser maior que 0.");

            RuleFor(produto => produto.DataVencimento)
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("O produto já está vencido no momento do cadastro.");

            RuleFor(produto => produto.Quantidade)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantidade de produtos não pode ser negativa.");

            RuleFor(produto => produto.UrlImagem)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(produto => !string.IsNullOrEmpty(produto.UrlImagem))
                .WithMessage("A URL da imagem não é válida.");

        }

        public override void AssineRegrasAtualizacao()
        {
            RuleFor(produto => produto.Id)
                .NotEmpty()
                .WithMessage("O Id do produto é obrigatório para atualização.");

            RuleFor(produto => produto.Nome)
               .NotEmpty()
               .WithMessage("O nome do produto não pode ser vazio.");

            RuleFor(produto => produto.Preco)
                .GreaterThan(0)
                .WithMessage("O valor do produto deve ser maior que 0.");

            RuleFor(produto => produto.DataVencimento)
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("A data de vencimento informada já está no passado.");

            RuleFor(produto => produto.Quantidade)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantidade de produtos não pode ser negativa.");

            RuleFor(produto => produto.UrlImagem)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(produto => !string.IsNullOrEmpty(produto.UrlImagem))
                .WithMessage("A URL da imagem não é válida.");

        }

        public override void AssineRegrasExclusao()
        {
            RuleFor(produto => produto.Id)
                .NotEmpty()
                .WithMessage("É necessário informar uma código válido para realizar a exclusão.");
        }        
    }
}
