namespace DOMAIN.Model.Perfil
{
    public record PermissaoDescricao(string Chave, string Nome, string Descricao);

    public static class PermissaoUtil
    {
        public const string TipoClaim = "Permissao";
        public const string PerfilAdministrador = "Administrador";
        public const string PerfilFuncionario = "Funcionario";

        public const string Produtos = "produtos";
        public const string Pedidos = "pedidos";
        public const string Financeiro = "financeiro";
        public const string Configuracoes = "configuracoes";
        public const string Categorias = "categorias";
        public const string Ongs = "ongs";

        public static readonly IReadOnlyList<PermissaoDescricao> Todas =
        [
            new(Produtos, "Produtos", "Cadastrar, editar e excluir produtos."),
            new(Pedidos, "Pedidos", "Ver os pedidos e mudar o status."),
            new(Financeiro, "Financeiro", "Ver faturamento, lucro e valor em risco no dashboard."),
            new(Configuracoes, "Configurações", "Alterar descontos e dados da loja."),
            new(Categorias, "Categorias", "Cadastrar categorias de produto."),
            new(Ongs, "ONGs", "Cadastrar as ONGs parceiras.")
        ];

        public static IEnumerable<string> Chaves => Todas.Select(p => p.Chave);

        public static bool EhValida(string? chave) => Todas.Any(p => p.Chave == chave);
    }
}
