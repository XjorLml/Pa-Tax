# Local setup and toolchain

**Authority:** selected versions, build hosts, configuration, and developer commands. [SDD](SDD.md) owns browser and hosting capabilities; [RELEASE](RELEASE.md) owns deployment.

State on 2026-11-13: the solution exists, and F01 builds, tests and runs in WSL2 on the development PC ([evidence](delivery/evidence/2026-11-13-f01-browse-accountants.md)). The commands below were executed there. The Windows inventory of 2026-11-11 is kept further down for reference.

## Reproducibility record

### Development host: WSL2 (current, 2026-11-13)

| Item | Selected value | Source / checked date |
|---|---|---|
| Host | WSL2 Ubuntu 24.04.1 (kernel 5.15.167.4-microsoft-standard-WSL2) on Windows 10.0.26300. No IPv6, so the database is reached through Supabase's session pooler. **2026-11-16:** the WSL distribution found was Ubuntu 26.04 LTS (kernel 6.6.114.1-microsoft-standard-WSL2) with no `~/.dotnet`, no `DOTNET_ROOT` in `~/.bashrc`, no user-secrets, no Linux Node and no `libicu`. The same day the user installed `libicu78` 78.2-2ubuntu1, and the SDK was reinstalled (row below). Re-record this row once the environment is settled | `uname -r`, `ip -6 route`, 2026-11-13; `/etc/os-release`, 2026-11-16 |
| .NET SDK | 11.0.100, user-level in `~/.dotnet` beside SDK 10.0.404, installed with Microsoft's `dotnet-install.sh --version 11.0.100`. `~/.bashrc` sets `DOTNET_ROOT` and `PATH`. Runtimes 11.0.0 (ASP.NET Core and .NET). 2026-11-16: reinstalled the same way in the Ubuntu 26.04 distribution (SDK 11.0.100 only); `~/.bashrc` again sets `DOTNET_ROOT` and `PATH` (checked in a new shell). Windows also has SDK 11.0.100 | `dotnet --list-sdks`, `dotnet --list-runtimes`, 2026-11-12 and 2026-11-16 |
| `global.json` | `11.0.100`, `rollForward: disable`, `allowPrerelease: false` | Repository root, 2026-11-13 |
| Solution | `TaxBuddy.slnx`: `src/TaxBuddy.Web`, `tests/TaxBuddy.Tests`, `tests/TaxBuddy.Web.Tests`. Shared settings in `Directory.Build.props` (nullable on, implicit usings, warnings treated as errors) and `.editorconfig` | Scaffold, 2026-11-13 |
| Data access | EF Core **10.0.15** (long-term support) with `Npgsql.EntityFrameworkCore.PostgreSQL` **10.0.3**. Npgsql's EF Core 11 provider was only `11.0.0-rc.3` on 2026-11-13, so the stable 10.x pair is used on .NET 11. Move both to 11 when Npgsql ships 11.0.0 | `dotnet package search`, 2026-11-13 |
| EF command-line tool | `dotnet-ef` 10.0.15 as a local tool in `dotnet-tools.json` at the repository root; `dotnet tool restore` installs it | Scaffold, 2026-11-13 |
| Tests | xUnit 2.9.3, xunit.runner.visualstudio 3.1.5, Microsoft.NET.Test.Sdk 17.14.1 (the template's versions); Microsoft.AspNetCore.Mvc.Testing 11.0.0 | Project files, 2026-11-13 |
| Styles | Tailwind CSS **4.3.3** with `@tailwindcss/cli` 4.3.3 through npm (lock file `src/TaxBuddy.Web/package-lock.json`). 4.3.5 was a week old on 2026-11-13, so the older stable release was pinned. Node 24.16.0 and npm 11.13.0 in WSL; Node 22+ is enough | `npm view`, 2026-11-13 |
| Container tooling | Docker is not installed. Not needed for development or tests: data tests use throwaway schemas in the development database | 2026-11-13 |

### Windows inventory (2026-11-11, for reference)

| Item | Selected value | Source / checked date |
|---|---|---|
| .NET SDK | .NET 11. The download page listed SDK 11.0.100 on 2026-11-11; record the exact version from `dotnet --version` after installing. Installed today: 8.0.307, 8.0.427, 9.0.123, 9.0.319, 9.0.320, 10.0.400, 10.0.403 | [REFERENCES](REFERENCES.md#revalidation-log); `dotnet --list-sdks`, 2026-11-11 |
| ASP.NET Core runtime | 11.0.x, installed with the SDK. Installed today: 8.0.x, 9.0.x, 10.0.11, 10.0.14 | `dotnet --list-runtimes`, 2026-11-11 |
| Target framework | `net11.0` for every project | [ADR 005](adr/005-web-platform-blazor.md) |
| Workloads | None needed. The MAUI workloads on this PC are not used by this project | — |
| IDE/extensions | VS Code 1.145.2 with C# Dev Kit 3.40.215 and the C# extension 2.160.4, which the owner asked to use. Visual Studio 2026 18.11.3 and 2022 17.14.44 are installed and not needed | `code --list-extensions` and `vswhere`, 2026-11-11 |
| Browsers for local checks | Supported: Chrome, Edge, Firefox, Brave and Safari (PRD D17). Installed on this PC: Chrome 157.0.8103.39, Edge 157.0.4341.46, Firefox 159.0.1, Brave 157.1.99.56. Safari needs an Apple device. Versions are recorded with each test run | File versions, 2026-11-11 |
| HTTPS development certificate | A valid certificate exists (expires 2027-08-26). Whether it is trusted was not checked | `dotnet dev-certs https --check`, 2026-11-11 |
| Database | Supabase (managed PostgreSQL), through EF Core and the Npgsql provider; package versions are recorded at scaffold. Development project created by the user on 2026-11-13 on the free plan: Southeast Asia (Singapore) `ap-southeast-1`, PostgreSQL 17.11, Nano compute, automatic row-level security switched on at creation. Whether the Data API is off is not yet confirmed. No test-site project yet | [ADR 007](adr/007-supabase-brevo-container-host.md), 2026-11-11; [connection check](delivery/evidence/2026-11-13-supabase-dev-connection.md), 2026-11-13 |
| Email | Brevo. No account exists yet. Development sends no real email | [ADR 007](adr/007-supabase-brevo-container-host.md), 2026-11-11 |
| Container tooling | Docker is not installed on this PC. It is needed only if the chosen host cannot build the image itself | Filesystem, 2026-11-11 |
| Package sources / dependencies | nuget.org and, for Tailwind only, npmjs.org. Explicit versions for every package. Decided at scaffold on 2026-11-13: no NuGet central package management or NuGet lock files yet (three projects), and the npm lock file is committed | Project files, 2026-11-13 |
| Documentation checker | Python 3.10+. On this PC, `python` and `py` are not on PATH; uv-managed CPython 3.14.4 is used through `uv run --no-project python` | Shell, 2026-11-10 |
| Source control | git 2.56.0.windows.1. GitHub CLI not installed (needed only for GitHub/CI work). No remote configured | Shell, 2026-11-10 |

The Android items recorded on 2026-11-10 (missing JDK, missing emulator, `ANDROID_SDK_ROOT` pointing to a missing folder) no longer matter for this project. They are kept in the [2026-11-10 evidence record](delivery/evidence/2026-11-10-documentation-setup.md).

Prototype refresh tooling observed on 2026-11-11: Node 24.16.0 and Python 3.14.6 are on PATH in the agent execution environment. The new dependency-free browser checker requires Node 22+ and a supplied Chromium executable ([tools guide](../tools/README.md#check-the-prototype)); it does not change the application's toolchain. Earlier inventory above is historical; installed SDKs and other browser versions were not re-inventoried in this task. Each run records the browser's own returned version.

## SDK selection

The root `global.json` ([global.json reference](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json)), created 2026-11-13:
- `sdk.version`: `11.0.100`, the exact full SDK version.
- `sdk.rollForward`: `disable`. Each SDK update is a deliberate, recorded change: install the new SDK, update this file, and rerun the tests.
- `sdk.allowPrerelease`: `false`.

.NET 11 is an STS release supported until 2028-11-14 and gets monthly patches. Plan to take them, because a web application is exposed to the internet.

## First checkout

Host preparation, done in WSL on 2026-11-12 and 2026-11-13:

1. Install the .NET 11 SDK. In WSL: Microsoft's `dotnet-install.sh --version 11.0.100 --install-dir ~/.dotnet`. On Windows: `winget install Microsoft.DotNet.SDK.11`, or the [download page](https://dotnet.microsoft.com/en-us/download/dotnet/11.0). The exact version must match `global.json`.
2. Run `dotnet --list-sdks` and record the version in the table above.
3. For the HTTPS launch profile only: trust the development certificate with `dotnet dev-certs https --trust`. Not done yet in WSL; the commands below use plain HTTP on `localhost`.
4. Create a Supabase project for development, with synthetic data only. Store its connection string with `dotnet user-secrets`, and record its region and PostgreSQL version here. Done 2026-11-13 in WSL (see the Database row above and the development database row below). To store or replace the value without it appearing on screen or in shell history, run these one line at a time and check that the printed length is not 0 before saving:

   ```bash
   read -rsp 'Connection string: ' CS; echo; echo "length: ${#CS}"
   dotnet user-secrets set --id taxbuddy-web "ConnectionStrings:TaxBuddy" "$CS"; unset CS
   ```

Then, from the repository root (bash in WSL; all executed on 2026-11-13):

```bash
dotnet tool restore                                   # dotnet-ef 10.0.15
(cd src/TaxBuddy.Web && npm ci)                       # Tailwind 4.3.3; the build runs it
dotnet build TaxBuddy.slnx -c Release                 # warnings are errors
dotnet test TaxBuddy.slnx -c Release                  # data tests use throwaway schemas in the development database
dotnet ef database update --project src/TaxBuddy.Web  # creates or updates the tables in the development database
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/TaxBuddy.Web --no-launch-profile -- seed-development
dotnet run --project src/TaxBuddy.Web --launch-profile http   # http://localhost:5234
```

- **Test database:** the data and integration tests read the same `ConnectionStrings:TaxBuddy` secret, or a `ConnectionStrings__TaxBuddy` environment variable. Without it they are reported as skipped, not passed. Each test class creates its own `test_…` schema and drops it afterwards; schemas left by a crashed run are dropped after six hours.
- **Seed:** `seed-development` replaces all profiles in the development database with the synthetic seed (`src/TaxBuddy.Web/Data/Seed/development-profiles.json`). It refuses to run outside the Development environment.
- **New migrations:** `dotnet ef migrations add <Name> --project src/TaxBuddy.Web --output-dir Data/Migrations`. Where the Tailwind step can't run (no Linux Node), build Release with `-p:SkipTailwind=true` first and add `--configuration Release --no-build`. `AddAccounts` (F04) was applied to the development database on 2026-11-16. Review the SQL with `dotnet ef migrations script --project src/TaxBuddy.Web` before applying it. Don't use `--no-build` straight after adding a migration: it scripts the previous build.

## Configuration and dependencies

- Check in `.editorconfig` with nullable reference types enabled and warnings reviewed. Use `Directory.Build.props` only for settings shared by all projects.
- Use explicit package versions. Consider `Directory.Packages.props` when shared versions become repetitive.
- Lock files: decided at scaffold, 2026-11-13. The npm lock file is committed. NuGet lock files are not used yet; adopt them with CI ([NuGet dependency locking](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)).
- EF Core packages: the web project references `Microsoft.EntityFrameworkCore.Relational` 10.0.15 directly, so the test projects resolve the same EF Core version as the design-time tools. Without it the tests resolved 10.0.4 and failed to compile.
- Secrets come from `dotnet user-secrets` locally and the host's secret store in production; never from tracked files.
- Build output and local data files stay ignored (see `.gitignore`).

## Local development

Filled in when `TaxBuddy.Web` was created on 2026-11-13. Google sign-in added with F04 on 2026-11-16; email waits for R08.

| Item | Selected value |
|---|---|
| Web project and local addresses | `src/TaxBuddy.Web/TaxBuddy.Web.csproj`. Launch profile `http`: `http://localhost:5234`. Profile `https`: `https://localhost:7169` and `http://localhost:5234` (needs a trusted development certificate). Windows browsers reach the app in WSL at the same `localhost` address |
| Styles | `Styles/app.css` is the Tailwind input. The build writes `wwwroot/app.css`, which is ignored by git, through the `BuildTailwindCss` target. It needs `npm ci` once in `src/TaxBuddy.Web`; without it the build prints a notice and the pages have no styles. `-p:SkipTailwind=true` skips it |
| Google sign-in (development registration) | A Google Cloud OAuth client of type Web application, set up by the owner ([F04 plan](../specs/google-sign-in/plan.md#google-cloud-setup)). Redirect addresses `http://localhost:5234/signin-google` and `https://localhost:7169/signin-google`. Secret keys `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret` in user-secrets `taxbuddy-web`. Without both, Google isn't registered and `/signin` says so. Created by the owner on 2026-11-16 (Testing mode). After saving a redirect URI, Google may answer `Error 400: redirect_uri_mismatch` for a while, even when the request matches. Check Google's "error details" against the client page, then wait and start again from `/signin`. Packages: `Microsoft.AspNetCore.Authentication.Google` 11.0.0 and `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.15; the second follows EF Core 10 until Npgsql ships 11 |
| Profile pictures (F06) | A private Supabase Storage bucket `profile-pictures`. User-secrets: `Supabase:Url` (`https://<project-id>.supabase.co`) and `Supabase:SecretKey` (a secret key, or the legacy `service_role` key; server-only), optionally `Supabase:PictureBucket`. Without the URL and key, upload is switched off and avatars use initials. Configured in WSL on 2026-11-16: the URL and a 41-character `sb_secret_` key; the bucket exists, private; round trip passed (F06 evidence). Setup steps: [F06 plan](../specs/profile-pictures/plan.md#approach). Packages: `SkiaSharp` and `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.155.1 (ADR 011) |
| Microsoft sign-in (development registration) | Redirect address: the handler's callback path, confirmed at implementation. Secret keys `Authentication:Microsoft:ClientId` and `Authentication:Microsoft:ClientSecret` in user-secrets |
| Development database | A Supabase project used only for development and tests, with synthetic data and a reset-and-seed command.<br>**Where the connection string is stored:** user-secrets ID `taxbuddy-web` (set it as the web project's `UserSecretsId` at scaffold), key `ConnectionStrings:TaxBuddy`. Outside the repository, readable only by the user.<br>**Format:** Npgsql keyword format, not a `postgresql://` URI: `Host=aws-1-ap-southeast-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project-id>;Password=<db-password>;SSL Mode=Require`.<br>**Why the session pooler:** this WSL host has no IPv6, and Supabase's direct connection is IPv6-only. Use exactly the host shown under Connect → Session pooler (`aws-1-`, not `aws-0-`, for this project).<br>The web project's `UserSecretsId` is `taxbuddy-web`. Data tests run against this database in their own throwaway schemas (since 2026-11-13); a local PostgreSQL can replace it later |
| Email in development | A development sender that writes emails to a local ignored folder or the log. No real email is sent |
| Testing from a phone | Open: reach the PC over the local network with a firewall rule, or use a development tunnel. Sign-in redirect addresses must match the address used |

- **Development secrets:** `dotnet user-secrets` is unencrypted and for development only ([REFERENCES](REFERENCES.md#implementation-and-hosting)).
- **Synthetic data only:** local and test databases never hold real people's details.

To check which secrets are stored without showing their values, run `python3 -c 'import json,os; print(list(json.load(open(os.path.expanduser("~/.microsoft/usersecrets/taxbuddy-web/secrets.json"), encoding="utf-8-sig"))))'`. `dotnet user-secrets list` prints the values, including the database password.

## Troubleshooting and maintenance

| Situation | Recovery |
|---|---|
| Documentation path/anchor error | Run `uv run --no-project python tools/check_docs.py` (or `python tools/check_docs.py` where Python is on PATH) from the repository root; use its file/line to correct the link or restore the target. Rerun after renaming a feature |
| `python` opens the Microsoft Store or is not found | Use the uv-managed interpreter: `uv run --no-project python ...`. Any Python 3.10+ works |
| `dotnet` reports that the requested SDK was not found | The SDK in `global.json` is not installed. Install that exact version, or update `global.json` deliberately and record it here |
| Browser warns about the local HTTPS certificate | Run `dotnet dev-certs https --trust`, then restart the browser |
| Sign-in fails locally with a redirect error | The redirect address registered at the provider must match the local address and port exactly |
| The database connection fails | A "host not found" error means the host is wrong: copy it exactly from Connect → Session pooler, with no placeholder characters. An `XX000` refusal usually means the wrong pooler cluster (`aws-0` instead of `aws-1`) or a username without `.<project-id>`. A `postgresql://` value is rejected; use the keyword format. A free project pauses after a week without activity, so resume it in the dashboard |
| `dotnet` stops with "Couldn't find a valid ICU package" (WSL) | Install the ICU runtime: `sudo apt update && sudo apt install libicu78` on Ubuntu 26.04 (`apt-cache search --names-only '^libicu[0-9]+$'` shows the name on other releases). Run `apt update` first, because an outdated package list gives 404 errors. `libicu-dev` is not needed. For a quick check without it, `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` runs the build and tests; record that in the evidence, because culture behaviour differs |
| Picture upload says "We couldn't save your picture" | Supabase Storage refused or couldn't be reached. Check `Supabase:Url` and `Supabase:SecretKey`, and that the `profile-pictures` bucket exists. The app log has "Picture upload failed" with the error type only |
| Windows: "An Application Control policy has blocked this file" when tests or the app start | Seen 2026-11-16 for newly built, unsigned `TaxBuddy.Web.exe` and test DLLs, most likely from Windows Smart App Control. The build itself succeeds. Run tests and the app in WSL, or have the PC's owner change the policy; don't work around it. Building on Windows (`dotnet.exe`) still runs Tailwind, which WSL can't run without a Linux Node |
| `dotnet test` in WSL exits 0 but prints nothing and runs no tests | The `obj/` restore files were last written from Windows (for example by VS Code's C# extension), so they hold `C:\...` paths. Check with `grep projectPath tests/TaxBuddy.Tests/obj/project.assets.json`. Build again in WSL without `--no-build`, then test. Seen 2026-11-16. Windows and WSL share these folders, so whichever built last wins |
| WSL build fails: `node_modules/.bin/tailwindcss ... exited with code 127` | WSL has no Linux Node, or `node_modules` was installed by Windows npm and holds only Windows binaries (`@tailwindcss/oxide-win32-x64-msvc`). Install Node 22+ (`sudo apt install nodejs npm`, Node 22.22.1 on Ubuntu 26.04), then in `src/TaxBuddy.Web` run `rm -rf node_modules && npm ci` from WSL. Meanwhile, `-p:SkipTailwind=true` builds with the existing `wwwroot/app.css`. Seen 2026-11-16 |
| The pages have no styling | `wwwroot/app.css` was not generated: run `npm ci` in `src/TaxBuddy.Web`, then build again |
| Tests show as skipped | No test database is configured: store `ConnectionStrings:TaxBuddy` (step 4 above) or set `ConnectionStrings__TaxBuddy`. Skipped is not passed |
| The app stops at start-up with "ConnectionStrings:TaxBuddy is not set" | It was not started as Development (user-secrets load only there), or the secret is missing. Use `--launch-profile http` or set `ASPNETCORE_ENVIRONMENT=Development` |
| A test/build could not execute | Record the concrete failure and next step in STATUS. A successful compile, restored package or another machine's historical pass does not replace that execution |

Project-specific failures and artifact locations are recorded here as they occur; the servicing owner is lumabanls. The [tools guide](../tools/README.md) owns checker commands, exit meanings and parser limits.

After a toolchain change, record exact versions and relevant test/build/browser results in [STATUS](delivery/STATUS.md). Do not inherit earlier results automatically.
