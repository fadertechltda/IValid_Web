using DOMAIN.Validador.Produto;
using DOMAIN.Validador.Configuracao;
using Google.Cloud.Firestore;
using REPOSITORY.Mapeadores.Produto;
using REPOSITORY.Mapeadores.Usuario;
using REPOSITORY.Mapeadores.Configuracao;
using REPOSITORY.Mapeadores.Pedido;
using SERVICE.Fachada;
using SERVICE.Processo;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var firebasePath = builder.Configuration["Firebase:CredentialsPath"];
var projectId = builder.Configuration["Firebase:ProjectId"];

if (!string.IsNullOrEmpty(firebasePath))
{
    var caminhoAbsoluto = Path.GetFullPath(firebasePath, builder.Environment.ContentRootPath);
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", caminhoAbsoluto);
}

builder.Services.AddSingleton(_ => FirestoreDb.Create(projectId));

builder.Services.AddHttpClient();

builder.Services.AddScoped<IProdutoMapeador, ProdutoMapeador>();
builder.Services.AddScoped<ProdutoProcesso>();
builder.Services.AddScoped<ProdutoFachada>();
builder.Services.AddScoped<ProdutoValidacao>();

builder.Services.AddScoped<IUsuarioMapeador, UsuarioMapeador>();
builder.Services.AddScoped<UsuarioProcesso>();
builder.Services.AddScoped<UsuarioFachada>();

builder.Services.AddScoped<IConfiguracaoMapeador, ConfiguracaoMapeador>();
builder.Services.AddScoped<ConfiguracaoProcesso>();
builder.Services.AddScoped<ConfiguracaoFachada>();
builder.Services.AddScoped<ConfiguracaoValidacao>();

builder.Services.AddScoped<IPedidoMapeador, PedidoMapeador>();
builder.Services.AddScoped<PedidoProcesso>();
builder.Services.AddScoped<PedidoFachada>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

var apiKeyInterna = builder.Configuration["Seguranca:ApiKeyInterna"];

app.Use(async (context, next) =>
{
    var caminho = context.Request.Path.Value ?? string.Empty;
    var ehRotaDeDocumentacao = caminho.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase)
                             || caminho.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase);

    if (!ehRotaDeDocumentacao)
    {
        if (string.IsNullOrEmpty(apiKeyInterna))
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("A API não está configurada corretamente (chave interna ausente). Configure 'Seguranca:ApiKeyInterna'.");
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Internal-Api-Key", out var chaveRecebida) ||
            chaveRecebida != apiKeyInterna)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Acesso negado: esta API é de uso interno.");
            return;
        }
    }

    await next();
});

app.UseAuthorization();
app.MapControllers();

app.Run();
