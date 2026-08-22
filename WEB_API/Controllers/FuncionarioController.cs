using DOMAIN.Model.Funcionario;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;
using Excecoes;

namespace WEB_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuncionarioController(FuncionarioFachada funcionarioFachada) : ControllerBase
    {
        private readonly FuncionarioFachada _funcionarioFachada = funcionarioFachada;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string supermercadoId)
        {
            var funcionarios = await _funcionarioFachada.ListarPorSupermercado(supermercadoId);
            return Ok(funcionarios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, [FromQuery] string supermercadoId)
        {
            FuncionarioModel? funcionario = await _funcionarioFachada.ListarPorId(id);

            if (funcionario == null || funcionario.SupermercadoId != supermercadoId)
            {
                return NotFound(new ExcecaoDetalhes { Codigo = CodigoExcecao.EntidadeNaoEncontrada, InformacaoAdicional = "Funcionário não encontrado." });
            }

            return Ok(funcionario);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CriarFuncionarioModel modelo, [FromQuery] string supermercadoId)
        {
            try
            {
                FuncionarioModel funcionario = new()
                {
                    Nome = modelo.Nome,
                    Perfil = modelo.Perfil,
                    SupermercadoId = supermercadoId
                };

                await _funcionarioFachada.CriarFuncionario(funcionario, modelo.Senha!);
                return Created();
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] AtualizarFuncionarioModel modelo, [FromQuery] string supermercadoId)
        {
            try
            {
                FuncionarioModel? funcionarioExistente = await _funcionarioFachada.ListarPorId(id);

                if (funcionarioExistente == null || funcionarioExistente.SupermercadoId != supermercadoId)
                {
                    throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Funcionário não encontrado.");
                }

                funcionarioExistente.Nome = modelo.Nome;
                funcionarioExistente.Perfil = modelo.Perfil;
                funcionarioExistente.Ativo = modelo.Ativo;

                await _funcionarioFachada.AtualizarFuncionario(funcionarioExistente, modelo.NovaSenha);
                return NoContent();
            }
            catch (IValidExcecao ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional });
            }
            catch (Exception ex)
            {
                return BadRequest(new ExcecaoDetalhes { Codigo = CodigoExcecao.Generico, InformacaoAdicional = ex.Message });
            }
        }
    }
}
