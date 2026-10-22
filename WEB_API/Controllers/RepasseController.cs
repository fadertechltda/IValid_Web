using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using WEB_API.Infraestrutura;

namespace WEB_API.Controllers
{
    [Route("api/[controller]")]
    public class RepasseController(RepasseFachada repasseFachada) : ControllerApiBase
    {
        private readonly RepasseFachada _repasseFachada = repasseFachada;

        [HttpGet]
        public Task<IActionResult> Get([FromQuery] string supermercadoId) =>
            ExecutarAsync(async () => Ok(await _repasseFachada.ObterAsync(supermercadoId)));
    }
}
