using DOMAIN.Model.Configuracao;
using REPOSITORY.Mapeadores.Configuracao;

namespace SERVICE.Processo
{
    public class ConfiguracaoProcesso(IConfiguracaoMapeador configuracaoMapeador)
    {
        private readonly IConfiguracaoMapeador _configuracaoMapeador = configuracaoMapeador;

        public async Task<ConfiguracaoModel> ObterConfiguracao()
        {
            var configuracao = await _configuracaoMapeador.ObterAsync();
            return configuracao ?? new ConfiguracaoModel();
        }

        public async Task SalvarConfiguracao(ConfiguracaoModel configuracao)
        {
            await _configuracaoMapeador.SalvarAsync(configuracao);
        }
    }
}
