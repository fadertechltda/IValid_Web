using Google.Cloud.Firestore;
using REPOSITORY.Mapeadores.Produto; // Adicione o namespace correto dos seus mapeadores

var builder = WebApplication.CreateBuilder(args);

// Pega os dados do appsettings.json
var firebasePath = builder.Configuration["Firebase:CredentialsPath"];
var projectId = builder.Configuration["Firebase:ProjectId"];

if (!string.IsNullOrEmpty(firebasePath))
{
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebasePath);
}

// Registro do Banco (Singleton é melhor para o Firebase)
builder.Services.AddSingleton(_ => FirestoreDb.Create(projectId));

// REGISTRO DOS SEUS MAPEADORES (Fundamental para não dar erro na Controller)
builder.Services.AddScoped<IProdutoMapeador, ProdutoMapeador>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
