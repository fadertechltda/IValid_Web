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

    public class MercadoPagoOpcoes
    {
        /// <summary>Access Token da conta recebedora. Informe somente por variável de ambiente (MercadoPago__AccessToken).</summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>Public Key da aplicação (não é segredo). O app a busca na API para tokenizar o cartão. Env: MercadoPago__PublicKey.</summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// SÓ PARA TESTE: nome enviado como pagador do Pix. Com "APRO" o ambiente de teste aprova o Pix sozinho.
        /// Deixe vazio em produção. Env: MercadoPago__NomePagadorTeste.
        /// </summary>
        public string NomePagadorTeste { get; set; } = string.Empty;

        public string BaseUrl { get; set; } = "https://api.mercadopago.com/";
    }

    public class RepasseOpcoes
    {
        /// <summary>Comissão do IValid sobre cada pedido pago online, em porcentagem (1.49 = 1,49%).</summary>
        public double ComissaoPercentual { get; set; } = 1.49;
    }

    public class NotificacaoOpcoes
    {
        public bool Habilitada { get; set; } = true;

        public string TopicoOfertas { get; set; } = "ofertas";
    }
}
