using Microsoft.AspNetCore.Mvc;
using SERVICE.Fachada;

namespace WEB_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController(DashboardFachada dashboardFachada) : ControllerBase
    {
        private readonly DashboardFachada _dashboardFachada = dashboardFachada;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string supermercadoId)
        {
            var dashboard = await _dashboardFachada.ObterDashboard(supermercadoId);
            return Ok(dashboard);
        }
    }
}
