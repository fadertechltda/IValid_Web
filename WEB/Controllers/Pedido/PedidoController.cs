using DOMAIN.Model.Pedido;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Excecoes;
using Microsoft.AspNetCore.Authorization;

namespace WEB.Controllers.Pedido
{
    [Authorize(Roles = "Administrador,Gerente,Atendente")]
    public class PedidoController(IHttpClientFactory httpClientFactory) : Controller
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("IValidApi");
        private readonly string _apiUrl = "api/Pedido";

        private string SupermercadoId => User.FindFirst("SupermercadoId")?.Value ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            List<PedidoModel>? pedidos = [];

            var response = await _httpClient.GetAsync($"{_apiUrl}?supermercadoId={SupermercadoId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                pedidos = JsonSerializer.Deserialize<List<PedidoModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                TempData["Erro"] = "Erro ao buscar os pedidos da API.";
            }

            return View(pedidos);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}?supermercadoId={SupermercadoId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Erro"] = "Pedido não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var pedido = JsonSerializer.Deserialize<PedidoModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarStatus(string id, string status)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var json = JsonSerializer.Serialize(new AtualizarStatusPedidoModel { Status = status });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PatchAsync($"{_apiUrl}/{id}/status?supermercadoId={SupermercadoId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Status do pedido atualizado.";
                }
                else
                {
                    var conteudo = await response.Content.ReadAsStringAsync();
                    string mensagem;

                    try
                    {
                        mensagem = JsonSerializer.Deserialize<ExcecaoDetalhes>(conteudo, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.InformacaoAdicional
                            ?? "Não foi possível atualizar o status do pedido.";
                    }
                    catch (JsonException)
                    {
                        mensagem = "Não foi possível atualizar o status do pedido.";
                    }

                    TempData["Erro"] = mensagem;
                }
            }
            catch (HttpRequestException)
            {
                TempData["Erro"] = "Não foi possível conectar ao servidor. Tente novamente em instantes.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
