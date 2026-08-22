using DOMAIN.Model.Configuracao;

namespace REPOSITORY.Mapeadores.Configuracao
{
    public interface IConfiguracaoMapeador
    {
        Task<ConfiguracaoModel?> ObterAsync();
        Task SalvarAsync(ConfiguracaoModel configuracao);
    }
}
