using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

var firebasePath = builder.Configuration["Firebase:CredentialsPath"];
var projectId = builder.Configuration["Firebase:ProjectId"];

if (!string.IsNullOrEmpty(firebasePath))
{
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebasePath);
}

builder.Services.AddScoped(_ => FirestoreDb.Create(projectId));
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