using System.Security.Claims;
using DOMAIN.Model.Perfil;
using Microsoft.AspNetCore.Authorization;

namespace WEB.Seguranca
{
    public static class PermissaoExtensions
    {
        public static bool EhAdministrador(this ClaimsPrincipal usuario) =>
            usuario.IsInRole(PermissaoUtil.PerfilAdministrador);

        public static bool TemPermissao(this ClaimsPrincipal usuario, string permissao) =>
            usuario.EhAdministrador() || usuario.HasClaim(PermissaoUtil.TipoClaim, permissao);

        public static string NomePerfil(this ClaimsPrincipal usuario) =>
            usuario.FindFirst("NomePerfil")?.Value ?? (usuario.EhAdministrador() ? PermissaoUtil.PerfilAdministrador : "Funcionário");

        public static void AdicionarPoliticasDePermissao(this AuthorizationOptions opcoes)
        {
            foreach (var permissao in PermissaoUtil.Chaves)
            {
                opcoes.AddPolicy(permissao, politica => politica.RequireAssertion(contexto => contexto.User.TemPermissao(permissao)));
            }
        }
    }
}
