using DOMAIN.Model.Ong;
using Excecoes;
using REPOSITORY.Mapeadores.Ong;

namespace SERVICE.Processo
{
    public class OngProcesso(IOngMapeador ongMapeador)
    {
        private readonly IOngMapeador _ongMapeador = ongMapeador;

        public async Task<List<OngModel>> ListarAsync(bool apenasAtivas)
        {
            var ongs = await _ongMapeador.ListarTodasAsync();
            return [.. ongs.Where(o => !apenasAtivas || o.Ativa).OrderBy(o => o.Nome)];
        }

        public Task<OngModel?> ObterAsync(string id) => _ongMapeador.ObterPorIdAsync(id);

        public async Task<string> CriarAsync(OngModel ong)
        {
            Validar(ong);
            ong.Id = null;
            return await _ongMapeador.CriarAsync(ong);
        }

        public async Task AtualizarAsync(OngModel ong)
        {
            Validar(ong);

            if (string.IsNullOrWhiteSpace(ong.Id) || await _ongMapeador.ObterPorIdAsync(ong.Id) == null)
            {
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "ONG não encontrada.");
            }

            await _ongMapeador.AtualizarAsync(ong);
        }

        public async Task DeletarAsync(string id)
        {
            if (await _ongMapeador.ObterPorIdAsync(id) == null)
            {
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "ONG não encontrada.");
            }

            await _ongMapeador.DeletarAsync(id);
        }

        public async Task<OngModel> ObterAtivaAsync(string id)
        {
            var ong = await _ongMapeador.ObterPorIdAsync(id);

            if (ong == null || !ong.Ativa)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "A ONG escolhida não está disponível para doações.");
            }

            return ong;
        }

        private static void Validar(OngModel ong)
        {
            if (string.IsNullOrWhiteSpace(ong.Nome) || ong.Nome.Trim().Length > 120)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Informe o nome da ONG com até 120 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(ong.UrlImagem) && !Uri.TryCreate(ong.UrlImagem, UriKind.Absolute, out _))
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "A URL da imagem da ONG é inválida.");
            }

            ong.Nome = ong.Nome.Trim();
        }
    }
}
