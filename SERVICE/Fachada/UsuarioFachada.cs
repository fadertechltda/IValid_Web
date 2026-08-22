using DOMAIN.Model.Funcionario;
using DOMAIN.Model.Supermercado;
using DOMAIN.Model.Usuario;
using Excecoes;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class UsuarioFachada(UsuarioProcesso usuarioProcesso, SupermercadoProcesso supermercadoProcesso, FuncionarioProcesso funcionarioProcesso, EmailProcesso emailProcesso)
    {
        private readonly UsuarioProcesso _usuarioProcesso = usuarioProcesso;
        private readonly SupermercadoProcesso _supermercadoProcesso = supermercadoProcesso;
        private readonly FuncionarioProcesso _funcionarioProcesso = funcionarioProcesso;
        private readonly EmailProcesso _emailProcesso = emailProcesso;

        public async Task<string> CriarAdministrador(RegistroModel registro)
        {
            UsuarioModel? existe = await _usuarioProcesso.ObterPorEmailAsync(registro.Email!);
            if (existe != null)
                throw new IValidExcecao(CodigoExcecao.ValidacaoSeguranca, "Email já cadastrado no sistema.");

            string supermercadoId = await _supermercadoProcesso.CriarSupermercado(new SupermercadoModel
            {
                Nome = registro.NomeSupermercado,
                Cnpj = registro.CnpjSupermercado,
                Endereco = registro.EnderecoSupermercado
            });

            UsuarioModel novoUsuario = new()
            {
                Email = registro.Email,
                NomeCompleto = registro.NomeCompleto,
                SupermercadoId = supermercadoId
            };

            await _usuarioProcesso.CriarAdministradorAsync(novoUsuario);

            SupermercadoModel? supermercado = await _supermercadoProcesso.ObterPorIdAsync(supermercadoId);
            string codigoAcesso = supermercado?.CodigoAcesso ?? string.Empty;

            if (!string.IsNullOrEmpty(codigoAcesso))
            {
                try
                {
                    await _emailProcesso.EnviarCodigoAcessoAsync(registro.Email!, registro.NomeSupermercado ?? "seu supermercado", codigoAcesso);
                }
                catch (Exception)
                {
                }
            }

            return codigoAcesso;
        }

        public async Task<List<UsuarioLoginModel>> ListarUsuariosParaLogin(string codigoLoja)
        {
            SupermercadoModel supermercado = await ObterSupermercadoPorCodigo(codigoLoja);

            List<UsuarioLoginModel> lista = [];

            UsuarioModel? administrador = await _usuarioProcesso.ObterPorSupermercadoIdAsync(supermercado.Id!);
            if (administrador != null)
            {
                lista.Add(new UsuarioLoginModel { Chave = $"ADMIN:{administrador.Id}", Nome = administrador.NomeCompleto ?? "Administrador" });
            }

            List<FuncionarioModel> funcionarios = await _funcionarioProcesso.ListarPorSupermercadoAsync(supermercado.Id!);
            foreach (FuncionarioModel funcionario in funcionarios.Where(f => f.Ativo))
            {
                lista.Add(new UsuarioLoginModel { Chave = $"FUNC:{funcionario.Id}", Nome = funcionario.Nome ?? "Funcionário" });
            }

            return lista;
        }

        public async Task<string> ObterEmailParaLoja(string usuarioId, string codigoLoja)
        {
            SupermercadoModel supermercado = await ObterSupermercadoPorCodigo(codigoLoja);

            UsuarioModel? usuario = await _usuarioProcesso.ObterPorIdAsync(usuarioId);

            if (usuario == null || usuario.SupermercadoId != supermercado.Id)
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Usuário não encontrado nesta loja.");

            return usuario.Email ?? string.Empty;
        }

        public async Task<ResultadoLoginModel> AutenticarAdministradorPorId(string usuarioId, string codigoLoja)
        {
            SupermercadoModel supermercado = await ObterSupermercadoPorCodigo(codigoLoja);

            UsuarioModel? usuario = await _usuarioProcesso.ObterPorIdAsync(usuarioId);

            if (usuario == null || usuario.SupermercadoId != supermercado.Id || usuario.Perfil != TipoUsuario.Administrador)
                throw new IValidExcecao(CodigoExcecao.ValidacaoSeguranca, "Acesso negado.");

            return new ResultadoLoginModel
            {
                Id = usuario.Id,
                Nome = usuario.NomeCompleto,
                Email = usuario.Email,
                Perfil = TipoUsuario.Administrador.ToString(),
                SupermercadoId = usuario.SupermercadoId
            };
        }

        public async Task<ResultadoLoginModel> AutenticarFuncionario(string funcionarioId, string senha, string codigoLoja)
        {
            SupermercadoModel supermercado = await ObterSupermercadoPorCodigo(codigoLoja);

            FuncionarioModel? funcionario = await _funcionarioProcesso.ListarPorIdAsync(funcionarioId);

            if (funcionario == null || funcionario.SupermercadoId != supermercado.Id || !funcionario.Ativo)
                throw new IValidExcecao(CodigoExcecao.ValidacaoSeguranca, "Acesso negado.");

            if (!FuncionarioProcesso.VerificarSenha(funcionario, senha))
                throw new IValidExcecao(CodigoExcecao.ValidacaoSeguranca, "Senha inválida.");

            return new ResultadoLoginModel
            {
                Id = funcionario.Id,
                Nome = funcionario.Nome,
                Email = null,
                Perfil = funcionario.Perfil.ToString(),
                SupermercadoId = funcionario.SupermercadoId
            };
        }

        private async Task<SupermercadoModel> ObterSupermercadoPorCodigo(string codigoLoja)
        {
            SupermercadoModel? supermercado = await _supermercadoProcesso.ObterPorCodigoAcessoAsync(codigoLoja);

            if (supermercado == null)
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Código da loja não encontrado.");

            return supermercado;
        }
    }
}
