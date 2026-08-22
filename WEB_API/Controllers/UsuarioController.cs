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
    public class UsuarioController(
        UsuarioFachada usuarioFachada,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<UsuarioController> logger) : ControllerBase
    {
        private readonly UsuarioFachada _usuarioFachada = usuarioFachada;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<UsuarioController> _logger = logger;
        private readonly string? _firebaseApiKey = configuration["Firebase:ApiKey"];
        private readonly string? _codigoConviteAdmin = configuration["Seguranca:CodigoConviteAdmin"];

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

                using var clienteHttp = _httpClientFactory.CreateClient();
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
          
                _logger.LogError(ex, "Erro inesperado no login do usuário {Email}", login?.Email);
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = "Ocorreu um erro inesperado ao tentar autenticar. Tente novamente mais tarde." });
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
  
                if (string.IsNullOrEmpty(_codigoConviteAdmin) || registro.CodigoConvite != _codigoConviteAdmin)
                {
                    return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.NaoAutorizado, InformacaoAdicional = "Código de convite de administrador inválido." });
                }

                if (string.IsNullOrEmpty(_firebaseApiKey))
                {
                    throw new IValidExcecao(CodigoExcecao.Generico, "A 'ApiKey' do Firebase não está configurada no appsettings.json da API. A criação do usuário não pode ser feita.");
                }

                using var clienteHttp = _httpClientFactory.CreateClient();
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
                    var corpoErro = await resposta.Content.ReadAsStringAsync();
                    var codigoFirebase = ExtrairCodigoErroFirebase(corpoErro);

                    _logger.LogWarning("Firebase Auth recusou o signUp para {Email}: {CodigoFirebase} | Corpo: {Corpo}", registro.Email, codigoFirebase, corpoErro);

                    var mensagem = codigoFirebase switch
                    {
                        "EMAIL_EXISTS" => "Este email já está cadastrado.",
                        "INVALID_EMAIL" => "Formato de email inválido.",
                        "WEAK_PASSWORD : Password should be at least 6 characters" or "WEAK_PASSWORD" => "A senha é muito fraca. Use pelo menos 6 caracteres.",
                        "OPERATION_NOT_ALLOWED" => "O cadastro por email/senha não está habilitado no projeto Firebase. Ative o provedor \"Email/senha\" em Authentication > Sign-in method no console do Firebase.",
                        "TOO_MANY_ATTEMPTS_TRY_LATER" => "Muitas tentativas em pouco tempo. Aguarde alguns minutos e tente novamente.",
                        _ when codigoFirebase != null && codigoFirebase.Contains("API_KEY") => "A ApiKey do Firebase configurada não tem permissão para esta operação (verifique restrições da chave no Google Cloud Console).",
                        _ => "Não foi possível criar o usuário no Firebase Auth. Veja o log da API para o motivo detalhado."
                    };

                    throw new IValidExcecao(CodigoExcecao.ValidacaoSeguranca, mensagem);
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
                _logger.LogError(ex, "Erro inesperado no registro do usuário {Email}", registro?.Email);
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = "Ocorreu um erro inesperado ao criar a conta. Tente novamente mais tarde." });
            }
        }

        [HttpPost("esqueci-senha")]
        public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaModel modelo)
        {
            try
            {
                if (string.IsNullOrEmpty(modelo.Email))
                {
                    return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.ValidacaoSeguranca, InformacaoAdicional = "Email é obrigatório." });
                }

                if (string.IsNullOrEmpty(_firebaseApiKey))
                {
                    throw new IValidExcecao(CodigoExcecao.Generico, "A 'ApiKey' do Firebase não está configurada no appsettings.json da API.");
                }

                using var clienteHttp = _httpClientFactory.CreateClient();
                var corpoRequisicao = new
                {
                    requestType = "PASSWORD_RESET",
                    email = modelo.Email
                };

                var conteudo = new StringContent(JsonSerializer.Serialize(corpoRequisicao), Encoding.UTF8, "application/json");
                await clienteHttp.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_firebaseApiKey}", conteudo);

                return Ok();
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao solicitar redefinição de senha para {Email}", modelo?.Email);
                return Ok();
            }
        }

        private static string? ExtrairCodigoErroFirebase(string corpoResposta)
        {
            try
            {
                using var doc = JsonDocument.Parse(corpoResposta);
                if (doc.RootElement.TryGetProperty("error", out var erro) &&
                    erro.TryGetProperty("message", out var mensagem))
                {
                    return mensagem.GetString();
                }
            }
            catch
            {
                // Corpo não veio no formato esperado; seguimos com null e caímos na mensagem genérica.
            }

            return null;
        }
    }
}
