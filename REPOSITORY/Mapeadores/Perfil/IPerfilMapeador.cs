using DOMAIN.Model.Perfil;

namespace REPOSITORY.Mapeadores.Perfil
{
    public interface IPerfilMapeador
    {
        Task<List<PerfilModel>> ListarPorSupermercadoAsync(string supermercadoId);
        Task<PerfilModel?> ObterPorIdAsync(string id);
        Task<bool> CriarSeNaoExistirAsync(PerfilModel perfil);
        Task<string> CriarAsync(PerfilModel perfil);
        Task AtualizarAsync(PerfilModel perfil);
        Task DeletarAsync(string id);
    }
}
