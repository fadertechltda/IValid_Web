namespace REPOSITORY.Mapeadores.Rotina
{
    public interface IRotinaMapeador
    {
        Task<string?> ObterUltimaExecucaoAsync(string rotina);
        Task RegistrarExecucaoAsync(string rotina, string marca);
    }
}
