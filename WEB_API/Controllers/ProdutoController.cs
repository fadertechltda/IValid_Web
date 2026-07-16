using DOMAIN.Model.Produto;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using Excecoes;

namespace WEB_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController(ProdutoFachada produtoFachada) : ControllerBase
    {
        private readonly ProdutoFachada _produtoFachada = produtoFachada;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProdutoModel produto)
        {
            try
            {
                await _produtoFachada.CadastrarProdutos(produto);
                return Created();
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromBody] ProdutoModel produto)
        {
            try
            {
                await _produtoFachada.AtualizarProdutos(produto);
                return NoContent();
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                ProdutoModel? produto = await _produtoFachada.ListarProdutoPorId(id);

                if (produto == null) 
                {
                    throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Produto não encontrado.");
                }

                await _produtoFachada.DeletarProdutos(produto);

                return Ok("Produto deletado com sucesso");
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var produtos = await _produtoFachada.ListarProdutos();
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            ProdutoModel? produto = await _produtoFachada.ListarProdutoPorId(id);

            if (produto == null)
            {
                return NotFound(new ExcecaoDetalhes { Codigo = CodigoExcecao.EntidadeNaoEncontrada, InformacaoAdicional = "Produto não encontrado" });
            }

            return Ok(produto);
        }
    }
}
