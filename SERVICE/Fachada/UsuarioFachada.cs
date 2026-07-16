using DOMAIN.Model.Usuario;
using Excecoes;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class UsuarioFachada(UsuarioProcesso usuarioProcesso)
    {
        private readonly UsuarioProcesso _usuarioProcesso = usuarioProcesso;

        public async Task<UsuarioModel> AutenticarAdministrador(string email)
        {
            UsuarioModel? usuario = await _usuarioProcesso.ObterPorEmailAsync(email);

            if (usuario == null)
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Usuário não encontrado.");

            if (usuario.Perfil != TipoUsuario.Administrador)
                throw new IValidExcecao(CodigoExcecao.ValidacaoSeguranca, "Acesso negado. Somente administradores podem acessar o painel.");

            return usuario;
        }

        public async Task CriarAdministrador(RegistroModel registro)
        {
            UsuarioModel? existe = await _usuarioProcesso.ObterPorEmailAsync(registro.Email!);
            if (existe != null)
                throw new IValidExcecao(CodigoExcecao.ValidacaoSeguranca, "Email já cadastrado no sistema.");

            UsuarioModel novoUsuario = new()
            {
                Email = registro.Email,
                NomeCompleto = registro.NomeCompleto
            };

            await _usuarioProcesso.CriarAdministradorAsync(novoUsuario);
        }
    }
}
