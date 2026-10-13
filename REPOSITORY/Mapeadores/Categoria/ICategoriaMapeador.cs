using DOMAIN.Model.Categoria;

namespace REPOSITORY.Mapeadores.Categoria
{
    public interface ICategoriaMapeador
    {
        Task<List<CategoriaModel>> ListarTodasAsync();
        Task<CategoriaModel?> ObterPorIdAsync(string id);
        Task<string> CriarAsync(CategoriaModel categoria);
        Task AtualizarAsync(CategoriaModel categoria);
        Task DeletarAsync(string id);
    }
}
