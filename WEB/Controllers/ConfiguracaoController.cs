using DOMAIN.Model.Configuracao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Excecoes;
using System.Text;
using System.Text.Json;

namespace WEB.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ConfiguracaoController(IHttpClientFactory httpClientFactory, ILogger<ConfiguracaoController> logger) : Controller
    {
        private readonly HttpClient _clienteHttp = httpClientFactory.CreateClient("IValidApi");
        private readonly ILogger<ConfiguracaoController> _logger = logger;
        private readonly string _apiUrl = "api/Configuracao";

        private string SupermercadoId => User.FindFirst("SupermercadoId")?.Value ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            ConfiguracaoModel? configuracao = null;

            try
            {
                var resposta = await _clienteHttp.GetAsync($"{_apiUrl}?supermercadoId={SupermercadoId}");

                if (resposta.IsSuccessStatusCode)
                {
                    var json = await resposta.Content.ReadAsStringAsync();
                    configuracao = JsonSerializer.Deserialize<ConfiguracaoModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    TempData["Erro"] = "Não foi possível carregar as configurações da API.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao buscar configurações");
                TempData["Erro"] = "Não foi possível conectar ao servidor.";
            }

            return View(configuracao ?? new ConfiguracaoModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ConfiguracaoModel configuracao)
        {
            try
            {
                configuracao.SupermercadoId = SupermercadoId;
                var textoJson = JsonSerializer.Serialize(configuracao);
                var conteudo = new StringContent(textoJson, Encoding.UTF8, "application/json");

                var resposta = await _clienteHttp.PutAsync(_apiUrl, conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Configurações salvas com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                var erroMsg = await ExtrairMensagemErroAsync(resposta);
                ModelState.AddModelError(string.Empty, erroMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao salvar configurações");
                ModelState.AddModelError(string.Empty, "Não foi possível conectar ao servidor. Tente novamente em instantes.");
            }

            return View(configuracao);
        }

        private static async Task<string> ExtrairMensagemErroAsync(HttpResponseMessage resposta)
        {
            var conteudo = await resposta.Content.ReadAsStringAsync();
            try
            {
                var detalhes = JsonSerializer.Deserialize<ExcecaoDetalhes>(conteudo, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return detalhes?.InformacaoAdicional ?? "Ocorreu um erro ao salvar as configurações.";
            }
            catch
            {
                return "Falha na comunicação com a API.";
            }
        }
    }
}
