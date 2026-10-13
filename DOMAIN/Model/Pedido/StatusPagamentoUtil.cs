namespace DOMAIN.Model.Pedido
{
    public static class StatusPagamentoUtil
    {
        public const string AguardandoPagamento = "aguardando pagamento";
        public const string Pago = "pago";
        public const string PagarNaRetirada = "a pagar na retirada";
        public const string Cancelado = "cancelado";
    }

    public static class FormaPagamentoUtil
    {
        public const string Dinheiro = "Dinheiro";
        public const string Voucher = "Voucher";
        public const string Online = "Online";
        public const string Cashback = "Cashback";
        public const string Demonstracao = "Demonstracao";

        public static readonly string[] Aceitas = [Dinheiro, Voucher, Online, Cashback, Demonstracao];

        public static string? Normalizar(string? forma)
        {
            if (string.IsNullOrWhiteSpace(forma)) return null;
            return Aceitas.FirstOrDefault(f => string.Equals(f, forma.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class OrigemItemUtil
    {
        public const string Compra = "compra";
        public const string Doacao = "doacao";

        public static bool EhDoacao(string? origem) => string.Equals(origem?.Trim(), Doacao, StringComparison.OrdinalIgnoreCase);
    }
}
