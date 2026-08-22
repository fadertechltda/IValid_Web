using DOMAIN.Model.Configuracao;
using DOMAIN.Validador.Configuracao;
using Excecoes;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class ConfiguracaoFachada(ConfiguracaoProcesso configuracaoProcesso, ConfiguracaoValidacao validador)
    {
        private readonly ConfiguracaoProcesso _configuracaoProcesso = configuracaoProcesso;
        private readonly ConfiguracaoValidacao _validador = validador;

        public async Task<ConfiguracaoModel> ObterConfiguracao()
        {
            return await _configuracaoProcesso.ObterConfiguracao();
        }

        public async Task AtualizarConfiguracao(ConfiguracaoModel configuracao)
        {
            _validador.AssineRegrasAtualizacao();

            var resultado = await _validador.ValidateAsync(configuracao);

            if (!resultado.IsValid)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, resultado.Errors.First().ErrorMessage);
            }

            await _configuracaoProcesso.SalvarConfiguracao(configuracao);
        }
    }
}
