using DOMAIN.Model.Usuario;

namespace REPOSITORY.Mapeadores.Usuario
{
    public interface IUsuarioMapeador
    {
        Task<UsuarioModel?> ObterPorEmailAsync(string email);
        Task<UsuarioModel?> ObterPorIdAsync(string id);
        Task<UsuarioModel?> ObterPorSupermercadoIdAsync(string supermercadoId);
        Task CriarAsync(UsuarioModel usuario);
    }
}
