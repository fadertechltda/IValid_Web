using DOMAIN.Model.Funcionario;
using DOMAIN.Validador.Funcionario;
using Excecoes;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class FuncionarioFachada(FuncionarioProcesso funcionarioProcesso, FuncionarioValidacao validador)
    {
        private readonly FuncionarioProcesso _funcionarioProcesso = funcionarioProcesso;
        private readonly FuncionarioValidacao _validador = validador;

        public async Task CriarFuncionario(FuncionarioModel funcionario, string senha)
        {
            _validador.AssineRegrasInclusao();

            var resultado = await _validador.ValidateAsync(funcionario);

            if (!resultado.IsValid)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, resultado.Errors.First().ErrorMessage);
            }

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

            await _funcionarioProcesso.AtualizarFuncionario(funcionario, novaSenha);
        }

        public async Task<FuncionarioModel?> ListarPorId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return await _funcionarioProcesso.ListarPorIdAsync(id);
        }

        public async Task<List<FuncionarioModel>> ListarPorSupermercado(string supermercadoId)
        {
            return await _funcionarioProcesso.ListarPorSupermercadoAsync(supermercadoId);
        }
    }
}
