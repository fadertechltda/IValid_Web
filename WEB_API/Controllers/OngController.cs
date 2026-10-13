using DOMAIN.Model.Ong;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using WEB_API.Infraestrutura;

namespace WEB_API.Controllers
{
    [Route("api/[controller]")]
    public class OngController(OngFachada ongFachada) : ControllerApiBase
    {
        private readonly OngFachada _ongFachada = ongFachada;

        [HttpGet]
        public Task<IActionResult> Listar() => ExecutarAsync(async () => Ok(await _ongFachada.ListarAsync()));

        [HttpGet("{id}")]
        public Task<IActionResult> Obter(string id) =>
            ExecutarAsync(async () =>
            {
                var ong = await _ongFachada.ObterAsync(id);
                return ong == null ? NotFound() : Ok(ong);
            });

        [HttpPost]
        public Task<IActionResult> Criar([FromBody] OngModel ong) =>
            ExecutarAsync(async () => Ok(new { id = await _ongFachada.CriarAsync(ong) }));

        [HttpPut("{id}")]
        public Task<IActionResult> Atualizar(string id, [FromBody] OngModel ong) =>
            ExecutarAsync(async () =>
            {
                ong.Id = id;
                await _ongFachada.AtualizarAsync(ong);
                return NoContent();
            });

        [HttpDelete("{id}")]
        public Task<IActionResult> Deletar(string id) =>
            ExecutarAsync(async () =>
            {
                await _ongFachada.DeletarAsync(id);
                return NoContent();
            });
    }
}
