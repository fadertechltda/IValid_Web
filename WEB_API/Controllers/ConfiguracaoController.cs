using DOMAIN.Model.Configuracao;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using Excecoes;

namespace WEB_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracaoController(ConfiguracaoFachada configuracaoFachada, ILogger<ConfiguracaoController> logger) : ControllerBase
    {
        private readonly ConfiguracaoFachada _configuracaoFachada = configuracaoFachada;
        private readonly ILogger<ConfiguracaoController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string supermercadoId)
        {
            var configuracao = await _configuracaoFachada.ObterConfiguracao(supermercadoId);
            return Ok(configuracao);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ConfiguracaoModel configuracao)
        {
            try
            {
                await _configuracaoFachada.AtualizarConfiguracao(configuracao);
                return NoContent();
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar configurações");
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = "Ocorreu um erro inesperado ao salvar as configurações." });
            }
        }
    }
}
