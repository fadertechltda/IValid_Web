using DOMAIN.Model.Dashboard;
using SERVICE.Processo;

namespace SERVICE.Fachada
{
    public class DashboardFachada(DashboardProcesso dashboardProcesso)
    {
        private readonly DashboardProcesso _dashboardProcesso = dashboardProcesso;

        public async Task<DashboardModel> ObterDashboard(string supermercadoId)
        {
            return await _dashboardProcesso.ObterDashboard(supermercadoId);
        }
    }
}
