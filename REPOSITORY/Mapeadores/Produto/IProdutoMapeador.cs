using DOMAIN.Interface;
using DOMAIN.Model.Produto;

namespace REPOSITORY.Mapeadores.Produto
{
    public interface IProdutoMapeador: IMapeador<ProdutoModel>
    {
        Task<List<ProdutoModel>> ListarPorSupermercadoAsync(string supermercadoId);
        Task<List<ProdutoModel>> ListarPorIdsAsync(IEnumerable<string> ids);
        Task AtualizarCamposAsync(string id, Dictionary<string, object> campos);
    }
}
