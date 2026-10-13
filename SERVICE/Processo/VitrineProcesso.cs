using System.Globalization;
using DOMAIN.Model.Configuracao;
using DOMAIN.Model.Produto;
using DOMAIN.Model.Vitrine;
using REPOSITORY.Mapeadores.Produto;

namespace SERVICE.Processo
{
    public class VitrineProcesso(IProdutoMapeador produtoMapeador, ConfiguracaoProcesso configuracaoProcesso)
    {
        public const string OrdemVencimento = "vencimento";
        public const string OrdemMenorPreco = "menorPreco";
        public const string OrdemMaiorDesconto = "maiorDesconto";
        public const string OrdemNome = "nome";

        private readonly IProdutoMapeador _produtoMapeador = produtoMapeador;
        private readonly ConfiguracaoProcesso _configuracaoProcesso = configuracaoProcesso;

        public async Task<PaginaModel<ProdutoVitrineModel>> ListarAsync(FiltroVitrineModel filtro)
        {
            List<ProdutoModel> produtos = string.IsNullOrWhiteSpace(filtro.SupermercadoId)
                ? await _produtoMapeador.ListarTodosAsync()
                : await _produtoMapeador.ListarPorSupermercadoAsync(filtro.SupermercadoId);

            await AplicarRegrasAsync(produtos);

            IEnumerable<ProdutoModel> consulta = produtos.Where(EstaDisponivel);

            if (!string.IsNullOrWhiteSpace(filtro.CategoriaId))
            {
                consulta = consulta.Where(p => p.CategoriaId == filtro.CategoriaId);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Status))
            {
                consulta = consulta.Where(p => string.Equals(p.Status, filtro.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Busca))
            {
                var termo = filtro.Busca.Trim();
                consulta = consulta.Where(p => Contem(p.Nome, termo) || Contem(p.Marca, termo) || Contem(p.NomeSupermercado, termo));
            }

            consulta = Ordenar(consulta, filtro.Ordem);

            var lista = consulta.ToList();
            int pagina = Math.Max(filtro.Pagina, 1);
            int tamanho = Math.Clamp(filtro.TamanhoPagina, 1, 100);

            return new PaginaModel<ProdutoVitrineModel>
            {
                Pagina = pagina,
                TamanhoPagina = tamanho,
                Total = lista.Count,
                Itens = [.. lista.Skip((pagina - 1) * tamanho).Take(tamanho).Select(Converter)]
            };
        }

        public async Task<ProdutoVitrineModel?> ObterAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var produto = await _produtoMapeador.ListarPorIdAsync(id);
            if (produto == null) return null;

            await AplicarRegrasAsync([produto]);
            return Converter(produto);
        }

        public async Task<List<ProdutoVitrineModel>> ListarPorIdsAsync(IEnumerable<string> ids)
        {
            var produtos = await _produtoMapeador.ListarPorIdsAsync(ids);
            await AplicarRegrasAsync(produtos);
            return [.. Ordenar(produtos, OrdemVencimento).Select(Converter)];
        }

        public async Task AplicarRegrasAsync(IEnumerable<ProdutoModel> produtos)
        {
            var configuracoes = new Dictionary<string, ConfiguracaoModel>();

            foreach (var produto in produtos)
            {
                var supermercadoId = produto.SupermercadoId ?? string.Empty;

                if (!configuracoes.TryGetValue(supermercadoId, out var configuracao))
                {
                    configuracao = await _configuracaoProcesso.ObterConfiguracao(supermercadoId);
                    configuracoes[supermercadoId] = configuracao;
                }

                ProdutoProcesso.AplicarRegras(produto, configuracao);
            }
        }

        public static bool EstaDisponivel(ProdutoModel produto)
        {
            return !string.IsNullOrEmpty(produto.SupermercadoId)
                && produto.Quantidade > 0
                && !string.Equals(produto.Status, "VENCIDO", StringComparison.OrdinalIgnoreCase);
        }

        public static ProdutoVitrineModel Converter(ProdutoModel produto) => new()
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Marca = produto.Marca,
            UrlImagem = produto.UrlImagem,
            CategoriaId = produto.CategoriaId,
            SupermercadoId = produto.SupermercadoId,
            NomeSupermercado = produto.NomeSupermercado,
            PrecoOriginal = produto.Preco,
            PrecoAtual = produto.PrecoPromocao,
            PercentualDesconto = produto.DescricaoPorcentual,
            DiasParaVencer = produto.DiaValidade,
            DataVencimento = produto.DataVencimento,
            Status = produto.Status,
            QuantidadeDisponivel = Math.Max(produto.Quantidade, 0)
        };

        private static IEnumerable<ProdutoModel> Ordenar(IEnumerable<ProdutoModel> produtos, string? ordem)
        {
            return ordem switch
            {
                OrdemMenorPreco => produtos.OrderBy(p => p.PrecoPromocao).ThenBy(p => p.DataVencimento),
                OrdemMaiorDesconto => produtos.OrderByDescending(p => p.DescricaoPorcentual).ThenBy(p => p.DataVencimento),
                OrdemNome => produtos.OrderBy(p => p.Nome, StringComparer.Create(new CultureInfo("pt-BR"), true)),
                _ => produtos.OrderBy(p => p.DataVencimento).ThenBy(p => p.Nome)
            };
        }

        private static bool Contem(string? texto, string termo)
        {
            if (string.IsNullOrEmpty(texto)) return false;

            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(texto, termo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
        }
    }
}
