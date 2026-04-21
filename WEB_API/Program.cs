using DOMAIN.Validador.Produto;
using Google.Cloud.Firestore;
using REPOSITORY.Mapeadores.Produto;
using SERVICE.Fachada;
using SERVICE.Processo;
using Scalar.AspNetCore; // <--- Importante para a interface visual!

var builder = WebApplication.CreateBuilder(args);

// 1. Configurações do Firebase (Lendo do appsettings.json)
var firebasePath = builder.Configuration["Firebase:CredentialsPath"];
var projectId = builder.Configuration["Firebase:ProjectId"];

if (!string.IsNullOrEmpty(firebasePath))
{
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebasePath);
}

// 2. Injeção de Dependências - Servicos e Banco
builder.Services.AddSingleton(_ => FirestoreDb.Create(projectId));

// Agrupando todas as injeções da sua arquitetura
builder.Services.AddScoped<IProdutoMapeador, ProdutoMapeador>();
builder.Services.AddScoped<ProdutoProcesso>();
builder.Services.AddScoped<ProdutoFachada>();
builder.Services.AddScoped<ProdutoValidacao>();

builder.Services.AddControllers();

// Ativa a geração da documentação OpenAPI (padrão .NET 9)
builder.Services.AddOpenApi();

var app = builder.Build();

// 3. Configuração do Pipeline (Ordem importa aqui!)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Gera o JSON da API

    // ATIVA A INTERFACE VISUAL (Acesse no navegador: /scalar/v1)
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
