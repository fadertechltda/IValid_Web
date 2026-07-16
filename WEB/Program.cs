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

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Autenticacao/Login";
        options.LogoutPath = "/Autenticacao/Logout";
        options.AccessDeniedPath = "/Autenticacao/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
