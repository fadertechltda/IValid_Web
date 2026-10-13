using System.ComponentModel.DataAnnotations;

namespace DOMAIN.Model.Cliente
{
    public class CriarClienteModel
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string? NomeCompleto { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }
    }

    public class AtualizarClienteModel
    {
        [Required]
        [StringLength(120, MinimumLength = 2)]
        public string? NomeCompleto { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        public EnderecoClienteModel? Endereco { get; set; }
    }

    public class TokenDispositivoModel
    {
        [Required]
        [StringLength(4096, MinimumLength = 10)]
        public string? Token { get; set; }
    }

    public class PerfilClienteModel
    {
        public string? Id { get; set; }

        public string? NomeCompleto { get; set; }

        public string? Email { get; set; }

        public string? Telefone { get; set; }

        public EnderecoClienteModel? Endereco { get; set; }

        public int TotalDoacoes { get; set; }

        public double CashbackDisponivel { get; set; }

        public string? Nivel { get; set; }

        public int PercentualCashbackNivel { get; set; }

        public int? DoacoesParaProximoNivel { get; set; }

        public ImpactoModel Impacto { get; set; } = new();
    }
}
