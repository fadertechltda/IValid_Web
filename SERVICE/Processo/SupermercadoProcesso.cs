using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DOMAIN.Model.Supermercado;
using REPOSITORY.Mapeadores.Supermercado;

namespace SERVICE.Processo
{
    public class SupermercadoProcesso(ISupermercadoMapeador supermercadoMapeador)
    {
        private readonly ISupermercadoMapeador _supermercadoMapeador = supermercadoMapeador;

        public async Task<string> CriarSupermercado(SupermercadoModel supermercado)
        {
            supermercado.CodigoAcesso = await GerarCodigoAcessoUnico(supermercado.Nome ?? "loja");
            return await _supermercadoMapeador.CriarAsync(supermercado);
        }

        public async Task<SupermercadoModel?> ObterPorIdAsync(string id)
        {
            return await _supermercadoMapeador.ListarPorIdAsync(id);
        }

        public async Task<SupermercadoModel?> ObterPorCodigoAcessoAsync(string codigoAcesso)
        {
            return await _supermercadoMapeador.ObterPorCodigoAcessoAsync(codigoAcesso);
        }

        private async Task<string> GerarCodigoAcessoUnico(string nome)
        {
            string raiz = Slugificar(nome);
            string candidato = raiz;
            var aleatorio = new Random();

            while (await _supermercadoMapeador.ObterPorCodigoAcessoAsync(candidato) != null)
            {
                candidato = $"{raiz}{aleatorio.Next(100, 999)}";
            }

            return candidato;
        }

        private static string Slugificar(string texto)
        {
            string textoNormalizado = texto.Normalize(NormalizationForm.FormD);
            var construtor = new StringBuilder();

            foreach (char caractere in textoNormalizado)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
                {
                    construtor.Append(caractere);
                }
            }

            string slug = Regex.Replace(construtor.ToString().ToLowerInvariant(), "[^a-z0-9]+", "");

            return string.IsNullOrEmpty(slug) ? "loja" : slug;
        }
    }
}
