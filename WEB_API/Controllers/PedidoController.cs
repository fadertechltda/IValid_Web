using DOMAIN.Model.Pedido;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using Excecoes;

namespace WEB_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidoController(PedidoFachada pedidoFachada, ILogger<PedidoController> logger) : ControllerBase
    {
        private readonly PedidoFachada _pedidoFachada = pedidoFachada;
        private readonly ILogger<PedidoController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string supermercadoId)
        {
            var pedidos = await _pedidoFachada.ListarPedidos(supermercadoId);
            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, [FromQuery] string supermercadoId)
        {
            PedidoModel? pedido = await _pedidoFachada.ListarPedidoPorId(id, supermercadoId);

            if (pedido == null)
            {
                return NotFound(new ExcecaoDetalhes { Codigo = CodigoExcecao.EntidadeNaoEncontrada, InformacaoAdicional = "Pedido não encontrado" });
            }

            return Ok(pedido);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> AtualizarStatus(string id, [FromBody] AtualizarStatusPedidoModel modelo, [FromQuery] string supermercadoId)
        {
            try
            {
                var pedido = await _pedidoFachada.AtualizarStatusAsync(id, supermercadoId, modelo.Status);
                return Ok(pedido);
            }
            catch (IValidExcecao ex)
            {
                var detalhes = new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional };
                return ex.Codigo == CodigoExcecao.EntidadeNaoEncontrada ? NotFound(detalhes) : BadRequest(detalhes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar o status do pedido {PedidoId}", id);
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = "Ocorreu um erro inesperado ao atualizar o pedido." });
            }
        }
    }
}
