using DOMAIN.Model;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;

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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                ProdutoModel? produto = await _produtoFachada.ListarProdutoPorId(id);

                if (produto == null) return NotFound("Produto não encontrado.");

                await _produtoFachada.DeletarProdutos(produto);

                return Ok("Produto deletado com sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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

            if (produto == null) return NotFound("Produto não encontrado");

            return Ok(produto);
        }
    }
}

