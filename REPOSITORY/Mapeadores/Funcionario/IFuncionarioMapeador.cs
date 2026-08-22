using DOMAIN.Model.Funcionario;

namespace REPOSITORY.Mapeadores.Funcionario
{
    public interface IFuncionarioMapeador
    {
        Task CriarAsync(FuncionarioModel funcionario);
        Task AtualizarAsync(FuncionarioModel funcionario);
        Task<FuncionarioModel?> ListarPorIdAsync(string id);
        Task<List<FuncionarioModel>> ListarPorSupermercadoAsync(string supermercadoId);
    }
}
