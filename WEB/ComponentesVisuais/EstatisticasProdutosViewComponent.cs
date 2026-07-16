using DOMAIN.Model.Produto;
using Microsoft.AspNetCore.Mvc;

namespace WEB.ComponentesVisuais
{
    public class EstatisticasProdutosViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<ProdutoModel>? produtos)
        {
            var lista = produtos ?? [];

            var resumo = new ResumoEstatisticas
            {
                TotalVermelho = lista.Count(p => p.Status?.ToUpper() == "VERMELHO"),
                TotalAmarelo  = lista.Count(p => p.Status?.ToUpper() == "AMARELO"),
                TotalVerde    = lista.Count(p => p.Status?.ToUpper() == "VERDE"),
                Total         = lista.Count
            };

            return View(resumo);
        }
    }

    public class ResumoEstatisticas
    {
        public int TotalVermelho { get; set; }
        public int TotalAmarelo  { get; set; }
        public int TotalVerde    { get; set; }
        public int Total         { get; set; }
    }
}
