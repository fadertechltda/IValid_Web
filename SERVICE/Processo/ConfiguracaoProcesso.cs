using DOMAIN.Model.Configuracao;
using REPOSITORY.Mapeadores.Configuracao;

namespace SERVICE.Processo
{
    public class ConfiguracaoProcesso(IConfiguracaoMapeador configuracaoMapeador)
    {
        private readonly IConfiguracaoMapeador _configuracaoMapeador = configuracaoMapeador;

        public async Task<ConfiguracaoModel> ObterConfiguracao(string supermercadoId)
        {
            var configuracao = await _configuracaoMapeador.ObterAsync(supermercadoId);
            return configuracao ?? new ConfiguracaoModel { SupermercadoId = supermercadoId };
        }

        public async Task SalvarConfiguracao(ConfiguracaoModel configuracao)
        {
            await _configuracaoMapeador.SalvarAsync(configuracao);
        }
    }
}
