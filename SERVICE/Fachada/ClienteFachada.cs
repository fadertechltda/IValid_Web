using DOMAIN.Model.Cliente;
using DOMAIN.Model.Vitrine;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class ClienteFachada(ClienteProcesso clienteProcesso)
    {
        private readonly ClienteProcesso _clienteProcesso = clienteProcesso;

        public Task<PerfilClienteModel> CriarPerfilAsync(string uid, string? email, CriarClienteModel dados) => _clienteProcesso.CriarPerfilAsync(uid, email, dados);

        public Task<PerfilClienteModel> ObterPerfilAsync(string uid) => _clienteProcesso.ObterPerfilAsync(uid);

        public Task<PerfilClienteModel> AtualizarPerfilAsync(string uid, AtualizarClienteModel dados) => _clienteProcesso.AtualizarPerfilAsync(uid, dados);

        public Task<ImpactoModel> ObterImpactoAsync(string uid) => _clienteProcesso.ObterImpactoAsync(uid);

        public Task<List<ProdutoVitrineModel>> ListarFavoritosAsync(string uid) => _clienteProcesso.ListarFavoritosAsync(uid);

        public Task AdicionarFavoritoAsync(string uid, string produtoId) => _clienteProcesso.AdicionarFavoritoAsync(uid, produtoId);

        public Task RemoverFavoritoAsync(string uid, string produtoId) => _clienteProcesso.RemoverFavoritoAsync(uid, produtoId);

        public Task RegistrarDispositivoAsync(string uid, string token) => _clienteProcesso.RegistrarDispositivoAsync(uid, token);

        public Task RemoverDispositivoAsync(string uid, string token) => _clienteProcesso.RemoverDispositivoAsync(uid, token);
    }
}
