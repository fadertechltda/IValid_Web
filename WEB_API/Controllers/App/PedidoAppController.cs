using DOMAIN.Model.Pedido;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using WEB_API.Infraestrutura;

namespace WEB_API.Controllers.App
{
    [Authorize]
    [Route("api/app/pedidos")]
    [Tags("App - Pedidos")]
    public class PedidoAppController(PedidoFachada pedidoFachada) : ControllerAppBase
    {
        private readonly PedidoFachada _pedidoFachada = pedidoFachada;

        [HttpPost("validar")]
        public Task<IActionResult> Validar([FromBody] CriarPedidoModel requisicao) =>
            ExecutarAsync(async () => Ok(await _pedidoFachada.SimularAsync(UsuarioId, requisicao)));

        [HttpPost]
        public Task<IActionResult> Criar([FromBody] CriarPedidoModel requisicao) =>
            ExecutarAsync(async () =>
            {
                var pedido = await _pedidoFachada.CriarAsync(UsuarioId, requisicao);
                return CreatedAtAction(nameof(Obter), new { id = pedido.Id }, pedido);
            });

        [HttpGet]
        public Task<IActionResult> Listar() =>
            ExecutarAsync(async () => Ok(await _pedidoFachada.ListarDoClienteAsync(UsuarioId)));

        [HttpGet("{id}")]
        public Task<IActionResult> Obter(string id) =>
            ExecutarAsync(async () => Ok(await _pedidoFachada.ObterDoClienteAsync(UsuarioId, id)));

        [HttpPost("{id}/cancelar")]
        public Task<IActionResult> Cancelar(string id) =>
            ExecutarAsync(async () => Ok(await _pedidoFachada.CancelarPeloClienteAsync(UsuarioId, id)));

        [HttpPost("{id}/pagamento/link")]
        public Task<IActionResult> GerarLinkPagamento(string id) =>
            ExecutarAsync(async () => Ok(await _pedidoFachada.GerarLinkPagamentoAsync(UsuarioId, id, NomeUsuario, EmailUsuario)));

        [HttpPost("{id}/pagamento/confirmar")]
        public Task<IActionResult> ConfirmarPagamento(string id, [FromBody] ConfirmarPagamentoModel dados) =>
            ExecutarAsync(async () => Ok(await _pedidoFachada.ConfirmarPagamentoAsync(UsuarioId, id, dados)));
    }
}
