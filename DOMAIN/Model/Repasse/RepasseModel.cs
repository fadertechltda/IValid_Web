namespace DOMAIN.Model.Repasse
{
    public class PedidoRepasseModel
    {
        public string? PedidoId { get; set; }

        public DateTime Data { get; set; }

        public double Total { get; set; }

        public double Comissao { get; set; }

        public double Liquido { get; set; }
    }

    public class FechamentoSemanalModel
    {
        public DateTime InicioSemana { get; set; }

        public DateTime FimSemana { get; set; }

        public DateTime DataPrevistaPagamento { get; set; }

        /// <summary>"aberto" (semana em andamento) ou "fechado".</summary>
        public string Situacao { get; set; } = "fechado";

        public int QuantidadePedidos { get; set; }

        public double VendasBrutas { get; set; }

        public double Comissao { get; set; }

        public double ValorARepassar { get; set; }

        public List<PedidoRepasseModel> Pedidos { get; set; } = [];
    }

    public class ResumoRepasseModel
    {
        public double ComissaoPercentual { get; set; }

        public double TotalARepassar { get; set; }

        public List<FechamentoSemanalModel> Fechamentos { get; set; } = [];
    }
}
