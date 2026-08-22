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

        [HttpGet("usuarios-loja")]
        public async Task<IActionResult> UsuariosLoja([FromQuery] string codigoLoja)
        {
            try
            {
                var usuarios = await _usuarioFachada.ListarUsuariosParaLogin(codigoLoja);
                return Ok(usuarios);
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao listar usuários da loja {CodigoLoja}", codigoLoja);
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = "Não foi possível carregar os usuários desta loja." });
            }
        }

        [HttpPost("login-loja")]
        public async Task<IActionResult> LoginLoja([FromBody] LoginModel login)
        {
            try
            {
                if (string.IsNullOrEmpty(login.CodigoLoja) || string.IsNullOrEmpty(login.UsuarioChave) || string.IsNullOrEmpty(login.Senha))
                {
                    return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.ValidacaoSeguranca, InformacaoAdicional = "Selecione o usuário e informe a senha." });
                }

                string[] partesChave = login.UsuarioChave.Split(':', 2);
                if (partesChave.Length != 2)
                {
                    return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.ValidacaoSeguranca, InformacaoAdicional = "Usuário selecionado é inválido." });
                }

                string tipo = partesChave[0];
                string id = partesChave[1];
                ResultadoLoginModel resultado;

                if (tipo == "ADMIN")
                {
                    string email = await _usuarioFachada.ObterEmailParaLoja(id, login.CodigoLoja);

                    if (string.IsNullOrEmpty(_firebaseApiKey))
                    {
                        throw new IValidExcecao(CodigoExcecao.Generico, "A 'ApiKey' do Firebase não está configurada no appsettings.json da API. A validação de senha é impossível.");
                    }

                    using var clienteHttp = _httpClientFactory.CreateClient();
                    var corpoRequisicao = new
                    {
                        email,
                        password = login.Senha,
                        returnSecureToken = true
                    };

                    var conteudo = new StringContent(JsonSerializer.Serialize(corpoRequisicao), Encoding.UTF8, "application/json");
                    var resposta = await clienteHttp.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseApiKey}", conteudo);

                    if (!resposta.IsSuccessStatusCode)
                    {
                        throw new IValidExcecao(CodigoExcecao.ValidacaoSeguranca, "Senha inválida.");
                    }

                    resultado = await _usuarioFachada.AutenticarAdministradorPorId(id, login.CodigoLoja);
                }
                else
                {
                    resultado = await _usuarioFachada.AutenticarFuncionario(id, login.Senha, login.CodigoLoja);
                }

                return Ok(resultado);
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no login da loja {CodigoLoja}", login?.CodigoLoja);
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

                var conteudoSignUp = await resposta.Content.ReadAsStringAsync();
                using var documentoSignUp = JsonDocument.Parse(conteudoSignUp);
                var idTokenCriado = documentoSignUp.RootElement.TryGetProperty("idToken", out var elementoIdToken) ? elementoIdToken.GetString() : null;

                try
                {
                    string codigoAcesso = await _usuarioFachada.CriarAdministrador(registro);
                    return Ok(new { CodigoAcesso = codigoAcesso });
                }
                catch
                {
                    await ReverterUsuarioFirebaseAsync(clienteHttp, idTokenCriado, registro.Email);
                    throw;
                }
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

        private async Task ReverterUsuarioFirebaseAsync(HttpClient clienteHttp, string? idToken, string? email)
        {
            if (string.IsNullOrEmpty(idToken))
            {
                return;
            }

            try
            {
                var corpoExclusao = new { idToken };
                var conteudoExclusao = new StringContent(JsonSerializer.Serialize(corpoExclusao), Encoding.UTF8, "application/json");
                await clienteHttp.PostAsync($"https://identitytoolkit.googleapis.com/v1/accounts:delete?key={_firebaseApiKey}", conteudoExclusao);
            }
            catch (Exception exExclusao)
            {
                _logger.LogError(exExclusao, "Falha ao reverter usuário Firebase órfão para {Email}", email);
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
            }

            return null;
        }
    }
}
