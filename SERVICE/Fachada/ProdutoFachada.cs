using DOMAIN.Model;
using DOMAIN.Validador.Produto;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class ProdutoFachada(ProdutoProcesso produtoProcesso, ProdutoValidacao produtoValidacao)
    {
        private readonly ProdutoProcesso _produtoProcesso = produtoProcesso;
        private readonly ProdutoValidacao _validador = produtoValidacao;
        
        public async Task CadastrarProdutos(ProdutoModel produto)
        {
            _produtoProcesso.CalcularStatusEPrecos(produto);

            _validador.AssineRegrasInclusao();

            var resultado = await _validador.ValidateAsync(produto);

            if (!resultado.IsValid)
            {
                throw new Exception(resultado.Errors.First().ErrorMessage);
            }

            await _produtoProcesso.CadastrarProduto(produto);
        }

        public async Task AtualizarProdutos(ProdutoModel produto)
        {
            _produtoProcesso.CalcularStatusEPrecos(produto);

            _validador.AssineRegrasAtualizacao();

            var resultado = await _validador.ValidateAsync(produto);

            if (!resultado.IsValid)
            {
                throw new Exception(resultado.Errors.First().ErrorMessage);
            }

            await _produtoProcesso.AtualizarProduto(produto);
        }

        public async Task DeletarProdutos(ProdutoModel produto)
        {
            _validador.AssineRegrasExclusao();

            var resultado = await _validador.ValidateAsync(produto);

            if (!resultado.IsValid)
            {
                throw new Exception(resultado.Errors.First().ErrorMessage);
            }
            await _produtoProcesso.DeletarProduto(produto);
        }

        public async Task<List<ProdutoModel>> ListarProdutos()
        {  
            return await _produtoProcesso.ListarProdutos();
        }

        public async Task<ProdutoModel?> ListarProdutoPorId(string id)
        {           
            if (string.IsNullOrEmpty(id)) return null;
            return await _produtoProcesso.ListarProdutoPorId(id);
        }
    }
}
