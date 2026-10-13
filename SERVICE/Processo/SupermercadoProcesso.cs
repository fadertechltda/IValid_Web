using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DOMAIN.Model.Supermercado;
using DOMAIN.Model.Vitrine;
using Excecoes;
using REPOSITORY.Mapeadores.Produto;
using REPOSITORY.Mapeadores.Supermercado;

namespace SERVICE.Processo
{
    public class SupermercadoProcesso(ISupermercadoMapeador supermercadoMapeador, IProdutoMapeador produtoMapeador)
    {
        private readonly ISupermercadoMapeador _supermercadoMapeador = supermercadoMapeador;
        private readonly IProdutoMapeador _produtoMapeador = produtoMapeador;

        public async Task<List<SupermercadoPublicoModel>> ListarPublicosAsync()
        {
            var supermercados = await _supermercadoMapeador.ListarTodosAsync();
            return [.. supermercados.OrderBy(s => s.Nome).Select(ConverterPublico)];
        }

        public async Task<SupermercadoPublicoModel?> ObterPublicoAsync(string id)
        {
            var supermercado = await _supermercadoMapeador.ListarPorIdAsync(id);
            return supermercado == null ? null : ConverterPublico(supermercado);
        }

        public async Task AtualizarDadosLojaAsync(string id, DadosLojaModel dados)
        {
            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Informe o nome da loja.");
            }

            if (dados.Latitude.HasValue != dados.Longitude.HasValue)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Informe a latitude e a longitude juntas.");
            }

            var supermercado = await _supermercadoMapeador.ListarPorIdAsync(id)
                ?? throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Supermercado não encontrado.");

            var nome = dados.Nome.Trim();
            await _supermercadoMapeador.AtualizarDadosAsync(id, nome, dados.Endereco?.Trim(), dados.Latitude, dados.Longitude);

            if (supermercado.Nome != nome)
            {
                var produtos = await _produtoMapeador.ListarPorSupermercadoAsync(id);

                foreach (var produto in produtos.Where(p => !string.IsNullOrEmpty(p.Id)))
                {
                    await _produtoMapeador.AtualizarCamposAsync(produto.Id!, new Dictionary<string, object> { ["storeName"] = nome });
                }
            }
        }

        private static SupermercadoPublicoModel ConverterPublico(SupermercadoModel supermercado) => new()
        {
            Id = supermercado.Id,
            Nome = supermercado.Nome,
            Endereco = supermercado.Endereco,
            Latitude = supermercado.Latitude,
            Longitude = supermercado.Longitude
        };

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
