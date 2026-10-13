using DOMAIN.Model.Funcionario;

namespace DOMAIN.Model.Perfil
{
    public static class PerfilPadraoUtil
    {
        public static IReadOnlyList<PerfilModel> Criar(string supermercadoId) =>
        [
            new()
            {
                Id = IdPadrao(supermercadoId, PerfilFuncionario.Gerente),
                Nome = "Gerente",
                SupermercadoId = supermercadoId,
                ChaveLegada = nameof(PerfilFuncionario.Gerente),
                Permissoes = [PermissaoUtil.Produtos, PermissaoUtil.Pedidos, PermissaoUtil.Financeiro]
            },
            new()
            {
                Id = IdPadrao(supermercadoId, PerfilFuncionario.OperadorEstoque),
                Nome = "Operador de Estoque",
                SupermercadoId = supermercadoId,
                ChaveLegada = nameof(PerfilFuncionario.OperadorEstoque),
                Permissoes = [PermissaoUtil.Produtos]
            },
            new()
            {
                Id = IdPadrao(supermercadoId, PerfilFuncionario.Atendente),
                Nome = "Atendente",
                SupermercadoId = supermercadoId,
                ChaveLegada = nameof(PerfilFuncionario.Atendente),
                Permissoes = [PermissaoUtil.Pedidos]
            }
        ];

        public static string IdPadrao(string supermercadoId, PerfilFuncionario perfil) =>
            $"{supermercadoId}_{perfil.ToString().ToLowerInvariant()}";
    }
}
