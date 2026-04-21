using DOMAIN.Model;
using REPOSITORY.Mapeadores.Produto;

namespace SERVICE.Processo
{
    public class ProdutoProcesso(IProdutoMapeador produtoMapeador)
    {
        private readonly IProdutoMapeador _produtoMapeador = produtoMapeador;

        public async Task CadastrarProduto(ProdutoModel produto)
        {
            CalcularStatusEPrecos(produto);
            await _produtoMapeador.CadastrarAsync(produto);
        }

        public async Task AtualizarProduto(ProdutoModel produto)
        {
            CalcularStatusEPrecos(produto);
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
                CalcularStatusEPrecos(produto);
            }

            return listaDeProdutos;
        }

        public async Task<ProdutoModel?> ListarProdutoPorId(string id)
        {
            var produto = await _produtoMapeador.ListarPorIdAsync(id);

            if (produto != null)
            {
                CalcularStatusEPrecos(produto);
            }

            return produto;
        }

        public void CalcularStatusEPrecos(ProdutoModel produto)
        {
            int diasParaVencer = (produto.DataVencimento - DateTime.Now.Date).Days;

            if (diasParaVencer <= 10)
            {
                produto.Status = "VERMELHO";
                produto.DescricaoPorcentual = 40;
                produto.PrecoPromocao = produto.Preco * 0.50;
            }
            else if (diasParaVencer <= 20)
            {
                produto.Status = "AMARELO";
                produto.DescricaoPorcentual = 20;
                produto.PrecoPromocao = produto.Preco * 0.80;
            }
            else
            {
                produto.Status = "VERDE";
                produto.DescricaoPorcentual = 0;
                produto.PrecoPromocao = produto.Preco;
            }
        }
    }
}
