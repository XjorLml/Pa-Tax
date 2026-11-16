using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace TaxBuddy.Web.Tests.Support;

/// <summary>
/// The test sign-in scheme (ADR 006, TEST-PLAN): a test-only address that leaves the browser exactly where Google's
/// handler would, with Identity's external cookie holding a Google identity. Registered only by the test factory.
/// </summary>
public sealed class TestExternalSignIn : IStartupFilter
{
    public const string Path = "/test/external-signin";

    public static string Address(string key, string email, string name) =>
        $"{Path}?key={Uri.EscapeDataString(key)}&email={Uri.EscapeDataString(email)}&name={Uri.EscapeDataString(name)}";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        // Not app.Map: that would make this address the path base, and the cookie would be limited to it.
        app.Use(async (context, nextMiddleware) =>
        {
            if (context.Request.Path != Path)
            {
                await nextMiddleware(context);
                return;
            }

            var query = context.Request.Query;
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, query["key"].ToString()),
                new Claim(ClaimTypes.Email, query["email"].ToString()),
                new Claim(ClaimTypes.Name, query["name"].ToString()),
            ], "Google");
            var properties = new AuthenticationProperties();
            // What SignInManager reads back in GetExternalLoginInfoAsync.
            properties.Items["LoginProvider"] = "Google";
            await context.SignInAsync(IdentityConstants.ExternalScheme, new ClaimsPrincipal(identity), properties);
            context.Response.StatusCode = StatusCodes.Status204NoContent;
        });
        next(app);
    };
}

/// <summary>Keeps every log message, so tests can check that personal data is never logged (F04 GS10).</summary>
public sealed class CapturedLogs : ILoggerProvider
{
    private readonly ConcurrentQueue<string> messages = new();

    public IReadOnlyCollection<string> Messages => messages;

    public ILogger CreateLogger(string categoryName) => new Logger(categoryName, messages);

    public void Dispose()
    {
    }

    private sealed class Logger(string category, ConcurrentQueue<string> messages) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            messages.Enqueue($"{logLevel} {category}: {formatter(state, exception)} {exception}");
    }
}
