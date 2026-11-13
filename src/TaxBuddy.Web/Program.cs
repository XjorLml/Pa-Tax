using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Components;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Services;

// "seed-development" is a developer command, not a host setting (SETUP: local development).
var seedCommand = args.Length > 0 && args[0] == DevelopmentSeed.Command;
var builder = WebApplication.CreateBuilder(seedCommand ? args[1..] : args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SDD: one short-lived context per operation, from a factory. The connection string is read when a context is
// first configured, so test hosts can supply their own.
builder.Services.AddDbContextFactory<TaxBuddyDbContext>((services, options) =>
    options.UseNpgsql(services.GetRequiredService<IConfiguration>().GetConnectionString("TaxBuddy")));

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IDirectoryStore, DirectoryStore>();
builder.Services.AddScoped<DirectoryService>();

var app = builder.Build();

if (string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("TaxBuddy")))
{
    throw new InvalidOperationException(
        "ConnectionStrings:TaxBuddy is not set. In development store it with dotnet user-secrets (SETUP: local development); " +
        "on a deployed site use the host's secret store.");
}

if (seedCommand)
{
    if (!app.Environment.IsDevelopment())
    {
        Console.Error.WriteLine($"{DevelopmentSeed.Command} runs only in the Development environment.");
        return 1;
    }

    try
    {
        var (listed, unlisted) = await DevelopmentSeed.ResetAndSeedAsync(
            app.Services.GetRequiredService<IDbContextFactory<TaxBuddyDbContext>>());
        Console.WriteLine($"Seeded {listed} listed and {unlisted} unlisted synthetic profiles.");
        return 0;
    }
    catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
    {
        Console.Error.WriteLine("The tables do not exist yet. Run 'dotnet ef database update --project src/TaxBuddy.Web' first.");
        return 1;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. Revisit before a public launch (RELEASE).
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// SDD search engines: public pages ask not to be indexed until PRD D19 is decided, and always on the test site.
if (!app.Configuration.GetValue<bool>("Seo:AllowIndexing"))
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
        await next(context);
    });
}

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
return 0;

/// <summary>Entry point; public so integration tests can host the app with WebApplicationFactory.</summary>
public partial class Program;
