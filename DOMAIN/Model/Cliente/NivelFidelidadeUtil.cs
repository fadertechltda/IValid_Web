namespace DOMAIN.Model.Cliente
{
    public record NivelFidelidade(string Nome, int MinimoDoacoes, int? MaximoDoacoes, double MultiplicadorCashback);

    public static class NivelFidelidadeUtil
    {
        public const double PercentualMaximoCashbackPorPedido = 0.15;

        public static readonly NivelFidelidade Bronze = new("Bronze", 0, 10, 0.01);
        public static readonly NivelFidelidade Prata = new("Prata", 11, 30, 0.03);
        public static readonly NivelFidelidade Ouro = new("Ouro", 31, null, 0.05);

        public static NivelFidelidade Obter(int totalDoacoes)
        {
            if (totalDoacoes <= Bronze.MaximoDoacoes) return Bronze;
            if (totalDoacoes <= Prata.MaximoDoacoes) return Prata;
            return Ouro;
        }

        public static int? DoacoesParaProximoNivel(int totalDoacoes)
        {
            var nivel = Obter(totalDoacoes);
            return nivel.MaximoDoacoes.HasValue ? nivel.MaximoDoacoes.Value + 1 - totalDoacoes : null;
        }

        public static double CalcularCashback(double valorDoado, int totalDoacoesAtual)
        {
            return Math.Round(valorDoado * Obter(totalDoacoesAtual).MultiplicadorCashback, 2);
        }

        public static double CashbackMaximoPermitido(double subtotal, double cashbackDisponivel)
        {
            return Math.Round(Math.Min(subtotal * PercentualMaximoCashbackPorPedido, Math.Max(cashbackDisponivel, 0)), 2);
        }
    }
}
