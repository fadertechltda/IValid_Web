using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

var apiKeyInterna = builder.Configuration["Seguranca:ApiKeyInterna"];

builder.Services.AddHttpClient("IValidApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7183/");
    if (!string.IsNullOrEmpty(apiKeyInterna))
    {
        client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKeyInterna);
    }
});

builder.Services.AddControllersWithViews(options =>
    {
        var politicaPadrao = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(politicaPadrao));
    })
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

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
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
