using DOMAIN.Interface;
using DOMAIN.Model.Produto;

namespace REPOSITORY.Mapeadores.Produto
{
    public interface IProdutoMapeador: IMapeador<ProdutoModel>
    {
        Task<List<ProdutoModel>> ListarPorSupermercadoAsync(string supermercadoId);
    }
}
