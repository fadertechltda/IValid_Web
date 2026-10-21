namespace SERVICE.Opcoes
{
    public class EmailOpcoes
    {
        public string ResendApiKey { get; set; } = string.Empty;

        public string Remetente { get; set; } = string.Empty;

        public string NomeRemetente { get; set; } = "IValid";
    }

    public class PagamentoOpcoes
    {
        public string InfinitePayHandle { get; set; } = string.Empty;

        public string InfinitePayBaseUrl { get; set; } = "https://api.checkout.infinitepay.io";

        public string RedirectUrl { get; set; } = "https://ivalid.invalid/pagamento-concluido";

        public bool PermitirDemonstracao { get; set; }

        public int MinutosExpiracaoPedidoPendente { get; set; } = 30;
    }

    public class NotificacaoOpcoes
    {
        public bool Habilitada { get; set; } = true;

        public string TopicoOfertas { get; set; } = "ofertas";
    }
}
