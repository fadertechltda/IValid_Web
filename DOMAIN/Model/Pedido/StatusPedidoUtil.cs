namespace DOMAIN.Model.Pedido
{
    public static class StatusPedidoUtil
    {
        public static string Normalizar(string? status)
        {
            var valor = (status ?? string.Empty).Trim().ToLowerInvariant();

            return valor switch
            {
                "pagamento pix pendente" or "pagamento pendente" or "pendente" => "PENDENTE",
                "confirmado" => "CONFIRMADO",
                "em preparação" or "separado" => "SEPARADO",
                "em rota de entrega" or "em rota" => "EM_ROTA",
                "finalizado" or "entregue" => "FINALIZADO",
                "cancelado" => "CANCELADO",
                _ => "DESCONHECIDO"
            };
        }

        public static int Prioridade(string? status) => Normalizar(status) switch
        {
            "PENDENTE" => 0,
            "CONFIRMADO" => 1,
            "SEPARADO" => 2,
            "EM_ROTA" => 3,
            "FINALIZADO" => 4,
            "CANCELADO" => 5,
            _ => 6
        };
    }
}
