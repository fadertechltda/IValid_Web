using DOMAIN.Model.Pedido;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class PedidoFachada(PedidoProcesso pedidoProcesso)
    {
        private readonly PedidoProcesso _pedidoProcesso = pedidoProcesso;

        public async Task<List<PedidoModel>> ListarPedidos(string supermercadoId)
        {
            return await _pedidoProcesso.ListarPedidos(supermercadoId);
        }

        public async Task<PedidoModel?> ListarPedidoPorId(string id, string supermercadoId)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return await _pedidoProcesso.ListarPedidoPorId(id, supermercadoId);
        }
    }
}
