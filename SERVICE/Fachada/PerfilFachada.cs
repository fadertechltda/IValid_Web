using DOMAIN.Model.Perfil;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class PerfilFachada(PerfilProcesso perfilProcesso)
    {
        private readonly PerfilProcesso _perfilProcesso = perfilProcesso;

        public Task<List<PerfilModel>> ListarAsync(string supermercadoId) => _perfilProcesso.ListarAsync(supermercadoId);

        public Task<PerfilModel?> ObterAsync(string id, string supermercadoId) => _perfilProcesso.ObterAsync(id, supermercadoId);

        public Task<string> CriarAsync(string supermercadoId, PerfilModel perfil) => _perfilProcesso.CriarAsync(supermercadoId, perfil);

        public Task AtualizarAsync(string supermercadoId, PerfilModel perfil) => _perfilProcesso.AtualizarAsync(supermercadoId, perfil);

        public Task DeletarAsync(string id, string supermercadoId) => _perfilProcesso.DeletarAsync(id, supermercadoId);
    }
}
