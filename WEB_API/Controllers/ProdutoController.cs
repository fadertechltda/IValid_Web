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
        public async Task<IActionResult> Post([FromBody] ProdutoModel produto, [FromQuery] string supermercadoId)
        {
            try
            {
                produto.SupermercadoId = supermercadoId;
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
        public async Task<IActionResult> Put([FromBody] ProdutoModel produto, [FromQuery] string supermercadoId)
        {
            try
            {
                ProdutoModel? produtoExistente = await _produtoFachada.ListarProdutoPorId(produto.Id!, supermercadoId);

                if (produtoExistente == null)
                {
                    throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Produto não encontrado.");
                }

                produto.SupermercadoId = supermercadoId;
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
        public async Task<IActionResult> Delete(string id, [FromQuery] string supermercadoId)
        {
            try
            {
                ProdutoModel? produto = await _produtoFachada.ListarProdutoPorId(id, supermercadoId);

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
        public async Task<IActionResult> Get([FromQuery] string supermercadoId)
        {
            var produtos = await _produtoFachada.ListarProdutos(supermercadoId);
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, [FromQuery] string supermercadoId)
        {
            ProdutoModel? produto = await _produtoFachada.ListarProdutoPorId(id, supermercadoId);

            if (produto == null)
            {
                return NotFound(new ExcecaoDetalhes { Codigo = CodigoExcecao.EntidadeNaoEncontrada, InformacaoAdicional = "Produto não encontrado" });
            }

            return Ok(produto);
        }
    }
}
