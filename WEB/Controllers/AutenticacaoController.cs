using DOMAIN.Model.Usuario;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace WEB.Controllers
{
    public class AutenticacaoController(IHttpClientFactory httpClientFactory) : Controller
    {
        private readonly HttpClient _clienteHttp = httpClientFactory.CreateClient("IValidApi");
        private readonly string _urlApi = "api/Usuario/login";

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

                var resposta = await _clienteHttp.PostAsync(_urlApi, conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    var respostaJson = await resposta.Content.ReadAsStringAsync();
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
                }

                var erroMsg = await ExtrairMensagemErro(resposta);
                ModelState.AddModelError(string.Empty, erroMsg);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro de conexão: {ex.Message}");
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

                var erroMsg = await ExtrairMensagemErro(resposta);
                ModelState.AddModelError(string.Empty, erroMsg);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro de conexão: {ex.Message}");
            }

            return View(modeloRegistro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Autenticacao");
        }

        private static async Task<string> ExtrairMensagemErro(HttpResponseMessage resposta)
        {
            var conteudo = await resposta.Content.ReadAsStringAsync();
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
                var msgRaw = conteudo.Length > 200 ? conteudo.Substring(0, 200) + "..." : conteudo;
                return $"Falha na comunicação com a API. Retorno bruto: {msgRaw}";
            }
        }
    }
}
