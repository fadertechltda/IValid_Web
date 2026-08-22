using DOMAIN.Model.Produto;
using DOMAIN.Model.Configuracao;
using REPOSITORY.Mapeadores.Produto;

namespace SERVICE.Processo
{
    public class ProdutoProcesso(IProdutoMapeador produtoMapeador, ConfiguracaoProcesso configuracaoProcesso)
    {
        private readonly IProdutoMapeador _produtoMapeador = produtoMapeador;
        private readonly ConfiguracaoProcesso _configuracaoProcesso = configuracaoProcesso;

        public async Task CadastrarProduto(ProdutoModel produto)
        {
            await CalcularStatusEPrecosAsync(produto);
            await _produtoMapeador.CadastrarAsync(produto);
        }

        public async Task AtualizarProduto(ProdutoModel produto)
        {
            await CalcularStatusEPrecosAsync(produto);
            await _produtoMapeador.AtualizarAsync(produto);
        }

        public async Task DeletarProduto(ProdutoModel produto)
        {
            await _produtoMapeador.DeletarAsync(produto);
        }

        public async Task<List<ProdutoModel>> ListarProdutos()
        {
            var listaDeProdutos = await _produtoMapeador.ListarTodosAsync();

            var configuracao = await _configuracaoProcesso.ObterConfiguracao();

            foreach (var produto in listaDeProdutos)
            {
                AplicarRegras(produto, configuracao);
            }

            return listaDeProdutos;
        }

        public async Task<ProdutoModel?> ListarProdutoPorId(string id)
        {
            var produto = await _produtoMapeador.ListarPorIdAsync(id);

            if (produto != null)
            {
                await CalcularStatusEPrecosAsync(produto);
            }

            return produto;
        }

        public async Task CalcularStatusEPrecosAsync(ProdutoModel produto)
        {
            var configuracao = await _configuracaoProcesso.ObterConfiguracao();
            AplicarRegras(produto, configuracao);
        }

        private static void AplicarRegras(ProdutoModel produto, ConfiguracaoModel configuracao)
        {
            int diasParaVencer = (produto.DataVencimento.Date - DateTime.UtcNow.Date).Days;
            produto.DiaValidade = diasParaVencer > 0 ? diasParaVencer : 0;

            if (diasParaVencer < 0)
            {
                produto.Status = "VENCIDO";
                produto.DescricaoPorcentual = 0;
                produto.PrecoPromocao = produto.Preco;
            }
            else if (diasParaVencer <= configuracao.DiasAlertaVermelho)
            {
                produto.Status = "VERMELHO";
                produto.DescricaoPorcentual = configuracao.PercentualDescontoVermelho;
                produto.PrecoPromocao = CalcularPrecoComDesconto(produto.Preco, configuracao.PercentualDescontoVermelho);
            }
            else if (diasParaVencer <= configuracao.DiasAlertaAmarelo)
            {
                produto.Status = "AMARELO";
                produto.DescricaoPorcentual = configuracao.PercentualDescontoAmarelo;
                produto.PrecoPromocao = CalcularPrecoComDesconto(produto.Preco, configuracao.PercentualDescontoAmarelo);
            }
            else
            {
                produto.Status = "VERDE";
                produto.DescricaoPorcentual = 0;
                produto.PrecoPromocao = produto.Preco;
            }
        }

        private static double CalcularPrecoComDesconto(double preco, int percentualDesconto)
        {
            return preco * (1 - percentualDesconto / 100.0);
        }
    }
}
