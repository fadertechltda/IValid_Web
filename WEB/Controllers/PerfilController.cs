using DOMAIN.Model.Perfil;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEB.Controllers
{
    [Authorize(Roles = PermissaoUtil.PerfilAdministrador)]
    public class PerfilController(IHttpClientFactory httpClientFactory, ILogger<PerfilController> logger) : Controller
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("IValidApi");
        private readonly ILogger<PerfilController> _logger = logger;
        private readonly string _apiUrl = "api/Perfil";

        private string SupermercadoId => User.FindFirst("SupermercadoId")?.Value ?? string.Empty;

        public async Task<IActionResult> Index(string? editar)
        {
            var perfis = await ListarAsync();
            ViewData["Perfis"] = perfis;
            return View(perfis.FirstOrDefault(p => p.Id == editar) ?? new PerfilModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salvar(PerfilModel perfil)
        {
            perfil.Permissoes ??= [];

            if (!ModelState.IsValid)
            {
                ViewData["Perfis"] = await ListarAsync();
                return View(nameof(Index), perfil);
            }

            try
            {
                var conteudo = CadastroApiHelper.ComoJson(perfil);
                var resposta = string.IsNullOrEmpty(perfil.Id)
                    ? await _httpClient.PostAsync($"{_apiUrl}?supermercadoId={SupermercadoId}", conteudo)
                    : await _httpClient.PutAsync($"{_apiUrl}/{perfil.Id}?supermercadoId={SupermercadoId}", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Perfil salvo. Funcionários com este perfil recebem as novas permissões no próximo login.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, await CadastroApiHelper.ExtrairErroAsync(resposta, "Não foi possível salvar o perfil."));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao salvar perfil");
                ModelState.AddModelError(string.Empty, "Não foi possível conectar ao servidor. Tente novamente em instantes.");
            }

            ViewData["Perfis"] = await ListarAsync();
            return View(nameof(Index), perfil);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(string id)
        {
            try
            {
                var resposta = await _httpClient.DeleteAsync($"{_apiUrl}/{id}?supermercadoId={SupermercadoId}");
                if (resposta.IsSuccessStatusCode) TempData["Sucesso"] = "Perfil excluído.";
                else TempData["Erro"] = await CadastroApiHelper.ExtrairErroAsync(resposta, "Não foi possível excluir o perfil.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao excluir perfil {Id}", id);
                TempData["Erro"] = "Não foi possível conectar ao servidor. Tente novamente em instantes.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<PerfilModel>> ListarAsync()
        {
            try
            {
                var resposta = await _httpClient.GetAsync($"{_apiUrl}?supermercadoId={SupermercadoId}");
                if (resposta.IsSuccessStatusCode) return await CadastroApiHelper.LerAsync<List<PerfilModel>>(resposta) ?? [];
                TempData["Erro"] = "Erro ao buscar os perfis da API.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao listar perfis");
                TempData["Erro"] = "Não foi possível conectar ao servidor.";
            }

            return [];
        }
    }
}
