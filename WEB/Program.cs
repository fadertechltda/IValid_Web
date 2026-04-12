using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

var firebasePath = builder.Configuration["Firebase:CredentialsPath"];
var projectId = builder.Configuration["Firebase:ProjectId"];

if (!string.IsNullOrEmpty(firebasePath))
{
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", firebasePath);
}

builder.Services.AddScoped(_ => FirestoreDb.Create(projectId));
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