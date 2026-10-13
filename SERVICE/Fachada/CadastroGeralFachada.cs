using DOMAIN.Model.Categoria;
using DOMAIN.Model.Ong;
using DOMAIN.Model.Supermercado;
using DOMAIN.Model.Vitrine;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class CategoriaFachada(CategoriaProcesso categoriaProcesso)
    {
        private readonly CategoriaProcesso _categoriaProcesso = categoriaProcesso;

        public Task<List<CategoriaModel>> ListarAsync() => _categoriaProcesso.ListarAsync();

        public Task<CategoriaModel?> ObterAsync(string id) => _categoriaProcesso.ObterAsync(id);

        public Task<string> CriarAsync(CategoriaModel categoria) => _categoriaProcesso.CriarAsync(categoria);

        public Task AtualizarAsync(CategoriaModel categoria) => _categoriaProcesso.AtualizarAsync(categoria);

        public Task DeletarAsync(string id) => _categoriaProcesso.DeletarAsync(id);
    }

    public class OngFachada(OngProcesso ongProcesso)
    {
        private readonly OngProcesso _ongProcesso = ongProcesso;

        public Task<List<OngModel>> ListarAsync() => _ongProcesso.ListarAsync(apenasAtivas: false);

        public Task<OngModel?> ObterAsync(string id) => _ongProcesso.ObterAsync(id);

        public Task<string> CriarAsync(OngModel ong) => _ongProcesso.CriarAsync(ong);

        public Task AtualizarAsync(OngModel ong) => _ongProcesso.AtualizarAsync(ong);

        public Task DeletarAsync(string id) => _ongProcesso.DeletarAsync(id);
    }

    public class SupermercadoFachada(SupermercadoProcesso supermercadoProcesso)
    {
        private readonly SupermercadoProcesso _supermercadoProcesso = supermercadoProcesso;

        public async Task<DadosLojaModel?> ObterDadosLojaAsync(string id)
        {
            SupermercadoModel? supermercado = await _supermercadoProcesso.ObterPorIdAsync(id);

            return supermercado == null ? null : new DadosLojaModel
            {
                Nome = supermercado.Nome,
                Endereco = supermercado.Endereco,
                Latitude = supermercado.Latitude,
                Longitude = supermercado.Longitude
            };
        }

        public Task AtualizarDadosLojaAsync(string id, DadosLojaModel dados) => _supermercadoProcesso.AtualizarDadosLojaAsync(id, dados);
    }
}
