using DOMAIN.Model.Usuario;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using Excecoes;
using System.Text.Json;
using System.Text;

namespace WEB_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController(UsuarioFachada usuarioFachada, IConfiguration configuration) : ControllerBase
    {
        private readonly UsuarioFachada _usuarioFachada = usuarioFachada;
        private readonly string? _firebaseApiKey = configuration["Firebase:ApiKey"];

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            try
            {
                if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Senha))
                {
                    return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.ValidacaoSeguranca, InformacaoAdicional = "Email e senha são obrigatórios." });
                }

                if (string.IsNullOrEmpty(_firebaseApiKey))
                {
                    throw new IValidExcecao(CodigoExcecao.Generico, "A 'ApiKey' do Firebase não está configurada no appsettings.json da API. A validação de senha é impossível.");
                }

                using var clienteHttp = new HttpClient();
                var corpoRequisicao = new
                {
                    email = login.Email,
                    password = login.Senha,
                    returnSecureToken = true
                };

                var conteudo = new StringContent(JsonSerializer.Serialize(corpoRequisicao), Encoding.UTF8, "application/json");
                var resposta = await clienteHttp.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseApiKey}", conteudo);

                if (!resposta.IsSuccessStatusCode)
                {
                    throw new IValidExcecao(CodigoExcecao.ValidacaoSeguranca, "Email ou senha inválidos.");
                }

                UsuarioModel usuario = await _usuarioFachada.AutenticarAdministrador(login.Email);
                return Ok(usuario);
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = ex.Message });
            }
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroModel registro)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.ValidacaoSeguranca, InformacaoAdicional = "Dados inválidos." });
                }

                if (string.IsNullOrEmpty(_firebaseApiKey))
                {
                    throw new IValidExcecao(CodigoExcecao.Generico, "A 'ApiKey' do Firebase não está configurada no appsettings.json da API. A criação do usuário não pode ser feita.");
                }

                using var clienteHttp = new HttpClient();
                var corpoRequisicao = new
                {
                    email = registro.Email,
                    password = registro.Senha,
                    returnSecureToken = true
                };

                var conteudo = new StringContent(JsonSerializer.Serialize(corpoRequisicao), Encoding.UTF8, "application/json");
                var resposta = await clienteHttp.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_firebaseApiKey}", conteudo);

                if (!resposta.IsSuccessStatusCode)
                {
                    throw new IValidExcecao(CodigoExcecao.ValidacaoSeguranca, "Não foi possível criar o usuário no Firebase Auth.");
                }

                await _usuarioFachada.CriarAdministrador(registro);
                return Ok();
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = ex.Message });
            }
        }
    }
}
