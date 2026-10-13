using DOMAIN.Model.Perfil;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using WEB_API.Infraestrutura;

namespace WEB_API.Controllers
{
    [Route("api/[controller]")]
    public class PerfilController(PerfilFachada perfilFachada) : ControllerApiBase
    {
        private readonly PerfilFachada _perfilFachada = perfilFachada;

        [HttpGet]
        public Task<IActionResult> Listar([FromQuery] string supermercadoId) =>
            ExecutarAsync(async () => Ok(await _perfilFachada.ListarAsync(supermercadoId)));

        [HttpGet("permissoes")]
        public IActionResult ListarPermissoes() => Ok(PermissaoUtil.Todas);

        [HttpGet("{id}")]
        public Task<IActionResult> Obter(string id, [FromQuery] string supermercadoId) =>
            ExecutarAsync(async () =>
            {
                var perfil = await _perfilFachada.ObterAsync(id, supermercadoId);
                return perfil == null ? NotFound() : Ok(perfil);
            });

        [HttpPost]
        public Task<IActionResult> Criar([FromBody] PerfilModel perfil, [FromQuery] string supermercadoId) =>
            ExecutarAsync(async () => Ok(new { id = await _perfilFachada.CriarAsync(supermercadoId, perfil) }));

        [HttpPut("{id}")]
        public Task<IActionResult> Atualizar(string id, [FromBody] PerfilModel perfil, [FromQuery] string supermercadoId) =>
            ExecutarAsync(async () =>
            {
                perfil.Id = id;
                await _perfilFachada.AtualizarAsync(supermercadoId, perfil);
                return NoContent();
            });

        [HttpDelete("{id}")]
        public Task<IActionResult> Deletar(string id, [FromQuery] string supermercadoId) =>
            ExecutarAsync(async () =>
            {
                await _perfilFachada.DeletarAsync(id, supermercadoId);
                return NoContent();
            });
    }
}
