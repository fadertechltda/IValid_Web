using DOMAIN.Model.Perfil;
using DOMAIN.Model.Categoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEB.Controllers
{
    [Authorize(Policy = PermissaoUtil.Categorias)]
    public class CategoriaController(IHttpClientFactory httpClientFactory, ILogger<CategoriaController> logger) : Controller
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("IValidApi");
        private readonly ILogger<CategoriaController> _logger = logger;
        private readonly string _apiUrl = "api/Categoria";

        public async Task<IActionResult> Index(string? editar)
        {
            var categorias = await ListarAsync();
            ViewData["Categorias"] = categorias;
            return View(categorias.FirstOrDefault(c => c.Id == editar) ?? new CategoriaModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salvar(CategoriaModel categoria)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Categorias"] = await ListarAsync();
                return View(nameof(Index), categoria);
            }

            try
            {
                var conteudo = CadastroApiHelper.ComoJson(categoria);
                var resposta = string.IsNullOrEmpty(categoria.Id)
                    ? await _httpClient.PostAsync(_apiUrl, conteudo)
                    : await _httpClient.PutAsync($"{_apiUrl}/{categoria.Id}", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Categoria salva com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, await CadastroApiHelper.ExtrairErroAsync(resposta, "Não foi possível salvar a categoria."));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao salvar categoria");
                ModelState.AddModelError(string.Empty, "Não foi possível conectar ao servidor. Tente novamente em instantes.");
            }

            ViewData["Categorias"] = await ListarAsync();
            return View(nameof(Index), categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(string id)
        {
            try
            {
                var resposta = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
                if (resposta.IsSuccessStatusCode) TempData["Sucesso"] = "Categoria excluída.";
                else TempData["Erro"] = await CadastroApiHelper.ExtrairErroAsync(resposta, "Não foi possível excluir a categoria.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao excluir categoria {Id}", id);
                TempData["Erro"] = "Não foi possível conectar ao servidor. Tente novamente em instantes.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<CategoriaModel>> ListarAsync()
        {
            try
            {
                var resposta = await _httpClient.GetAsync(_apiUrl);
                if (resposta.IsSuccessStatusCode) return await CadastroApiHelper.LerAsync<List<CategoriaModel>>(resposta) ?? [];
                TempData["Erro"] = "Erro ao buscar as categorias da API.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao listar categorias");
                TempData["Erro"] = "Não foi possível conectar ao servidor.";
            }

            return [];
        }
    }
}
