using Microsoft.AspNetCore.Mvc;

namespace WEB.Controllers.Produto
{
    public class ProdutoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
