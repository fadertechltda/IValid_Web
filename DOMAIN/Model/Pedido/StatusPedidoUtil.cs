namespace DOMAIN.Model.Pedido
{
    public static class StatusPedidoUtil
    {
        public const string Pendente = "PENDENTE";
        public const string Confirmado = "CONFIRMADO";
        public const string Separado = "SEPARADO";
        public const string EmRota = "EM_ROTA";
        public const string Finalizado = "FINALIZADO";
        public const string Cancelado = "CANCELADO";
        public const string Desconhecido = "DESCONHECIDO";

        private static readonly Dictionary<string, string[]> TransicoesPermitidas = new()
        {
            [Pendente] = [Cancelado],
            [Confirmado] = [Separado, Cancelado],
            [Separado] = [EmRota, Finalizado, Cancelado],
            [EmRota] = [Finalizado, Cancelado],
            [Finalizado] = [],
            [Cancelado] = []
        };

        public static string Normalizar(string? status)
        {
            var valor = (status ?? string.Empty).Trim().ToLowerInvariant();

            return valor switch
            {
                "pagamento pix pendente" or "pagamento pendente" or "pendente" => Pendente,
                "confirmado" => Confirmado,
                "em preparação" or "separado" => Separado,
                "em rota de entrega" or "em rota" or "em_rota" => EmRota,
                "finalizado" or "entregue" => Finalizado,
                "cancelado" => Cancelado,
                _ => Desconhecido
            };
        }

        public static int Prioridade(string? status) => Normalizar(status) switch
        {
            Pendente => 0,
            Confirmado => 1,
            Separado => 2,
            EmRota => 3,
            Finalizado => 4,
            Cancelado => 5,
            _ => 6
        };

        public static bool TransicaoPermitida(string? statusAtual, string novoStatus)
        {
            var atual = Normalizar(statusAtual);
            return TransicoesPermitidas.TryGetValue(atual, out var destinos) && destinos.Contains(novoStatus);
        }

        public static IReadOnlyList<string> ProximosStatus(string? statusAtual)
        {
            return TransicoesPermitidas.TryGetValue(Normalizar(statusAtual), out var destinos) ? destinos : [];
        }

        public static bool ClientePodeCancelar(string? statusAtual)
        {
            var atual = Normalizar(statusAtual);
            return atual is Pendente or Confirmado;
        }
    }
}
