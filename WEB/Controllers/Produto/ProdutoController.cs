using DOMAIN.Model.Produto;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace WEB.Controllers.Produto
{
    public class ProdutoController(IHttpClientFactory httpClientFactory) : Controller
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("IValidApi");
        private readonly string _apiUrl = "api/Produto";

        public async Task<IActionResult> Index()
        {
            List<ProdutoModel>? produtos = [];

            var response = await _httpClient.GetAsync(_apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                produtos = JsonSerializer.Deserialize<List<ProdutoModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                TempData["Erro"] = "Erro ao buscar os produtos da API.";
            }

            return View(produtos);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProdutoModel produto)
        {
            try
            {
                var json = JsonSerializer.Serialize(produto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Produto cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Erro retornado pela API: {errorMsg}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro de conexão: {ex.Message}");
            }

            return View(produto);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Erro"] = "Produto não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var produto = JsonSerializer.Deserialize<ProdutoModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(produto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProdutoModel produto)
        {
            if (id != produto.Id) return NotFound();

            try
            {
                var json = JsonSerializer.Serialize(produto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiUrl}/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Produto atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"\n[ERRO PUT API] StatusCode: {response.StatusCode}");
                Console.WriteLine($"[ERRO PUT API] ErrorMsg: {errorMsg}\n");
                ModelState.AddModelError(string.Empty, $"Erro retornado pela API: {(string.IsNullOrEmpty(errorMsg) ? "Mensagem vazia, olhe o console" : errorMsg)}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro de conexão: {ex.Message}");
            }

            return View(produto);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Erro"] = "Produto não encontrado na API.";
                return RedirectToAction(nameof(Index));
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var produto = JsonSerializer.Deserialize<ProdutoModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(produto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Produto deletado com sucesso!";
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["Erro"] = $"Erro na API ao deletar: {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro de conexão: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
