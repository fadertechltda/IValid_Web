using DOMAIN.Model.Cliente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using WEB_API.Infraestrutura;

namespace WEB_API.Controllers.App
{
    [Authorize]
    [Route("api/app/clientes")]
    [Tags("App - Cliente")]
    public class ClienteAppController(ClienteFachada clienteFachada) : ControllerAppBase
    {
        private readonly ClienteFachada _clienteFachada = clienteFachada;

        [HttpPost]
        public Task<IActionResult> Criar([FromBody] CriarClienteModel dados) =>
            ExecutarAsync(async () => Ok(await _clienteFachada.CriarPerfilAsync(UsuarioId, EmailUsuario, dados)));

        [HttpGet("me")]
        public Task<IActionResult> Obter() =>
            ExecutarAsync(async () => Ok(await _clienteFachada.ObterPerfilAsync(UsuarioId)));

        [HttpPut("me")]
        public Task<IActionResult> Atualizar([FromBody] AtualizarClienteModel dados) =>
            ExecutarAsync(async () => Ok(await _clienteFachada.AtualizarPerfilAsync(UsuarioId, dados)));

        [HttpGet("me/impacto")]
        public Task<IActionResult> ObterImpacto() =>
            ExecutarAsync(async () => Ok(await _clienteFachada.ObterImpactoAsync(UsuarioId)));

        [HttpGet("me/favoritos")]
        public Task<IActionResult> ListarFavoritos() =>
            ExecutarAsync(async () => Ok(await _clienteFachada.ListarFavoritosAsync(UsuarioId)));

        [HttpPut("me/favoritos/{produtoId}")]
        public Task<IActionResult> AdicionarFavorito(string produtoId) =>
            ExecutarAsync(async () =>
            {
                await _clienteFachada.AdicionarFavoritoAsync(UsuarioId, produtoId);
                return NoContent();
            });

        [HttpDelete("me/favoritos/{produtoId}")]
        public Task<IActionResult> RemoverFavorito(string produtoId) =>
            ExecutarAsync(async () =>
            {
                await _clienteFachada.RemoverFavoritoAsync(UsuarioId, produtoId);
                return NoContent();
            });

        [HttpPost("me/dispositivos")]
        public Task<IActionResult> RegistrarDispositivo([FromBody] TokenDispositivoModel dados) =>
            ExecutarAsync(async () =>
            {
                await _clienteFachada.RegistrarDispositivoAsync(UsuarioId, dados.Token!);
                return NoContent();
            });

        [HttpPost("me/dispositivos/remover")]
        public Task<IActionResult> RemoverDispositivo([FromBody] TokenDispositivoModel dados) =>
            ExecutarAsync(async () =>
            {
                await _clienteFachada.RemoverDispositivoAsync(UsuarioId, dados.Token!);
                return NoContent();
            });
    }
}
