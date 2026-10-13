using DOMAIN.Model.Ong;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEB.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class OngController(IHttpClientFactory httpClientFactory, ILogger<OngController> logger) : Controller
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("IValidApi");
        private readonly ILogger<OngController> _logger = logger;
        private readonly string _apiUrl = "api/Ong";

        public async Task<IActionResult> Index(string? editar)
        {
            var ongs = await ListarAsync();
            ViewData["Ongs"] = ongs;
            return View(ongs.FirstOrDefault(o => o.Id == editar) ?? new OngModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salvar(OngModel ong)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Ongs"] = await ListarAsync();
                return View(nameof(Index), ong);
            }

            try
            {
                var conteudo = CadastroApiHelper.ComoJson(ong);
                var resposta = string.IsNullOrEmpty(ong.Id)
                    ? await _httpClient.PostAsync(_apiUrl, conteudo)
                    : await _httpClient.PutAsync($"{_apiUrl}/{ong.Id}", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "ONG salva com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, await CadastroApiHelper.ExtrairErroAsync(resposta, "Não foi possível salvar a ONG."));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao salvar ONG");
                ModelState.AddModelError(string.Empty, "Não foi possível conectar ao servidor. Tente novamente em instantes.");
            }

            ViewData["Ongs"] = await ListarAsync();
            return View(nameof(Index), ong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(string id)
        {
            try
            {
                var resposta = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
                if (resposta.IsSuccessStatusCode) TempData["Sucesso"] = "ONG excluída.";
                else TempData["Erro"] = await CadastroApiHelper.ExtrairErroAsync(resposta, "Não foi possível excluir a ONG.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao excluir ONG {Id}", id);
                TempData["Erro"] = "Não foi possível conectar ao servidor. Tente novamente em instantes.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<OngModel>> ListarAsync()
        {
            try
            {
                var resposta = await _httpClient.GetAsync(_apiUrl);
                if (resposta.IsSuccessStatusCode) return await CadastroApiHelper.LerAsync<List<OngModel>>(resposta) ?? [];
                TempData["Erro"] = "Erro ao buscar as ONGs da API.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao listar ONGs");
                TempData["Erro"] = "Não foi possível conectar ao servidor.";
            }

            return [];
        }
    }
}
