using DOMAIN.Model.Repasse;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class RepasseFachada(RepasseProcesso repasseProcesso)
    {
        private readonly RepasseProcesso _repasseProcesso = repasseProcesso;

        public async Task<ResumoRepasseModel> ObterAsync(string supermercadoId)
        {
            if (string.IsNullOrWhiteSpace(supermercadoId)) return new ResumoRepasseModel();
            return await _repasseProcesso.ObterAsync(supermercadoId);
        }
    }
}
