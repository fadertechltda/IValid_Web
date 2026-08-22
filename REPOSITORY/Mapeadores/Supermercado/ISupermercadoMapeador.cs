using DOMAIN.Model.Supermercado;

namespace REPOSITORY.Mapeadores.Supermercado
{
    public interface ISupermercadoMapeador
    {
        Task<string> CriarAsync(SupermercadoModel supermercado);
        Task<SupermercadoModel?> ListarPorIdAsync(string id);
        Task<SupermercadoModel?> ObterPorCodigoAcessoAsync(string codigoAcesso);
    }
}
