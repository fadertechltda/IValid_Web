using DOMAIN.Model.Perfil;
using DOMAIN.Model.Configuracao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Excecoes;
using System.Globalization;
using System.Text;
using DOMAIN.Model.Vitrine;
using System.Text.Json;

namespace WEB.Controllers
{
    [Authorize(Policy = PermissaoUtil.Configuracoes)]
    public class ConfiguracaoController(IHttpClientFactory httpClientFactory, ILogger<ConfiguracaoController> logger) : Controller
    {
        private readonly HttpClient _clienteHttp = httpClientFactory.CreateClient("IValidApi");
        private readonly ILogger<ConfiguracaoController> _logger = logger;
        private readonly string _apiUrl = "api/Configuracao";
        private readonly string _apiUrlSupermercado = "api/Supermercado";

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

            ViewData["DadosLoja"] = await ObterDadosLojaAsync();
            return View(configuracao ?? new ConfiguracaoModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DadosLoja(string? nome, string? endereco, string? latitude, string? longitude)
        {
            double? lat = LerCoordenada(latitude);
            double? lng = LerCoordenada(longitude);

            if ((!string.IsNullOrWhiteSpace(latitude) && lat == null) || (!string.IsNullOrWhiteSpace(longitude) && lng == null))
            {
                TempData["Erro"] = "Latitude e longitude devem ser números (ex.: -19.9681).";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var dados = new DadosLojaModel { Nome = nome, Endereco = endereco, Latitude = lat, Longitude = lng };
                var conteudo = new StringContent(JsonSerializer.Serialize(dados), Encoding.UTF8, "application/json");
                var resposta = await _clienteHttp.PutAsync($"{_apiUrlSupermercado}/{SupermercadoId}", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Dados da loja salvos com sucesso!";
                }
                else
                {
                    TempData["Erro"] = await ExtrairMensagemErroAsync(resposta);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao salvar os dados da loja");
                TempData["Erro"] = "Não foi possível conectar ao servidor. Tente novamente em instantes.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<DadosLojaModel> ObterDadosLojaAsync()
        {
            try
            {
                var resposta = await _clienteHttp.GetAsync($"{_apiUrlSupermercado}/{SupermercadoId}");

                if (resposta.IsSuccessStatusCode)
                {
                    var json = await resposta.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<DadosLojaModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new DadosLojaModel();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao buscar os dados da loja");
            }

            return new DadosLojaModel();
        }

        private static double? LerCoordenada(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            return double.TryParse(valor.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var numero) ? numero : null;
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

            ViewData["DadosLoja"] = await ObterDadosLojaAsync();
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
