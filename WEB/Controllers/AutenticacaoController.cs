using DOMAIN.Model.Usuario;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace WEB.Controllers
{
    [AllowAnonymous]
    public class AutenticacaoController(IHttpClientFactory httpClientFactory, ILogger<AutenticacaoController> logger) : Controller
    {
        private readonly HttpClient _clienteHttp = httpClientFactory.CreateClient("IValidApi");
        private readonly ILogger<AutenticacaoController> _logger = logger;

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel modeloLogin, string etapa)
        {
            if (string.IsNullOrEmpty(modeloLogin.CodigoLoja))
            {
                ModelState.AddModelError(string.Empty, "Informe o código da loja.");
                return View(modeloLogin);
            }

            if (etapa == "buscar" || string.IsNullOrEmpty(modeloLogin.UsuarioChave))
            {
                ModelState.Remove(nameof(LoginModel.UsuarioChave));
                ModelState.Remove(nameof(LoginModel.Senha));
                return await CarregarUsuariosDaLoja(modeloLogin);
            }

            if (!ModelState.IsValid)
            {
                return await CarregarUsuariosDaLoja(modeloLogin);
            }

            try
            {
                var textoJson = JsonSerializer.Serialize(modeloLogin);
                var conteudo = new StringContent(textoJson, Encoding.UTF8, "application/json");

                var resposta = await _clienteHttp.PostAsync("api/Usuario/login-loja", conteudo);
                var respostaJson = await resposta.Content.ReadAsStringAsync();

                if (resposta.IsSuccessStatusCode)
                {
                    var resultado = JsonSerializer.Deserialize<ResultadoLoginModel>(respostaJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (resultado != null)
                    {
                        var reivindicacoes = new List<Claim>
                        {
                            new(ClaimTypes.Name, resultado.Nome ?? "Usuário"),
                            new(ClaimTypes.Email, resultado.Email ?? ""),
                            new(ClaimTypes.Role, resultado.Perfil ?? ""),
                            new(ClaimTypes.NameIdentifier, resultado.Id ?? ""),
                            new("SupermercadoId", resultado.SupermercadoId ?? "")
                        };

                        var identidadeReivindicacoes = new ClaimsIdentity(reivindicacoes, CookieAuthenticationDefaults.AuthenticationScheme);

                        var propriedadesAutenticacao = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(identidadeReivindicacoes),
                            propriedadesAutenticacao);

                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError(string.Empty, "Não foi possível processar a resposta do servidor.");
                }
                else
                {
                    var erroMsg = ExtrairMensagemErro(respostaJson);
                    ModelState.AddModelError(string.Empty, erroMsg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao tentar autenticar na loja {CodigoLoja}", modeloLogin.CodigoLoja);
                ModelState.AddModelError(string.Empty, "Não foi possível conectar ao servidor. Tente novamente em instantes.");
            }

            return await CarregarUsuariosDaLoja(modeloLogin);
        }

        private async Task<IActionResult> CarregarUsuariosDaLoja(LoginModel modeloLogin)
        {
            try
            {
                var resposta = await _clienteHttp.GetAsync($"api/Usuario/usuarios-loja?codigoLoja={modeloLogin.CodigoLoja}");

                if (!resposta.IsSuccessStatusCode)
                {
                    var erroMsg = await ExtrairMensagemErroAsync(resposta);
                    ModelState.AddModelError(string.Empty, erroMsg);
                    return View("Login", modeloLogin);
                }

                var json = await resposta.Content.ReadAsStringAsync();
                var usuarios = JsonSerializer.Deserialize<List<UsuarioLoginModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

                if (usuarios.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "Nenhum usuário encontrado para esta loja.");
                    return View("Login", modeloLogin);
                }

                ViewData["Usuarios"] = usuarios;
                ViewData["LojaSelecionada"] = true;
                return View("Login", modeloLogin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao buscar usuários da loja {CodigoLoja}", modeloLogin.CodigoLoja);
                ModelState.AddModelError(string.Empty, "Não foi possível conectar ao servidor. Tente novamente em instantes.");
                return View("Login", modeloLogin);
            }
        }

        [HttpGet]
        public IActionResult Registro()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegistroModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroModel modeloRegistro)
        {
            if (!ModelState.IsValid)
            {
                return View(modeloRegistro);
            }

            try
            {
                var textoJson = JsonSerializer.Serialize(modeloRegistro);
                var conteudo = new StringContent(textoJson, Encoding.UTF8, "application/json");

                var resposta = await _clienteHttp.PostAsync("api/Usuario/registro", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Conta criada com sucesso! Enviamos o código de acesso da sua loja para o e-mail informado. Verifique sua caixa de entrada (e o spam) para fazer login.";
                    return RedirectToAction("Login");
                }

                var erroMsg = await ExtrairMensagemErroAsync(resposta);
                ModelState.AddModelError(string.Empty, erroMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao tentar registrar {Email}", modeloRegistro.Email);
                ModelState.AddModelError(string.Empty, "Não foi possível conectar ao servidor. Tente novamente em instantes.");
            }

            return View(modeloRegistro);
        }

        [HttpGet]
        public IActionResult EsqueciSenha()
        {
            return View(new EsqueciSenhaModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EsqueciSenha(EsqueciSenhaModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            try
            {
                var textoJson = JsonSerializer.Serialize(modelo);
                var conteudo = new StringContent(textoJson, Encoding.UTF8, "application/json");
                await _clienteHttp.PostAsync("api/Usuario/esqueci-senha", conteudo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao solicitar redefinição de senha para {Email}", modelo.Email);
            }

            TempData["Sucesso"] = "Se o email informado estiver cadastrado, você receberá um link para redefinir sua senha.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Autenticacao");
        }

        private static string ExtrairMensagemErro(string conteudo)
        {
            try
            {
                using var doc = JsonDocument.Parse(conteudo);
                var root = doc.RootElement;
                if (root.TryGetProperty("informacaoAdicional", out var msgElement) ||
                    root.TryGetProperty("InformacaoAdicional", out msgElement))
                {
                    return msgElement.GetString() ?? "Ocorreu um erro ao processar a requisição.";
                }

                if (root.TryGetProperty("title", out var titleElement))
                {
                    return titleElement.GetString() ?? "Ocorreu um erro de validação.";
                }

                return "Ocorreu um erro inesperado no servidor.";
            }
            catch
            {
                return "Falha na comunicação com a API.";
            }
        }

        private static async Task<string> ExtrairMensagemErroAsync(HttpResponseMessage resposta)
        {
            var conteudo = await resposta.Content.ReadAsStringAsync();
            return ExtrairMensagemErro(conteudo);
        }
    }
}
