using DOMAIN.Model.Ong;

namespace REPOSITORY.Mapeadores.Ong
{
    public interface IOngMapeador
    {
        Task<List<OngModel>> ListarTodasAsync();
        Task<OngModel?> ObterPorIdAsync(string id);
        Task<string> CriarAsync(OngModel ong);
        Task AtualizarAsync(OngModel ong);
        Task DeletarAsync(string id);
    }
}
