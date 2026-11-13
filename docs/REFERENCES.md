# Official reference checks

**Reference baseline:** unless a row says it was carried over from the starter kit, it was read on 2026-11-11 by an agent web fetch for lumabanls, after the platform changed to a web application. Those pages were requested with `view=aspnetcore-11.0`. Recheck at toolchain changes; this project's versions are selected in [SETUP](SETUP.md). These sources support the technical guidance. They are not evidence that the application works.

The MAUI and remote-API references inherited from the starter kit were removed with the platform change. They remain in the template repository and in this file's git history.

## Toolchain and platforms

| Source | Verified guidance / adoption implication |
|---|---|
| [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) | Page "Last updated: November 10, 2026". .NET 11: released 2026-11-10, latest patch 11.0.0, STS, end of support 2028-11-14. .NET 10: LTS, latest patch 10.0.15, end of support 2028-11-14. .NET 8 and 9 are out of support |
| [.NET 11 release notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/README.md) | 11.0.0 listed with date 2026/11/10. ".NET 11 is a Standard Term Support (STS) release and will be supported ... from November 10, 2026, to November 14, 2028." |
| [.NET 11 downloads](https://dotnet.microsoft.com/en-us/download/dotnet/11.0) | The page showed "SDK 11.0.100". Only part of the page was read word for word, so confirm the exact version with `dotnet --version` after installing |
| [Blazor supported platforms](https://learn.microsoft.com/en-us/aspnet/core/blazor/supported-platforms?view=aspnetcore-11.0) | Apple Safari, Google Chrome, Microsoft Edge and Mozilla Firefox, each "Current", meaning "the latest version of the browser". No minimum version numbers are given |
| [global.json](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) | Selects the .NET SDK independently of project target frameworks; exact version and roll-forward policy are configurable. Carried over from the kit's 2026-10-21 check |
| [NuGet dependency locking](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies) | A lock file records the resolved dependency graph; locked restore can reject a changed graph in CI. Carried over from the kit's 2026-10-21 check |

## Implementation and hosting

| Source | Verified guidance / adoption implication |
|---|---|
| [Blazor render modes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-11.0) | Modes: Static Server, Interactive Server, Interactive WebAssembly, Interactive Auto. "The default render mode is Static." Interactive modes "support prerendering by default", so an interactive page's start-up code can run twice |
| [Blazor forms overview](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/?view=aspnetcore-10.0) | Found by an agent web search on 2026-11-13, from the summary of the ASP.NET Core 10.0 page; check the 11.0 page at implementation. A form submitted by a statically rendered component needs a name (`FormName`), or the post fails when it arrives. `[SupplyParameterFromForm]` binds the posted values. Antiforgery tokens are validated only with `UseAntiforgery`; `EditForm` adds the token itself, while a plain HTML form needs the `AntiforgeryToken` component. Static forms are validated on the server after the post, with no client-side validation |
| [Blazor with EF Core](https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-11.0) | "DbContext isn't thread safe and isn't designed for concurrent use." "The recommended approach to create a new DbContext with dependencies is to use a factory." Registered with `AddDbContextFactory` |
| [Blazor security](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-11.0) | Authorization code running on the client "can't be trusted to absolutely enforce access rules"; enforce rules on the server. Antiforgery middleware needs an explicit `UseAntiforgery` call |
| [Threat mitigation for interactive server rendering](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/interactive-server-side-rendering?view=aspnetcore-11.0) | Sections include shared state, `IHttpContextAccessor`/`HttpContext`, resource exhaustion and denial of service. "Protect against multiple dispatches" of the same event. "don't permit clients to allocate an unbound amount of memory". "There's no limit on the number of connections per user for an app." |
| [Blazor SignalR guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-11.0) | On a lost connection "a default UI is displayed to the user while the client attempts to reconnect." "If reconnection fails, the user is instructed to retry or reload the page." With more than one server, "the app must implement session affinity" |
| [Server-side state management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/server?view=aspnetcore-11.0) | "When a user can't be reconnected to their original circuit, the user receives a new circuit with newly initialized state." Durable state belongs in server storage; per-tab state can use protected browser storage |
| [Google sign-in](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-11.0) | Package `Microsoft.AspNetCore.Authentication.Google` (`AddGoogle`), or `Google.Apis.Auth.AspNetCore3`. "The URI segment `/signin-google` is set as the default callback". Store the client ID and secret with Secret Manager under `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret` |
| [Microsoft account sign-in](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins?view=aspnetcore-11.0) | Package `Microsoft.AspNetCore.Authentication.MicrosoftAccount`. Secrets under `Authentication:Microsoft:ClientId` and `Authentication:Microsoft:ClientSecret`. The callback path is not stated on the page; confirm at implementation |
| [Host and deploy server-side Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/server/?view=aspnetcore-11.0) | "Blazor works best when using WebSockets as the SignalR transport". Memory is "likely the first resource that the app exhausts"; "Each circuit uses approximately 250 KB of memory for a minimal *Hello World*-style app." Azure App Service needs WebSockets and session affinity configured |
| [Data Protection configuration](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-11.0) | Change the default key storage when "The app is spread across multiple machines"; in containers keep keys in storage "that persists beyond the container's lifetime". The page has no explicit sentence about app restarts |
| [Applying EF Core migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying) | For production, prefer a migration bundle or an idempotent SQL script; "carefully consider" migrating at app startup. Carried over from the kit's 2026-11-09 check |
| [App secrets in development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0) | Secret Manager (`dotnet user-secrets`) doesn't encrypt and is for development only. Carried over from the kit's 2026-11-09 check |
| [Rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0) | `AddRateLimiter()`/`UseRateLimiter()`; counters are kept in memory per app instance. Carried over from the kit's 2026-11-09 check |
| [Integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0) | Package `Microsoft.AspNetCore.Mvc.Testing`; `WebApplicationFactory<TEntryPoint>` runs the app in memory on a test server. Carried over from the kit's 2026-11-09 check |

Not verified word for word on 2026-11-11, so treat as leads to check at implementation:
- that `HttpContext` is null during interactive rendering;
- default reconnection timings;
- pausing and resuming circuits.

## Hosting, database and email services

Read on 2026-11-11 by an agent web fetch for lumabanls, from each vendor's own pages unless a row says otherwise. Prices and limits change; confirm them before relying on them.

| Source | Verified guidance / adoption implication |
|---|---|
| [Vercel runtimes](https://vercel.com/docs/functions/runtimes) | Page dated 2026-08-12. Official runtimes: Node.js, Bun, Python, Rust, Go, Ruby, Wasm, Edge and Container. .NET is not listed |
| [Vercel Container Runtime](https://vercel.com/docs/functions/runtimes/container) | Page dated 2026-07-07. "Container Runtime is available in Beta on all plans." A function without traffic for 5 minutes in production "will automatically scale down" |
| [Vercel function limits](https://vercel.com/docs/functions/limitations) | Page dated 2026-08-24. Maximum duration: 300 seconds on Hobby, 800 seconds on Pro |
| [Vercel WebSockets](https://vercel.com/docs/functions/websockets) | Page dated 2026-08-10; generally available since 2026-10-20. "WebSocket connections close when a Vercel Function reaches its maximum duration." Shown for Node.js, Bun and Python only |
| [Supabase: connecting to Postgres](https://supabase.com/docs/guides/database/connecting-to-postgres) | The direct connection is for "persistent backends, such as virtual machines (VMs) and long-running containers" and uses IPv6 on the Free plan. The shared pooler in session mode (port 5432) works over IPv4. Transaction mode (port 6543) is for "serverless or edge functions", and "The shared pooler doesn't support prepared statements in transaction mode." |
| [Supabase pricing](https://supabase.com/pricing) | Free: "500 MB database size"; "Free projects are paused after 1 week of inactivity. Limit of 2 active projects." Pro: "from $25/month" |
| [Supabase regions](https://supabase.com/docs/guides/platform/regions) | "Southeast Asia (Singapore), `ap-southeast-1`" is available |
| [Supabase social login](https://supabase.com/docs/guides/auth/social-login) | Google and "Azure (Microsoft)" are listed. Not used by Tax Buddy, which signs users in itself ([ADR 006](adr/006-web-sign-in.md)) |
| [Supabase C# reference](https://supabase.com/docs/reference/csharp/introduction) | A `supabase` NuGet library exists for Supabase's web APIs. No Supabase page about EF Core or Npgsql was found, so that pairing rests on PostgreSQL compatibility and must be proven at scaffold |
| [Render free instances](https://render.com/docs/free) | "Render spins down a Free web service that goes 15 minutes without receiving any inbound traffic." Waking takes "about one minute". 750 free instance hours a month. Not for production |
| [Render WebSockets](https://render.com/docs/websocket), [Docker](https://render.com/docs/docker), [regions](https://render.com/docs/regions) | "Render does not enforce a maximum duration for WebSocket connections." "Render fully supports Docker-based deploys." Singapore is a region. Paid prices were not readable |
| [Fly.io pricing](https://docs.fly.io/about/pricing), [regions](https://docs.fly.io/reference/regions) | No free tier. A `shared-cpu-1x` machine with 512 MB costs "$3.69/month"; "if your personal organization's monthly bill comes to less than $5, we waive it." Singapore (`sin`) is a region |
| [Railway pricing](https://railway.com/pricing), [regions](https://docs.railway.com/reference/regions) | Hobby plan $5 a month plus per-second usage. Singapore is a region. Docker and WebSocket support were not stated on the pages read |
| [Azure App Service pricing](https://azure.microsoft.com/en-us/pricing/details/app-service/linux/) | Free tier: "Shared (60 CPU minutes / day)", and "Use of free plan for production workloads is not supported." The Basic tier's price was not readable, and no page read mentions .NET 11 |
| [Brevo pricing](https://www.brevo.com/pricing/) and [SendGrid pricing](https://sendgrid.com/en-us/pricing) | Neither page could be read. Third-party comparison pages found by web search agree on these figures: Brevo has an ongoing free tier of about 300 emails a day that includes sending through its API; SendGrid offers a 60-day trial and then plans from about $19.95 a month. Treat both as unconfirmed |

## Revalidation log

| Date | Topic / source | Chosen version or decision | Verified by / affected docs |
|---|---|---|---|
| 2026-11-13 | [Blazor forms overview](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/?view=aspnetcore-10.0) (10.0 page) | Signed-in actions on static pages use named forms posted to the page (Proposed) | Agent web search summary, not a full read; [ADR 010](adr/010-single-project-per-page-render-modes.md), [SDD](SDD.md#render-modes) |
| 2026-11-10 | MAUI support policy, .NET 11 release notes, .NET support policy | .NET 11 chosen for the MAUI plan; pinning waited for GA because only RC3 was listed. MAUI 10 (10.0.120) was the only supported MAUI line, ending 2027-05-11. Superseded by the platform change | Agent web fetch for lumabanls; [ADR 003](adr/003-dotnet-11-toolchain.md) |
| 2026-11-10 | MAUI Web authenticator documentation | WebAuthenticator documented as not working on Windows (conflict C01). No longer applies | Agent web fetch; [ADR 004](adr/004-external-sign-in.md) |
| 2026-11-10 | Workload sets documentation | Workload-set pinning for MAUI. No longer applies | Agent web fetch |
| 2026-11-11 | [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), [.NET 11 release notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/README.md) | .NET 11.0.0 is released. Owner chose .NET 11 for the web application | Agent web fetch; [ADR 005](adr/005-web-platform-blazor.md), [SETUP](SETUP.md) |
| 2026-11-11 | The Blazor and ASP.NET Core pages in the tables above | Render-mode split, context factory, sign-in handlers, hosting needs | Agent web fetch; [SDD](SDD.md), [DEVELOPMENT](DEVELOPMENT.md), [RELEASE](RELEASE.md), [ADR 006](adr/006-web-sign-in.md) |
| 2026-11-11 | The Vercel, Supabase, Render, Fly.io, Railway, Azure and email pages in [Hosting, database and email services](#hosting-database-and-email-services) | Supabase for the database and Brevo for email. Vercel ruled out as the host; the replacement host is open, with Render proposed first | Agent web fetch and web search; [ADR 007](adr/007-supabase-brevo-container-host.md), [SDD](SDD.md), [SETUP](SETUP.md), [RELEASE](RELEASE.md) |

Support dates, browser versions and provider sign-in requirements change. Cite the current official guidance when adding a hosting target or going public.
