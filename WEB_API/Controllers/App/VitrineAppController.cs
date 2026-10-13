using DOMAIN.Model.Vitrine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using WEB_API.Infraestrutura;

namespace WEB_API.Controllers.App
{
    [AllowAnonymous]
    [Route("api/app")]
    [Tags("App - Vitrine")]
    public class VitrineAppController(VitrineFachada vitrineFachada) : ControllerAppBase
    {
        private readonly VitrineFachada _vitrineFachada = vitrineFachada;

        [HttpGet("produtos")]
        public Task<IActionResult> ListarProdutos([FromQuery] FiltroVitrineModel filtro) =>
            ExecutarAsync(async () => Ok(await _vitrineFachada.ListarProdutosAsync(filtro)));

        [HttpGet("produtos/ofertas-relampago")]
        public Task<IActionResult> ListarOfertasRelampago([FromQuery] FiltroVitrineModel filtro) =>
            ExecutarAsync(async () =>
            {
                filtro.Status = "VERMELHO";
                return Ok(await _vitrineFachada.ListarProdutosAsync(filtro));
            });

        [HttpGet("produtos/{id}")]
        public Task<IActionResult> ObterProduto(string id) =>
            ExecutarAsync(async () =>
            {
                var produto = await _vitrineFachada.ObterProdutoAsync(id);
                return produto == null ? NotFound() : Ok(produto);
            });

        [HttpGet("categorias")]
        public Task<IActionResult> ListarCategorias() =>
            ExecutarAsync(async () => Ok(await _vitrineFachada.ListarCategoriasAsync()));

        [HttpGet("supermercados")]
        public Task<IActionResult> ListarSupermercados() =>
            ExecutarAsync(async () => Ok(await _vitrineFachada.ListarSupermercadosAsync()));

        [HttpGet("supermercados/{id}")]
        public Task<IActionResult> ObterSupermercado(string id) =>
            ExecutarAsync(async () =>
            {
                var supermercado = await _vitrineFachada.ObterSupermercadoAsync(id);
                return supermercado == null ? NotFound() : Ok(supermercado);
            });

        [HttpGet("ongs")]
        public Task<IActionResult> ListarOngs() =>
            ExecutarAsync(async () => Ok(await _vitrineFachada.ListarOngsAtivasAsync()));
    }
}
