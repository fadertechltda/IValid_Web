using DOMAIN.Model.Categoria;
using DOMAIN.Model.Ong;
using DOMAIN.Model.Vitrine;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class VitrineFachada(VitrineProcesso vitrineProcesso, CategoriaProcesso categoriaProcesso, SupermercadoProcesso supermercadoProcesso, OngProcesso ongProcesso)
    {
        private readonly VitrineProcesso _vitrineProcesso = vitrineProcesso;
        private readonly CategoriaProcesso _categoriaProcesso = categoriaProcesso;
        private readonly SupermercadoProcesso _supermercadoProcesso = supermercadoProcesso;
        private readonly OngProcesso _ongProcesso = ongProcesso;

        public Task<PaginaModel<ProdutoVitrineModel>> ListarProdutosAsync(FiltroVitrineModel filtro) => _vitrineProcesso.ListarAsync(filtro);

        public Task<ProdutoVitrineModel?> ObterProdutoAsync(string id) => _vitrineProcesso.ObterAsync(id);

        public Task<List<CategoriaModel>> ListarCategoriasAsync() => _categoriaProcesso.ListarAsync();

        public Task<List<SupermercadoPublicoModel>> ListarSupermercadosAsync() => _supermercadoProcesso.ListarPublicosAsync();

        public Task<SupermercadoPublicoModel?> ObterSupermercadoAsync(string id) => _supermercadoProcesso.ObterPublicoAsync(id);

        public Task<List<OngModel>> ListarOngsAtivasAsync() => _ongProcesso.ListarAsync(apenasAtivas: true);
    }
}
