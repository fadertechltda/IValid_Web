using DOMAIN.Model.Cliente;
using DOMAIN.Model.Pedido;
using DOMAIN.Model.Produto;

namespace REPOSITORY.Mapeadores.Transacao
{
    public interface ITransacaoMapeador
    {
        string GerarIdPedido();
        Task<T> ExecutarAsync<T>(Func<ITransacaoContexto, Task<T>> operacao);
    }

    public interface ITransacaoContexto
    {
        Task<ProdutoModel?> ObterProdutoAsync(string id);
        Task<ClienteModel?> ObterClienteAsync(string uid);
        Task<PedidoModel?> ObterPedidoAsync(string id);
        void AtualizarProduto(string id, Dictionary<string, object> campos);
        void MesclarCliente(string uid, Dictionary<string, object> campos);
        void CriarPedido(PedidoModel pedido);
        void AtualizarPedido(string id, Dictionary<string, object> campos);
    }
}
