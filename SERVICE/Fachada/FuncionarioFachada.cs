using DOMAIN.Model.Funcionario;
using DOMAIN.Validador.Funcionario;
using Excecoes;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class FuncionarioFachada(FuncionarioProcesso funcionarioProcesso, PerfilProcesso perfilProcesso, FuncionarioValidacao validador)
    {
        private readonly FuncionarioProcesso _funcionarioProcesso = funcionarioProcesso;
        private readonly PerfilProcesso _perfilProcesso = perfilProcesso;
        private readonly FuncionarioValidacao _validador = validador;

        public async Task CriarFuncionario(FuncionarioModel funcionario, string senha)
        {
            _validador.AssineRegrasInclusao();

            var resultado = await _validador.ValidateAsync(funcionario);

            if (!resultado.IsValid)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, resultado.Errors.First().ErrorMessage);
            }

            var perfil = await _perfilProcesso.ObterObrigatorioAsync(funcionario.PerfilId, funcionario.SupermercadoId!);
            funcionario.PerfilString = perfil.Nome;

            await _funcionarioProcesso.CriarFuncionario(funcionario, senha);
        }

        public async Task AtualizarFuncionario(FuncionarioModel funcionario, string? novaSenha)
        {
            _validador.AssineRegrasAtualizacao();

            var resultado = await _validador.ValidateAsync(funcionario);

            if (!resultado.IsValid)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, resultado.Errors.First().ErrorMessage);
            }

            var perfil = await _perfilProcesso.ObterObrigatorioAsync(funcionario.PerfilId, funcionario.SupermercadoId!);
            funcionario.PerfilString = perfil.Nome;

            await _funcionarioProcesso.AtualizarFuncionario(funcionario, novaSenha);
        }

        public async Task<FuncionarioModel?> ListarPorId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            var funcionario = await _funcionarioProcesso.ListarPorIdAsync(id);

            if (funcionario != null)
            {
                var perfil = await _perfilProcesso.ResolverPerfilDoFuncionarioAsync(funcionario);
                funcionario.PerfilId ??= perfil?.Id;
                funcionario.PerfilNome = perfil?.Nome;
            }

            return funcionario;
        }

        public async Task<List<FuncionarioModel>> ListarPorSupermercado(string supermercadoId)
        {
            var funcionarios = await _funcionarioProcesso.ListarPorSupermercadoAsync(supermercadoId);
            var perfis = await _perfilProcesso.ListarAsync(supermercadoId);

            foreach (var funcionario in funcionarios)
            {
                var perfil = perfis.FirstOrDefault(p => PerfilProcesso.PertenceAoPerfil(funcionario, p));
                funcionario.PerfilId ??= perfil?.Id;
                funcionario.PerfilNome = perfil?.Nome ?? "Sem perfil";
            }

            return funcionarios;
        }
    }
}
