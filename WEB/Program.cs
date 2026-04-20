var builder = WebApplication.CreateBuilder(args);

// REMOVIDO: O projeto WEB não usa mais a chave do Firebase!
// ADICIONADO: Configuração para falar com a API
builder.Services.AddHttpClient("IValidApi", client =>
{
    // Coloque aqui a URL onde a sua WEB_API roda (ex: https://localhost:7001/)
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001/");
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
