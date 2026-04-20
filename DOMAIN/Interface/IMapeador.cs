namespace DOMAIN.Interface
{
    public interface IMapeador<T> where T : class 
    {
        Task CadastrarAsync(T item);
        Task AtualizarAsync(T item);
        Task DeletarAsync(T item);
        Task<T?> ListarPorIdAsync(string id);
        Task<List<T>> ListarTodosAsync();
    }
}
