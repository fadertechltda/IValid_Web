using DOMAIN.Model.Pedido;

namespace REPOSITORY.Mapeadores.Pedido
{
    public interface IPedidoMapeador
    {
        Task<List<PedidoModel>> ListarTodosAsync();
        Task<PedidoModel?> ListarPorIdAsync(string id);
    }
}
