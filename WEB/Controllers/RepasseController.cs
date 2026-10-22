using System.Text.Json;
using DOMAIN.Model.Perfil;
using DOMAIN.Model.Repasse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEB.Controllers
{
    [Authorize(Policy = PermissaoUtil.Financeiro)]
    public class RepasseController(IHttpClientFactory httpClientFactory, ILogger<RepasseController> logger) : Controller
    {
        private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNameCaseInsensitive = true };

        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("IValidApi");
        private readonly ILogger<RepasseController> _logger = logger;
        private readonly string _apiUrl = "api/Repasse";

        private string SupermercadoId => User.FindFirst("SupermercadoId")?.Value ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var resumo = new ResumoRepasseModel();

            try
            {
                var resposta = await _httpClient.GetAsync($"{_apiUrl}?supermercadoId={Uri.EscapeDataString(SupermercadoId)}");

                if (resposta.IsSuccessStatusCode)
                {
                    var json = await resposta.Content.ReadAsStringAsync();
                    resumo = JsonSerializer.Deserialize<ResumoRepasseModel>(json, OpcoesJson) ?? new ResumoRepasseModel();
                }
                else
                {
                    _logger.LogWarning("API de repasses respondeu HTTP {Status}", (int)resposta.StatusCode);
                    TempData["Erro"] = "Não foi possível carregar os repasses.";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Falha ao consultar a API de repasses");
                TempData["Erro"] = "Não foi possível conectar ao servidor. Tente novamente em instantes.";
            }

            return View(resumo);
        }
    }
}
