using DOMAIN.Model.Cliente;

namespace REPOSITORY.Mapeadores.Cliente
{
    public interface IClienteMapeador
    {
        Task<ClienteModel?> ObterAsync(string uid);
        Task<bool> CriarAsync(ClienteModel cliente);
        Task AtualizarCamposAsync(string uid, Dictionary<string, object?> campos);
        Task AdicionarFavoritoAsync(string uid, string produtoId);
        Task RemoverFavoritoAsync(string uid, string produtoId);
        Task AdicionarTokenDispositivoAsync(string uid, string token);
        Task RemoverTokenDispositivoAsync(string uid, string token);
    }
}
