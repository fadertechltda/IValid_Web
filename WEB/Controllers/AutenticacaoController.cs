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
    // Esta é a única entrada pública do painel: todo o resto exige login,
    // por causa do AuthorizeFilter global registrado em Program.cs.
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
        public async Task<IActionResult> Login(LoginModel modeloLogin)
        {
            if (!ModelState.IsValid)
            {
                return View(modeloLogin);
            }

            try
            {
                var textoJson = JsonSerializer.Serialize(modeloLogin);
                var conteudo = new StringContent(textoJson, Encoding.UTF8, "application/json");

                var resposta = await _clienteHttp.PostAsync("api/Usuario/login", conteudo);
                var respostaJson = await resposta.Content.ReadAsStringAsync();

                if (resposta.IsSuccessStatusCode)
                {
                    var usuario = JsonSerializer.Deserialize<UsuarioModel>(respostaJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (usuario != null)
                    {
                        var reivindicacoes = new List<Claim>
                        {
                            new(ClaimTypes.Name, usuario.NomeCompleto ?? usuario.Email ?? "Usuário"),
                            new(ClaimTypes.Email, usuario.Email ?? ""),
                            new(ClaimTypes.Role, usuario.Perfil.ToString()),
                            new(ClaimTypes.NameIdentifier, usuario.Id ?? "")
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

                    // A API respondeu 2xx mas não veio um usuário reconhecível: trata como erro.
                    ModelState.AddModelError(string.Empty, "Não foi possível processar a resposta do servidor.");
                    return View(modeloLogin);
                }

                var erroMsg = ExtrairMensagemErro(respostaJson);
                ModelState.AddModelError(string.Empty, erroMsg);
            }
            catch (Exception ex)
            {
                // Detalhe completo só no log do servidor; o usuário recebe uma mensagem genérica,
                // para não expor detalhes internos (ex: motivo de falha de conexão com a API).
                _logger.LogError(ex, "Erro de conexão com a API ao tentar autenticar {Email}", modeloLogin.Email);
                ModelState.AddModelError(string.Empty, "Não foi possível conectar ao servidor. Tente novamente em instantes.");
            }

            return View(modeloLogin);
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
                    TempData["Sucesso"] = "Conta criada com sucesso! Faça login.";
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

            // Mensagem sempre igual, mesmo em caso de falha: não confirmamos nem negamos
            // se o email existe no sistema (evita que alguém use esta tela para descobrir
            // quais contas existem).
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
