using CS4090_Project.Components;
using Db;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

// using (var context = new DatabaseContext(new DbContextOptions<DatabaseContext>())) { context.Database.EnsureCreated(); }

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContextFactory<DatabaseContext>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AppAuthenticator>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AppAuthenticator>());
builder.Services.AddAuthentication();

var app = builder.Build();
app.UseAuthentication();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
