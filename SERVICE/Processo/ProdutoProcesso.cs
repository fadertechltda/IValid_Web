using DOMAIN.Model;
using REPOSITORY.Mapeadores.Produto;

namespace SERVICE.Processo
{
    public class ProdutoProcesso(IProdutoMapeador produtoMapeador)
    {
        private readonly IProdutoMapeador _produtoMapeador = produtoMapeador;

        public async Task CadastrarProduto(ProdutoModel produto)
        {
            await _produtoMapeador.CadastrarAsync(produto);
        }

        public async Task AtualizarProduto(ProdutoModel produto)
        {
            await _produtoMapeador.AtualizarAsync(produto);
        }

        public async Task<List<ProdutoModel>> ListarProdutos()
        {
            var listaDeProdutos = await _produtoMapeador.ListarTodosAsync();

            return listaDeProdutos;
        }

        public async Task<ProdutoModel?> ListarProdutoPorId(string id)
        {
            var produto = await _produtoMapeador.ListarPorIdAsync(id);
            return produto;
        }
    }
}
