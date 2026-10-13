using Excecoes;
using Microsoft.AspNetCore.Mvc;
using SERVICE.Processo;

namespace WEB_API.Infraestrutura
{
    [ApiController]
    [Produces("application/json")]
    public abstract class ControllerApiBase : ControllerBase
    {
        protected async Task<IActionResult> ExecutarAsync(Func<Task<IActionResult>> acao)
        {
            try
            {
                return await acao();
            }
            catch (PedidoInvalidoExcecao ex)
            {
                return Conflict(new { codigo = ex.Codigo, informacaoAdicional = ex.InformacaoAdicional, resumo = ex.Resumo });
            }
            catch (IValidExcecao ex)
            {
                var detalhes = new ExcecaoDetalhes { Codigo = ex.Codigo, InformacaoAdicional = ex.InformacaoAdicional };

                return ex.Codigo switch
                {
                    CodigoExcecao.EntidadeNaoEncontrada => NotFound(detalhes),
                    CodigoExcecao.NaoAutorizado => StatusCode(StatusCodes.Status403Forbidden, detalhes),
                    CodigoExcecao.Concorrencia or CodigoExcecao.EstoqueInsuficiente or CodigoExcecao.TransicaoStatusInvalida or CodigoExcecao.PagamentoNaoConfirmado => Conflict(detalhes),
                    CodigoExcecao.Generico => StatusCode(StatusCodes.Status500InternalServerError, detalhes),
                    _ => BadRequest(detalhes)
                };
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
                logger.LogError(ex, "Erro inesperado em {Caminho}", HttpContext.Request.Path);

                return StatusCode(StatusCodes.Status500InternalServerError, new ExcecaoDetalhes
                {
                    Codigo = CodigoExcecao.Generico,
                    InformacaoAdicional = "Ocorreu um erro inesperado. Tente novamente mais tarde."
                });
            }
        }
    }

    public abstract class ControllerAppBase : ControllerApiBase
    {
        protected string UsuarioId => User.FindFirst("user_id")?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new IValidExcecao(CodigoExcecao.NaoAutorizado, "Token sem identificação do usuário.");

        protected string? EmailUsuario => User.FindFirst("email")?.Value;

        protected string? NomeUsuario => User.FindFirst("name")?.Value;
    }
}
