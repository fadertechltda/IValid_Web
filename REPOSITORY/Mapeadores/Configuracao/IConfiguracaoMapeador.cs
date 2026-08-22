using DOMAIN.Model.Configuracao;

namespace REPOSITORY.Mapeadores.Configuracao
{
    public interface IConfiguracaoMapeador
    {
        Task<ConfiguracaoModel?> ObterAsync(string supermercadoId);
        Task SalvarAsync(ConfiguracaoModel configuracao);
    }
}
