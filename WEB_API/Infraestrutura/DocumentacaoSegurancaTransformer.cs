using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WEB_API.Infraestrutura
{
    public class DocumentacaoSegurancaTransformer : IOpenApiDocumentTransformer
    {
        private const string EsquemaFirebase = "FirebaseBearer";
        private const string EsquemaChaveInterna = "ChaveInterna";

        public Task TransformAsync(OpenApiDocument documento, OpenApiDocumentTransformerContext contexto, CancellationToken cancellationToken)
        {
            documento.Info.Title = "IValid API";
            documento.Info.Description =
                "Rotas `api/app/*`: consumidas pelo aplicativo, autenticadas com o ID token do Firebase Authentication (`Authorization: Bearer <token>`); " +
                "as rotas de vitrine (produtos, categorias, supermercados e ONGs) são públicas. " +
                "Demais rotas: uso exclusivo do painel, exigem o header `X-Internal-Api-Key`.";

            documento.Components ??= new OpenApiComponents();
            documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            documento.Components.SecuritySchemes[EsquemaFirebase] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "ID token do Firebase Authentication obtido no app com `FirebaseAuth.instance.currentUser.getIdToken()`."
            };

            documento.Components.SecuritySchemes[EsquemaChaveInterna] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-Internal-Api-Key",
                Description = "Chave interna usada apenas pelo painel administrativo."
            };

            foreach (var (caminho, item) in documento.Paths)
            {
                bool ehApp = caminho.StartsWith("/api/app", StringComparison.OrdinalIgnoreCase);
                var esquema = new OpenApiSecuritySchemeReference(ehApp ? EsquemaFirebase : EsquemaChaveInterna, documento);

                if (item.Operations == null) continue;

                foreach (var operacao in item.Operations.Values)
                {
                    operacao.Security ??= [];
                    operacao.Security.Add(new OpenApiSecurityRequirement { [esquema] = [] });
                }
            }

            return Task.CompletedTask;
        }
    }
}
