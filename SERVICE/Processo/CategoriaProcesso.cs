using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DOMAIN.Model.Categoria;
using Excecoes;
using REPOSITORY.Mapeadores.Categoria;
using REPOSITORY.Mapeadores.Produto;

namespace SERVICE.Processo
{
    public class CategoriaProcesso(ICategoriaMapeador categoriaMapeador, IProdutoMapeador produtoMapeador)
    {
        private readonly ICategoriaMapeador _categoriaMapeador = categoriaMapeador;
        private readonly IProdutoMapeador _produtoMapeador = produtoMapeador;

        public async Task<List<CategoriaModel>> ListarAsync()
        {
            var categorias = await _categoriaMapeador.ListarTodasAsync();
            return [.. categorias.OrderBy(c => c.Ordem).ThenBy(c => c.Nome)];
        }

        public Task<CategoriaModel?> ObterAsync(string id) => _categoriaMapeador.ObterPorIdAsync(id);

        public async Task<string> CriarAsync(CategoriaModel categoria)
        {
            Validar(categoria);

            categoria.Id = string.IsNullOrWhiteSpace(categoria.Id) ? GerarIdentificador(categoria.Nome!) : GerarIdentificador(categoria.Id);

            if (await _categoriaMapeador.ObterPorIdAsync(categoria.Id) != null)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Já existe uma categoria com esse identificador.");
            }

            return await _categoriaMapeador.CriarAsync(categoria);
        }

        public async Task AtualizarAsync(CategoriaModel categoria)
        {
            Validar(categoria);

            if (string.IsNullOrWhiteSpace(categoria.Id) || await _categoriaMapeador.ObterPorIdAsync(categoria.Id) == null)
            {
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Categoria não encontrada.");
            }

            await _categoriaMapeador.AtualizarAsync(categoria);
        }

        public async Task DeletarAsync(string id)
        {
            if (await _categoriaMapeador.ObterPorIdAsync(id) == null)
            {
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Categoria não encontrada.");
            }

            var produtos = await _produtoMapeador.ListarTodosAsync();

            if (produtos.Any(p => p.CategoriaId == id))
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "A categoria está em uso por produtos e não pode ser excluída.");
            }

            await _categoriaMapeador.DeletarAsync(id);
        }

        private static void Validar(CategoriaModel categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.Nome) || categoria.Nome.Trim().Length > 60)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Informe um nome de categoria com até 60 caracteres.");
            }

            categoria.Nome = categoria.Nome.Trim();
        }

        private static string GerarIdentificador(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var construtor = new StringBuilder();

            foreach (char caractere in normalizado)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
                {
                    construtor.Append(caractere);
                }
            }

            var identificador = Regex.Replace(construtor.ToString().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

            return string.IsNullOrEmpty(identificador) ? Guid.NewGuid().ToString("N")[..8] : identificador;
        }
    }
}
