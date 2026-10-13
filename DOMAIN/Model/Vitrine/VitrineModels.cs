using System.ComponentModel.DataAnnotations;

namespace DOMAIN.Model.Vitrine
{
    public class ProdutoVitrineModel
    {
        public string? Id { get; set; }

        public string? Nome { get; set; }

        public string? Marca { get; set; }

        public string? UrlImagem { get; set; }

        public string? CategoriaId { get; set; }

        public string? SupermercadoId { get; set; }

        public string? NomeSupermercado { get; set; }

        public double PrecoOriginal { get; set; }

        public double PrecoAtual { get; set; }

        public int PercentualDesconto { get; set; }

        public int DiasParaVencer { get; set; }

        public DateTime DataVencimento { get; set; }

        public string? Status { get; set; }

        public int QuantidadeDisponivel { get; set; }
    }

    public class FiltroVitrineModel
    {
        public string? CategoriaId { get; set; }

        public string? Busca { get; set; }

        public string? SupermercadoId { get; set; }

        public string? Status { get; set; }

        public string? Ordem { get; set; }

        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        [Range(1, 100)]
        public int TamanhoPagina { get; set; } = 20;
    }

    public class PaginaModel<T>
    {
        public List<T> Itens { get; set; } = [];

        public int Pagina { get; set; }

        public int TamanhoPagina { get; set; }

        public int Total { get; set; }

        public bool TemProxima => Pagina * TamanhoPagina < Total;
    }

    public class SupermercadoPublicoModel
    {
        public string? Id { get; set; }

        public string? Nome { get; set; }

        public string? Endereco { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }

    public class DadosLojaModel
    {
        [Required(ErrorMessage = "Informe o nome da loja.")]
        [StringLength(120)]
        public string? Nome { get; set; }

        [StringLength(250)]
        public string? Endereco { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude inválida.")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude inválida.")]
        public double? Longitude { get; set; }
    }
}
