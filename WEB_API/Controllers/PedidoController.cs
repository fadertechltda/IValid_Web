using DOMAIN.Model.Pedido;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using Excecoes;

namespace WEB_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidoController(PedidoFachada pedidoFachada) : ControllerBase
    {
        private readonly PedidoFachada _pedidoFachada = pedidoFachada;

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
    }
}
