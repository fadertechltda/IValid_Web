using System.ComponentModel.DataAnnotations;

namespace DOMAIN.Model.Pedido
{
    public static class MetodoPagamentoOnline
    {
        public const string Cartao = "cartao";
        public const string Pix = "pix";
    }

    public class PagarPedidoModel
    {
        [Required]
        public string? Metodo { get; set; }

        /// <summary>Token de uso único gerado pelo SDK do Mercado Pago no aplicativo (somente cartão).</summary>
        public string? TokenCartao { get; set; }

        /// <summary>Bandeira devolvida pelo SDK, por exemplo "visa" ou "master" (somente cartão).</summary>
        public string? BandeiraId { get; set; }

        /// <summary>"credit_card" ou "debit_card" (somente cartão). Padrão: crédito.</summary>
        public string? TipoCartao { get; set; }

        [Range(1, 12)]
        public int Parcelas { get; set; } = 1;
    }

    public class SincronizarPagamentoModel
    {
        [Required]
        public string? OrdemId { get; set; }
    }

    public class ResultadoPagamentoModel
    {
        public string? PedidoId { get; set; }

        public string? OrdemId { get; set; }

        /// <summary>"pago" ou "aguardando".</summary>
        public string Situacao { get; set; } = "aguardando";

        public bool Pago { get; set; }

        public double Valor { get; set; }

        public string? QrCode { get; set; }

        public string? QrCodeBase64 { get; set; }

        public string? TicketUrl { get; set; }
    }
}
