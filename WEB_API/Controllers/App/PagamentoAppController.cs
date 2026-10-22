using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using WEB_API.Infraestrutura;

namespace WEB_API.Controllers.App
{
    [Authorize]
    [Route("api/app/pagamentos")]
    [Tags("App - Pagamentos")]
    public class PagamentoAppController(PedidoFachada pedidoFachada) : ControllerAppBase
    {
        private readonly PedidoFachada _pedidoFachada = pedidoFachada;

        /// <summary>Public Key do Mercado Pago, usada pelo app para gerar o token do cartão. Não é segredo.</summary>
        [HttpGet("mercadopago/configuracao")]
        public Task<IActionResult> ObterConfiguracao() =>
            ExecutarAsync(() => Task.FromResult<IActionResult>(Ok(new { publicKey = _pedidoFachada.ObterChavePublicaMercadoPago() })));
    }
}
