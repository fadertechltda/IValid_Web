using DOMAIN.Model.Funcionario;
using DOMAIN.Model.Perfil;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Excecoes;
using Microsoft.AspNetCore.Authorization;

namespace WEB.Controllers.Funcionario
{
    [Authorize(Roles = "Administrador")]
    public class FuncionarioController(IHttpClientFactory httpClientFactory, ILogger<FuncionarioController> logger) : Controller
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("IValidApi");
        private readonly ILogger<FuncionarioController> _logger = logger;
        private readonly string _apiUrl = "api/Funcionario";

        private string SupermercadoId => User.FindFirst("SupermercadoId")?.Value ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            List<FuncionarioModel>? funcionarios = [];

            var response = await _httpClient.GetAsync($"{_apiUrl}?supermercadoId={SupermercadoId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                funcionarios = JsonSerializer.Deserialize<List<FuncionarioModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                TempData["Erro"] = "Erro ao buscar os funcionários da API.";
            }

            return View(funcionarios);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Perfis"] = await ListarPerfisAsync();
            return View(new CriarFuncionarioModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CriarFuncionarioModel modelo)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Perfis"] = await ListarPerfisAsync();
                return View(modelo);
            }

            try
            {
                var json = JsonSerializer.Serialize(modelo);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiUrl}?supermercadoId={SupermercadoId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Funcionário cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                var erroMsg = await ExtrairMensagemErro(response);
                ModelState.AddModelError(string.Empty, erroMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao criar funcionário");
                ModelState.AddModelError(string.Empty, "Não foi possível conectar ao servidor. Tente novamente em instantes.");
            }

            ViewData["Perfis"] = await ListarPerfisAsync();
            return View(modelo);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}?supermercadoId={SupermercadoId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Erro"] = "Funcionário não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var funcionario = JsonSerializer.Deserialize<FuncionarioModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (funcionario == null) return NotFound();

            var modelo = new AtualizarFuncionarioModel
            {
                Id = funcionario.Id,
                Nome = funcionario.Nome,
                PerfilId = funcionario.PerfilId,
                Ativo = funcionario.Ativo
            };

            ViewData["Perfis"] = await ListarPerfisAsync();
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AtualizarFuncionarioModel modelo)
        {
            if (id != modelo.Id) return NotFound();

            try
            {
                var json = JsonSerializer.Serialize(modelo);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiUrl}/{id}?supermercadoId={SupermercadoId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Sucesso"] = "Funcionário atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                var erroMsg = await ExtrairMensagemErro(response);
                ModelState.AddModelError(string.Empty, erroMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao editar funcionário {Id}", id);
                ModelState.AddModelError(string.Empty, "Não foi possível conectar ao servidor. Tente novamente em instantes.");
            }

            ViewData["Perfis"] = await ListarPerfisAsync();
            return View(modelo);
        }

        private async Task<List<PerfilModel>> ListarPerfisAsync()
        {
            try
            {
                var resposta = await _httpClient.GetAsync($"api/Perfil?supermercadoId={SupermercadoId}");
                if (resposta.IsSuccessStatusCode) return await CadastroApiHelper.LerAsync<List<PerfilModel>>(resposta) ?? [];
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API ao listar perfis");
            }

            TempData["Erro"] = "Não foi possível carregar os perfis.";
            return [];
        }

        private static async Task<string> ExtrairMensagemErro(HttpResponseMessage response)
        {
            var conteudo = await response.Content.ReadAsStringAsync();
            try
            {
                var detalhes = JsonSerializer.Deserialize<ExcecaoDetalhes>(conteudo, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return detalhes?.InformacaoAdicional ?? conteudo;
            }
            catch
            {
                return conteudo;
            }
        }
    }
}
