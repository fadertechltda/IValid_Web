using DOMAIN.Model.Pedido;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class PedidoFachada(PedidoProcesso pedidoProcesso, PagamentoProcesso pagamentoProcesso)
    {
        private readonly PedidoProcesso _pedidoProcesso = pedidoProcesso;
        private readonly PagamentoProcesso _pagamentoProcesso = pagamentoProcesso;

        public async Task<List<PedidoModel>> ListarPedidos(string supermercadoId)
        {
            return await _pedidoProcesso.ListarPedidos(supermercadoId);
        }

        public async Task<PedidoModel?> ListarPedidoPorId(string id, string supermercadoId)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return await _pedidoProcesso.ListarPedidoPorId(id, supermercadoId);
        }

        public Task<PedidoModel> AtualizarStatusAsync(string id, string supermercadoId, string? status) => _pedidoProcesso.AtualizarStatusPelaLojaAsync(id, supermercadoId, status);

        public Task<ResumoPedidoModel> SimularAsync(string uid, CriarPedidoModel requisicao) => _pedidoProcesso.SimularAsync(uid, requisicao);

        public Task<PedidoModel> CriarAsync(string uid, CriarPedidoModel requisicao) => _pedidoProcesso.CriarAsync(uid, requisicao);

        public Task<List<PedidoModel>> ListarDoClienteAsync(string uid) => _pedidoProcesso.ListarDoClienteAsync(uid);

        public Task<PedidoModel> ObterDoClienteAsync(string uid, string id) => _pedidoProcesso.ObterDoClienteAsync(uid, id);

        public Task<PedidoModel> CancelarPeloClienteAsync(string uid, string id) => _pedidoProcesso.CancelarPeloClienteAsync(uid, id);

        public Task<LinkPagamentoModel> GerarLinkPagamentoAsync(string uid, string id, string? nome, string? email) => _pagamentoProcesso.GerarLinkAsync(uid, id, nome, email);

        public Task<PedidoModel> ConfirmarPagamentoAsync(string uid, string id, ConfirmarPagamentoModel dados) => _pagamentoProcesso.ConfirmarAsync(uid, id, dados);
    }
}
