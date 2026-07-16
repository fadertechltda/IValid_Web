var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("IValidApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7183/");
});

builder.Services.AddControllersWithViews()
    .AddMvcOptions(options =>
    {
        options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((valor, _) => $"O valor '{valor}' é inválido.");
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Este campo é obrigatório.");
        options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
        options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => "O valor é obrigatório.");
    });

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
