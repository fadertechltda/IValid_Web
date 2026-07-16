using DOMAIN.Model.Usuario;
using REPOSITORY.Mapeadores.Usuario;

namespace SERVICE.Processo
{
    public class UsuarioProcesso(IUsuarioMapeador usuarioMapeador)
    {
        private readonly IUsuarioMapeador _usuarioMapeador = usuarioMapeador;

        public async Task<UsuarioModel?> ObterPorEmailAsync(string email)
        {
            return await _usuarioMapeador.ObterPorEmailAsync(email);
        }

        public async Task<UsuarioModel?> ObterPorIdAsync(string id)
        {
            return await _usuarioMapeador.ObterPorIdAsync(id);
        }

        public async Task CriarAdministradorAsync(UsuarioModel usuario)
        {
            usuario.Perfil = TipoUsuario.Administrador;
            usuario.DataCriacao = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await _usuarioMapeador.CriarAsync(usuario);
        }
    }
}
