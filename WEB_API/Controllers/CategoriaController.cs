using DOMAIN.Model.Categoria;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using WEB_API.Infraestrutura;

namespace WEB_API.Controllers
{
    [Route("api/[controller]")]
    public class CategoriaController(CategoriaFachada categoriaFachada) : ControllerApiBase
    {
        private readonly CategoriaFachada _categoriaFachada = categoriaFachada;

        [HttpGet]
        public Task<IActionResult> Listar() => ExecutarAsync(async () => Ok(await _categoriaFachada.ListarAsync()));

        [HttpGet("{id}")]
        public Task<IActionResult> Obter(string id) =>
            ExecutarAsync(async () =>
            {
                var categoria = await _categoriaFachada.ObterAsync(id);
                return categoria == null ? NotFound() : Ok(categoria);
            });

        [HttpPost]
        public Task<IActionResult> Criar([FromBody] CategoriaModel categoria) =>
            ExecutarAsync(async () => Ok(new { id = await _categoriaFachada.CriarAsync(categoria) }));

        [HttpPut("{id}")]
        public Task<IActionResult> Atualizar(string id, [FromBody] CategoriaModel categoria) =>
            ExecutarAsync(async () =>
            {
                categoria.Id = id;
                await _categoriaFachada.AtualizarAsync(categoria);
                return NoContent();
            });

        [HttpDelete("{id}")]
        public Task<IActionResult> Deletar(string id) =>
            ExecutarAsync(async () =>
            {
                await _categoriaFachada.DeletarAsync(id);
                return NoContent();
            });
    }
}
