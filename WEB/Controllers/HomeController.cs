using DOMAIN.Model.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WEB.Controllers
{
    [Authorize]
    public class HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger) : Controller
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("IValidApi");
        private readonly ILogger<HomeController> _logger = logger;
        private readonly string _apiUrl = "api/Dashboard";

        private string SupermercadoId => User.FindFirst("SupermercadoId")?.Value ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            DashboardModel dashboard = new();

            try
            {
                var resposta = await _httpClient.GetAsync($"{_apiUrl}?supermercadoId={SupermercadoId}");

                if (resposta.IsSuccessStatusCode)
                {
                    var json = await resposta.Content.ReadAsStringAsync();
                    dashboard = JsonSerializer.Deserialize<DashboardModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new DashboardModel();
                }
                else
                {
                    TempData["Erro"] = "Não foi possível carregar os dados do painel.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao buscar o dashboard");
                TempData["Erro"] = "Não foi possível conectar ao servidor.";
            }

            return View(dashboard);
        }
    }
}
