using DOMAIN.Model.Vitrine;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using WEB_API.Infraestrutura;

namespace WEB_API.Controllers
{
    [Route("api/[controller]")]
    public class SupermercadoController(SupermercadoFachada supermercadoFachada) : ControllerApiBase
    {
        private readonly SupermercadoFachada _supermercadoFachada = supermercadoFachada;

        [HttpGet("{id}")]
        public Task<IActionResult> Obter(string id) =>
            ExecutarAsync(async () =>
            {
                var dados = await _supermercadoFachada.ObterDadosLojaAsync(id);
                return dados == null ? NotFound() : Ok(dados);
            });

        [HttpPut("{id}")]
        public Task<IActionResult> Atualizar(string id, [FromBody] DadosLojaModel dados) =>
            ExecutarAsync(async () =>
            {
                await _supermercadoFachada.AtualizarDadosLojaAsync(id, dados);
                return NoContent();
            });
    }
}
