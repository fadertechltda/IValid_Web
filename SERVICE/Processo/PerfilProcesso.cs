using DOMAIN.Model.Funcionario;
using DOMAIN.Model.Perfil;
using Excecoes;
using REPOSITORY.Mapeadores.Funcionario;
using REPOSITORY.Mapeadores.Perfil;

namespace SERVICE.Processo
{
    public class PerfilProcesso(IPerfilMapeador perfilMapeador, IFuncionarioMapeador funcionarioMapeador)
    {
        private const int TamanhoMaximoNome = 60;

        private readonly IPerfilMapeador _perfilMapeador = perfilMapeador;
        private readonly IFuncionarioMapeador _funcionarioMapeador = funcionarioMapeador;

        public async Task<List<PerfilModel>> ListarAsync(string supermercadoId)
        {
            var perfis = await ListarGarantindoPadroesAsync(supermercadoId);
            var funcionarios = await _funcionarioMapeador.ListarPorSupermercadoAsync(supermercadoId);

            foreach (var perfil in perfis)
            {
                perfil.QuantidadeFuncionarios = funcionarios.Count(f => PertenceAoPerfil(f, perfil));
            }

            return [.. perfis.OrderBy(p => p.Nome, StringComparer.CurrentCultureIgnoreCase)];
        }

        public async Task<PerfilModel?> ObterAsync(string id, string supermercadoId)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var perfil = await _perfilMapeador.ObterPorIdAsync(id);
            return perfil?.SupermercadoId == supermercadoId ? perfil : null;
        }

        public async Task<PerfilModel> ObterObrigatorioAsync(string? id, string supermercadoId)
        {
            return await ObterAsync(id ?? string.Empty, supermercadoId)
                ?? throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Selecione um perfil válido.");
        }

        public async Task<string> CriarAsync(string supermercadoId, PerfilModel perfil)
        {
            var existentes = await ListarGarantindoPadroesAsync(supermercadoId);
            Validar(perfil, existentes);

            perfil.Id = null;
            perfil.SupermercadoId = supermercadoId;
            perfil.ChaveLegada = null;

            return await _perfilMapeador.CriarAsync(perfil);
        }

        public async Task AtualizarAsync(string supermercadoId, PerfilModel perfil)
        {
            _ = await ObterAsync(perfil.Id ?? string.Empty, supermercadoId)
                ?? throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Perfil não encontrado.");

            var existentes = await _perfilMapeador.ListarPorSupermercadoAsync(supermercadoId);
            Validar(perfil, existentes);

            await _perfilMapeador.AtualizarAsync(perfil);
        }

        public async Task DeletarAsync(string id, string supermercadoId)
        {
            var perfil = await ObterAsync(id, supermercadoId)
                ?? throw new IValidExcecao(CodigoExcecao.EntidadeNaoEncontrada, "Perfil não encontrado.");

            var funcionarios = await _funcionarioMapeador.ListarPorSupermercadoAsync(supermercadoId);
            int emUso = funcionarios.Count(f => PertenceAoPerfil(f, perfil));

            if (emUso > 0)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao,
                    $"O perfil está em uso por {emUso} funcionário(s). Troque o perfil deles antes de excluir.");
            }

            await _perfilMapeador.DeletarAsync(id);
        }

        public async Task<PerfilModel?> ResolverPerfilDoFuncionarioAsync(FuncionarioModel funcionario)
        {
            if (string.IsNullOrEmpty(funcionario.SupermercadoId)) return null;

            if (!string.IsNullOrEmpty(funcionario.PerfilId))
            {
                return await ObterAsync(funcionario.PerfilId, funcionario.SupermercadoId);
            }

            var perfis = await ListarGarantindoPadroesAsync(funcionario.SupermercadoId);
            var chaveLegada = (funcionario.PerfilLegado ?? PerfilFuncionario.Atendente).ToString();

            return perfis.FirstOrDefault(p => p.ChaveLegada == chaveLegada);
        }

        public static bool PertenceAoPerfil(FuncionarioModel funcionario, PerfilModel perfil)
        {
            if (!string.IsNullOrEmpty(funcionario.PerfilId))
            {
                return funcionario.PerfilId == perfil.Id;
            }

            var chaveLegada = (funcionario.PerfilLegado ?? PerfilFuncionario.Atendente).ToString();
            return perfil.ChaveLegada == chaveLegada;
        }

        private async Task<List<PerfilModel>> ListarGarantindoPadroesAsync(string supermercadoId)
        {
            var perfis = await _perfilMapeador.ListarPorSupermercadoAsync(supermercadoId);

            if (perfis.Count > 0) return perfis;

            foreach (var perfilPadrao in PerfilPadraoUtil.Criar(supermercadoId))
            {
                await _perfilMapeador.CriarSeNaoExistirAsync(perfilPadrao);
            }

            return await _perfilMapeador.ListarPorSupermercadoAsync(supermercadoId);
        }

        private static void Validar(PerfilModel perfil, List<PerfilModel> existentes)
        {
            var nome = perfil.Nome?.Trim();

            if (string.IsNullOrEmpty(nome) || nome.Length > TamanhoMaximoNome)
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, $"Informe o nome do perfil com até {TamanhoMaximoNome} caracteres.");
            }

            if (string.Equals(nome, PermissaoUtil.PerfilAdministrador, StringComparison.OrdinalIgnoreCase))
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "O nome \"Administrador\" é reservado ao dono da loja.");
            }

            if (existentes.Any(p => p.Id != perfil.Id && string.Equals(p.Nome?.Trim(), nome, StringComparison.OrdinalIgnoreCase)))
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Já existe um perfil com esse nome.");
            }

            var permissoes = (perfil.Permissoes ?? []).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();

            if (permissoes.Any(p => !PermissaoUtil.EhValida(p)))
            {
                throw new IValidExcecao(CodigoExcecao.SolicitarValidacao, "Foi informada uma permissão inválida.");
            }

            perfil.Nome = nome;
            perfil.Permissoes = permissoes;
        }
    }
}
