using DOMAIN.Model.Pedido;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
    }
}
