using DOMAIN.Model.Funcionario;
using REPOSITORY.Mapeadores.Funcionario;

namespace SERVICE.Processo
{
    public class FuncionarioProcesso(IFuncionarioMapeador funcionarioMapeador)
    {
        private readonly IFuncionarioMapeador _funcionarioMapeador = funcionarioMapeador;

        public async Task CriarFuncionario(FuncionarioModel funcionario, string senha)
        {
            funcionario.SenhaHash = SenhaHasher.GerarHash(senha);
            funcionario.Ativo = true;
            await _funcionarioMapeador.CriarAsync(funcionario);
        }

        public async Task AtualizarFuncionario(FuncionarioModel funcionario, string? novaSenha)
        {
            if (!string.IsNullOrEmpty(novaSenha))
            {
                funcionario.SenhaHash = SenhaHasher.GerarHash(novaSenha);
            }

            await _funcionarioMapeador.AtualizarAsync(funcionario);
        }

        public async Task<FuncionarioModel?> ListarPorIdAsync(string id)
        {
            return await _funcionarioMapeador.ListarPorIdAsync(id);
        }

        public async Task<List<FuncionarioModel>> ListarPorSupermercadoAsync(string supermercadoId)
        {
            return await _funcionarioMapeador.ListarPorSupermercadoAsync(supermercadoId);
        }

        public static bool VerificarSenha(FuncionarioModel funcionario, string senha)
        {
            if (string.IsNullOrEmpty(funcionario.SenhaHash))
                return false;

            return SenhaHasher.VerificarSenha(senha, funcionario.SenhaHash);
        }
    }
}
