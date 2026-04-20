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

        public async Task DeletarProduto(ProdutoModel produto)
        {
            await _produtoMapeador.DeletarAsync(produto);
        }

        public async Task<List<ProdutoModel>> ListarProdutos()
        {
            var listaDeProdutos = await _produtoMapeador.ListarTodosAsync();

            foreach(var produto in listaDeProdutos)
            {
                ProcessarVencimentoEStatus(produto);
            }

            return listaDeProdutos;
        }

        public async Task<ProdutoModel?> ListarProdutoPorId(string id)
        {
            var produto = await _produtoMapeador.ListarPorIdAsync(id);
            return produto;
        }

        private static void ProcessarVencimentoEStatus(ProdutoModel produto)
        {
            int diasParaVencer = (produto.DataVencimento - DateTime.Now.Date).Days;

            if (diasParaVencer <= 10)
            {
                produto.Status = "Vermelho";
                produto.DescricaoPorcentual = 40;
                produto.PrecoPromocao = produto.Preco * 0.50;
            }
            else if (diasParaVencer <= 20)
            {
                produto.Status = "Amarelo";
                produto.DescricaoPorcentual = 20;
                produto.PrecoPromocao = produto.Preco * 0.20;
            }
            else
            {
                produto.Status = "Verde";
                produto.DescricaoPorcentual = 0;
                produto.PrecoPromocao = produto.Preco;
            }
        }
    }
}
