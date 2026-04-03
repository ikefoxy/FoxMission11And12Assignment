using Microsoft.Extensions.FileProviders;
using backend.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IBookRepository, SqliteBookRepository>();

var app = builder.Build();

// React production build output that ASP.NET will serve as static files.
var frontendDistCandidates = new[]
{
    Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "frontend", "dist")),
    Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "wwwroot"))
};
var frontendDistPath = frontendDistCandidates.FirstOrDefault(Directory.Exists);

app.MapControllers();

if (!string.IsNullOrWhiteSpace(frontendDistPath) && Directory.Exists(frontendDistPath))
{
    var frontendFileProvider = new PhysicalFileProvider(frontendDistPath);

    // Serve index.html and static assets from the built React app.
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = frontendFileProvider
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = frontendFileProvider
    });

    // Let client-side routes resolve to React's index.html.
    app.MapFallback(async context =>
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(frontendDistPath, "index.html"));
    });
}
else
{
    app.MapGet("/", () =>
        Results.Text("Frontend build not found. Run: cd frontend && npm run build", "text/plain"));
}

app.Run();
