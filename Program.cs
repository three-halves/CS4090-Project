using CS4090_Project.Components;
using Db;
using Microsoft.EntityFrameworkCore;

using (var context = new DatabaseContext(new DbContextOptions<DatabaseContext>())) { context.Database.EnsureCreated(); }

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContextFactory<DatabaseContext>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
