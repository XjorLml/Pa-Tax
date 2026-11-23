# Microsoft sign-in plan

Scenarios: [spec](spec.md). Tracking: [tasks](tasks.md).

**Archived 2026-11-23** by the owner: the switch `Authentication:Microsoft:Enabled` (default off) keeps it hidden. The plan below is how it was built.

## Modules

| Path | Change | Scenarios |
|---|---|---|
| `src/TaxBuddy.Web/Account/AccountSetup.cs` | `AddMicrosoftAccount` when both keys are set, with the same cancel and failure events as Google; keys read when first used | MS01–MS06 |
| `src/TaxBuddy.Web/Account/AccountService.cs` | `IsMicrosoftAvailableAsync`, `ProviderNamesAsync` | MS01, MS06 |
| `Components/Pages/SignInPage.razor` | The Microsoft form under Google, as in the prototype; provider names in the failure line and footnote | MS01, MS06 |
| Help, How it works, profile hint, delete page | The offered providers' names | MS03, MS06 |
| `tests/TaxBuddy.Web.Tests/MicrosoftSignInTests.cs`, test support | The factory's `withMicrosoft`, the test sign-in's provider | MS01–MS06 |

## Approach

Follow F04 exactly: the scheme is registered only when configured, so an unconfigured site shows no button and a forged post for it is refused (400). The test sign-in writes the external cookie with provider `Microsoft`, so registration and returning sign-in run through the real callback.

## Microsoft Entra setup

Steps are in [SETUP](../../docs/SETUP.md#local-development): an app registration for personal and work or school accounts, the Web redirect URI `http://localhost:5234/signin-microsoft`, and a client secret's value in user-secrets.

## Verification

`dotnet build` with no warnings, the new web tests, the full suite, and the docs checker. A real round trip waits for the app registration (MS07).
