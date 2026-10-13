using DOMAIN.Model.Cliente;
using DOMAIN.Model.Vitrine;
using Excecoes;
using REPOSITORY.Mapeadores.Cliente;

namespace SERVICE.Processo
{
    public class ClienteProcesso(IClienteMapeador clienteMapeador, VitrineProcesso vitrineProcesso)
    {
        private const int LimiteFavoritos = 200;
        private const int LimiteDispositivos = 10;

        private readonly IClienteMapeador _clienteMapeador = clienteMapeador;
        private readonly VitrineProcesso _vitrineProcesso = vitrineProcesso;

        public async Task<PerfilClienteModel> CriarPerfilAsync(string uid, string? email, CriarClienteModel dados)
        {
            var existente = await _clienteMapeador.ObterAsync(uid);

            if (existente == null)
            {
                await _clienteMapeador.CriarAsync(new ClienteModel
                {
                    Id = uid,
                    NomeCompleto = dados.NomeCompleto?.Trim(),
                    Email = email,
                    Telefone = dados.Telefone?.Trim(),
                    Perfil = ClienteModel.PerfilCliente
                });
            }

            return await ObterPerfilAsync(uid);
        }

        public async Task<PerfilClienteModel> ObterPerfilAsync(string uid)
        {
            var cliente = await ObterClienteAsync(uid);
            var nivel = NivelFidelidadeUtil.Obter(cliente.TotalDoacoes);

            return new PerfilClienteModel
            {
                Id = cliente.Id,
                NomeCompleto = cliente.NomeCompleto,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                Endereco = cliente.Endereco,
                TotalDoacoes = cliente.TotalDoacoes,
                CashbackDisponivel = Math.Round(cliente.CashbackDisponivel, 2),
                Nivel = nivel.Nome,
                PercentualCashbackNivel = (int)Math.Round(nivel.MultiplicadorCashback * 100),
                DoacoesParaProximoNivel = NivelFidelidadeUtil.DoacoesParaProximoNivel(cliente.TotalDoacoes),
                Impacto = cliente.Impacto ?? new ImpactoModel()
            };
        }

        public async Task<PerfilClienteModel> AtualizarPerfilAsync(string uid, AtualizarClienteModel dados)
        {
            await ObterClienteAsync(uid);

            var campos = new Dictionary<string, object?>
            {
                ["fullName"] = dados.NomeCompleto?.Trim(),
                ["phone"] = dados.Telefone?.Trim()
            };

            if (dados.Endereco != null)
            {
                campos["address"] = dados.Endereco;
            }

            await _clienteMapeador.AtualizarCamposAsync(uid, campos);
            return await ObterPerfilAsync(uid);
        }

        public async Task<ImpactoModel> ObterImpactoAsync(string uid)
        {
            var cliente = await ObterClienteAsync(uid);
            return cliente.Impacto ?? new ImpactoModel();
        }

        public async Task<List<ProdutoVitrineModel>> ListarFavoritosAsync(string uid)
        {
            var cliente = await ObterClienteAsync(uid);
            return await _vitrineProcesso.ListarPorIdsAsync(cliente.Favoritos);
        }

        public async Task AdicionarFavoritoAsync(string uid, string produtoId)
        {
            var cliente = await ObterClienteAsync(uid);

            if (cliente.Favoritos.Contains(produtoId)) return;

            if (cliente.Favoritos.Count >= LimiteFavoritos)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, $"Limite de {LimiteFavoritos} favoritos atingido.");
            }

            if (await _vitrineProcesso.ObterAsync(produtoId) == null)
            {
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Produto não encontrado.");
            }

            await _clienteMapeador.AdicionarFavoritoAsync(uid, produtoId);
        }

        public async Task RemoverFavoritoAsync(string uid, string produtoId)
        {
            await ObterClienteAsync(uid);
            await _clienteMapeador.RemoverFavoritoAsync(uid, produtoId);
        }

        public async Task RegistrarDispositivoAsync(string uid, string token)
        {
            var cliente = await ObterClienteAsync(uid);

            if (cliente.TokensDispositivo.Contains(token)) return;

            if (cliente.TokensDispositivo.Count >= LimiteDispositivos)
            {
                await _clienteMapeador.RemoverTokenDispositivoAsync(uid, cliente.TokensDispositivo[0]);
            }

            await _clienteMapeador.AdicionarTokenDispositivoAsync(uid, token);
        }

        public async Task RemoverDispositivoAsync(string uid, string token)
        {
            await _clienteMapeador.RemoverTokenDispositivoAsync(uid, token);
        }

        public async Task<List<string>> ObterTokensAsync(string uid)
        {
            var cliente = await _clienteMapeador.ObterAsync(uid);
            return cliente?.TokensDispositivo ?? [];
        }

        public async Task<ClienteModel> ObterClienteAsync(string uid)
        {
            var cliente = await _clienteMapeador.ObterAsync(uid);

            if (cliente == null)
            {
                throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Perfil do cliente não encontrado. Crie o perfil em POST api/app/clientes.");
            }

            if (cliente.EhFuncionarioDoPainel)
            {
                throw new IValidExcecao(CodigoExcecao.NaoAutorizado, "Esta conta pertence ao painel administrativo.");
            }

            return cliente;
        }
    }
}
