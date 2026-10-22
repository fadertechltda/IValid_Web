using DOMAIN.Validador.Produto;
using DOMAIN.Validador.Configuracao;
using DOMAIN.Validador.Funcionario;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.HttpOverrides;
using REPOSITORY.Mapeadores.Produto;
using REPOSITORY.Mapeadores.Usuario;
using REPOSITORY.Mapeadores.Configuracao;
using REPOSITORY.Mapeadores.Pedido;
using REPOSITORY.Mapeadores.Supermercado;
using REPOSITORY.Mapeadores.Funcionario;
using SERVICE.Fachada;
using SERVICE.Processo;
using Scalar.AspNetCore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using REPOSITORY.Mapeadores.Categoria;
using REPOSITORY.Mapeadores.Cliente;
using REPOSITORY.Mapeadores.Ong;
using REPOSITORY.Mapeadores.Perfil;
using REPOSITORY.Mapeadores.Rotina;
using REPOSITORY.Mapeadores.Transacao;
using SERVICE.Integracao;
using SERVICE.Opcoes;
using WEB_API.Infraestrutura;

var builder = WebApplication.CreateBuilder(args);

var firebasePath = builder.Configuration["Firebase:CredentialsPath"];
var projectId = builder.Configuration["Firebase:ProjectId"];

if (!string.IsNullOrEmpty(firebasePath))
{
    var caminhoAbsoluto = Path.GetFullPath(firebasePath, builder.Environment.ContentRootPath);
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", caminhoAbsoluto);
}

builder.Services.AddSingleton(_ => new FirestoreDbBuilder
{
    ProjectId = projectId,
    EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOrProduction
}.Build());

builder.Services.AddHttpClient();

builder.Services.AddScoped<IProdutoMapeador, ProdutoMapeador>();
builder.Services.AddScoped<ProdutoProcesso>();
builder.Services.AddScoped<ProdutoFachada>();
builder.Services.AddScoped<ProdutoValidacao>();

var emailOpcoes = builder.Configuration.GetSection("Email").Get<EmailOpcoes>() ?? new EmailOpcoes();
builder.Services.AddSingleton(emailOpcoes);
builder.Services.AddHttpClient<EmailProcesso>(cliente =>
{
    cliente.BaseAddress = new Uri("https://api.resend.com/");
    cliente.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddScoped<IUsuarioMapeador, UsuarioMapeador>();
builder.Services.AddScoped<UsuarioProcesso>();
builder.Services.AddScoped<UsuarioFachada>();

builder.Services.AddScoped<ISupermercadoMapeador, SupermercadoMapeador>();
builder.Services.AddScoped<SupermercadoProcesso>();

builder.Services.AddScoped<IFuncionarioMapeador, FuncionarioMapeador>();
builder.Services.AddScoped<FuncionarioProcesso>();
builder.Services.AddScoped<FuncionarioFachada>();
builder.Services.AddScoped<FuncionarioValidacao>();

builder.Services.AddScoped<IConfiguracaoMapeador, ConfiguracaoMapeador>();
builder.Services.AddScoped<ConfiguracaoProcesso>();
builder.Services.AddScoped<ConfiguracaoFachada>();
builder.Services.AddScoped<ConfiguracaoValidacao>();

builder.Services.AddScoped<IPedidoMapeador, PedidoMapeador>();
builder.Services.AddScoped<PedidoProcesso>();
builder.Services.AddScoped<PedidoFachada>();

builder.Services.AddScoped<DashboardProcesso>();
builder.Services.AddScoped<DashboardFachada>();

var pagamentoOpcoes = builder.Configuration.GetSection("Pagamento").Get<PagamentoOpcoes>() ?? new PagamentoOpcoes();
var notificacaoOpcoes = builder.Configuration.GetSection("Notificacoes").Get<NotificacaoOpcoes>() ?? new NotificacaoOpcoes();
var mercadoPagoOpcoes = builder.Configuration.GetSection("MercadoPago").Get<MercadoPagoOpcoes>() ?? new MercadoPagoOpcoes();
builder.Services.AddSingleton(pagamentoOpcoes);
builder.Services.AddSingleton(mercadoPagoOpcoes);
builder.Services.AddSingleton(builder.Configuration.GetSection("Repasse").Get<RepasseOpcoes>() ?? new RepasseOpcoes());
builder.Services.AddSingleton(notificacaoOpcoes);

builder.Services.AddScoped<ICategoriaMapeador, CategoriaMapeador>();
builder.Services.AddScoped<IOngMapeador, OngMapeador>();
builder.Services.AddScoped<IClienteMapeador, ClienteMapeador>();
builder.Services.AddScoped<ITransacaoMapeador, TransacaoMapeador>();
builder.Services.AddScoped<IRotinaMapeador, RotinaMapeador>();

builder.Services.AddScoped<IPerfilMapeador, PerfilMapeador>();
builder.Services.AddScoped<PerfilProcesso>();
builder.Services.AddScoped<PerfilFachada>();

builder.Services.AddScoped<VitrineProcesso>();
builder.Services.AddScoped<CategoriaProcesso>();
builder.Services.AddScoped<OngProcesso>();
builder.Services.AddScoped<ClienteProcesso>();
builder.Services.AddScoped<INotificacaoProcesso, NotificacaoProcesso>();
builder.Services.AddScoped<PagamentoProcesso>();
builder.Services.AddScoped<RotinaProcesso>();
builder.Services.AddScoped<RepasseProcesso>();
builder.Services.AddScoped<RepasseFachada>();

builder.Services.AddScoped<VitrineFachada>();
builder.Services.AddScoped<ClienteFachada>();
builder.Services.AddScoped<CategoriaFachada>();
builder.Services.AddScoped<OngFachada>();
builder.Services.AddScoped<SupermercadoFachada>();

builder.Services.AddHttpClient<InfinitePayCliente>(cliente =>
{
    cliente.BaseAddress = new Uri(pagamentoOpcoes.InfinitePayBaseUrl.TrimEnd('/') + "/");
    cliente.Timeout = TimeSpan.FromSeconds(20);
});

builder.Services.AddHttpClient<MercadoPagoCliente>(cliente =>
{
    cliente.BaseAddress = new Uri(mercadoPagoOpcoes.BaseUrl.TrimEnd('/') + "/");
    cliente.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHostedService<RotinaDiariaService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        opcoes.Authority = $"https://securetoken.google.com/{projectId}";
        opcoes.MapInboundClaims = false;
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{projectId}",
            ValidateAudience = true,
            ValidAudience = projectId,
            ValidateLifetime = true,
            NameClaimType = "user_id"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddOpenApi(opcoes => opcoes.AddDocumentTransformer<DocumentacaoSegurancaTransformer>());

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (FirebaseApp.DefaultInstance == null)
{
    try
    {
        FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.GetApplicationDefault(), ProjectId = projectId });
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "FirebaseApp não inicializado: as notificações push ficarão desativadas.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();

var apiKeyInterna = builder.Configuration["Seguranca:ApiKeyInterna"];

app.Use(async (context, next) =>
{
    var caminho = context.Request.Path.Value ?? string.Empty;
    var ehRotaDeDocumentacao = caminho.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase)
                             || caminho.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase);
    var ehRotaDoAplicativo = caminho.StartsWith("/api/app/", StringComparison.OrdinalIgnoreCase)
                          || caminho.Equals("/api/app", StringComparison.OrdinalIgnoreCase);

    if (!ehRotaDeDocumentacao && !ehRotaDoAplicativo)
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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
