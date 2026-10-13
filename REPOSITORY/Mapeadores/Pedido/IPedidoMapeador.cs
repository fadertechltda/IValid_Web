using DOMAIN.Model.Pedido;

namespace REPOSITORY.Mapeadores.Pedido
{
    public interface IPedidoMapeador
    {
        Task<List<PedidoModel>> ListarTodosAsync();
        Task<PedidoModel?> ListarPorIdAsync(string id);
        Task<List<PedidoModel>> ListarPorSupermercadoAsync(string supermercadoId);
        Task<List<PedidoModel>> ListarSemSupermercadoAsync();
        Task<List<PedidoModel>> ListarPorUsuarioAsync(string usuarioId);
        Task<List<PedidoModel>> ListarPorStatusAsync(string status);
    }
}
