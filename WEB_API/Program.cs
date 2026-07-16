using DOMAIN.Validador.Produto;
using Google.Cloud.Firestore;
using REPOSITORY.Mapeadores.Produto;
using SERVICE.Fachada;
using SERVICE.Processo;
using Scalar.AspNetCore; 

var builder = WebApplication.CreateBuilder(args);

var firebasePath = builder.Configuration["Firebase:CredentialsPath"];
var projectId = builder.Configuration["Firebase:ProjectId"];

if (!string.IsNullOrEmpty(firebasePath))
{
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebasePath);
}

builder.Services.AddSingleton(_ => FirestoreDb.Create(projectId));

builder.Services.AddScoped<IProdutoMapeador, ProdutoMapeador>();
builder.Services.AddScoped<ProdutoProcesso>();
builder.Services.AddScoped<ProdutoFachada>();
builder.Services.AddScoped<ProdutoValidacao>();

builder.Services.AddScoped<REPOSITORY.Mapeadores.Usuario.IUsuarioMapeador, REPOSITORY.Mapeadores.Usuario.UsuarioMapeador>();
builder.Services.AddScoped<UsuarioProcesso>();
builder.Services.AddScoped<UsuarioFachada>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 

    app.MapScalarApiReference();
}


app.UseAuthorization();
app.MapControllers();

app.Run();
