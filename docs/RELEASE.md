# Release and deployment procedure

**Authority:** publishing, deployment, and recovery procedure. [PRD](PRD.md) chooses the audience; [TEST-PLAN](delivery/TEST-PLAN.md) defines acceptance gates; [STATUS](delivery/STATUS.md) records actual readiness. Nothing here authorizes deploying or going public.

State on 2026-11-11: no build, host or domain exists. The first audience is invited testers on a test site (PRD release scope).

## Identity and environments

| Item | Selected value / owner |
|---|---|
| Site name and domain | Tax Buddy, by the user's instruction of 2026-11-12 (PRD D01); domain and name availability checks remain open |
| Environments | Local development, and one test site for invited testers. A separate production site is decided before public launch (PRD D11) |
| Who can use the test site | Invited testers only; the mechanism is open (PRD D20) |
| Version identity | Proposed: each deployment records its git commit, shown on a health or about page, so a tester's report can be tied to a build |
| Hosting | Render, as a container in its Singapore region ([ADR 007](adr/007-supabase-brevo-container-host.md)). The plan is not chosen, and the free service sleeps when idle; owner lumabanls |
| Database | Supabase, with a separate project for the test site in the region closest to its users. The plan and backups are decided with the host |
| Sign-in registrations | One Google and one Microsoft app registration per environment, each with that environment's redirect addresses ([ADR 006](adr/006-web-sign-in.md)) |
| Email service and sender | Brevo. The sender address and its domain set-up are open (PRD D18) |
| Release decision owner | lumabanls. No release decision recorded yet |

There is no app package, store listing or code-signing certificate: nothing is installed on users' devices.

## Publish recipe

No recipe yet. After the scaffold step, record the exact publish command and its output here, starting from:

```powershell
dotnet publish src/TaxBuddy.Web/TaxBuddy.Web.csproj -c Release
```

This command has not been executed. If the chosen host takes a container image, record the Dockerfile and the image build as well. Record the configuration, the runtime the host needs (framework-dependent or self-contained), the output path, and how the output reaches the host.

## Deployment

Host requirements, each to be confirmed against the chosen host ([REFERENCES](REFERENCES.md#implementation-and-hosting)):

| Need | Why |
|---|---|
| HTTPS with a valid certificate; HSTS | Sign-in and session cookies require it |
| WebSockets enabled | Interactive pages work best over WebSockets |
| Forwarded-headers set-up, if behind a proxy | So the site sees the real scheme and address; sign-in redirects depend on it |
| Data-protection keys stored outside the app's files | Otherwise sessions and form tokens can break after a redeploy |
| Production secrets in the host's secret store | Google and Microsoft client secrets, the database connection, the email service key. Rotation owner: lumabanls |
| One server instance, or session affinity plus a shared chat notifier | Live chat updates are in-process ([SDD](SDD.md#live-chat-and-email-notifications)) |
| Outbound email allowed | R08 |
| A health address and logs without personal data | To verify a deployment and diagnose failures. Log retention follows PRD D14 |

Sequence:

1. Identify the source revision and any uncommitted changes; resolve required gates in TEST-PLAN and record remaining limitations.
2. Publish with SETUP's SDK. Keep secrets out of command history, logs, source files and build output.
3. Apply database changes with a migration bundle or a reviewed SQL script, before the new build needs them. Migrating at app startup needs a recorded decision.
4. Deploy. Open chat pages lose their connection during a restart, so users see a reconnecting notice (TP15).
5. Verify the deployed site: health address, one public page, sign-in with each provider, one message, and one notification email (TP07, TP12).
6. Record the release decision, the deployed revision, the environment and the smoke results as evidence.

Schema changes follow a two-step rule. First add the new shape while the old one keeps working. Remove the old one in a later release. This keeps a rollback to the previous build from breaking on newly written data.

Before any public launch, PRD D09 (trust and safety), D10 (privacy review) and D19 (search indexing) must be decided.

## Recovery and support

Internal testing: testers report problems directly to lumabanls. Reports carry the support code (correlation ID) shown by the site, never screenshots of other users' chats or contact details.

- **Bad deployment:** redeploy the previous build. If the schema changed, follow that release's recorded rollback plan; do not assume the old build can read the new schema.
- **Data loss:** all account and chat data is in the server database, so recovery equals database backup and restore (PRD D11).
- **Lost data-protection keys:** everyone is signed out and must sign in again. No data is lost.
- **Leaked secret:** rotate it at the provider and in the host's secret store, then redeploy.

Preserve released migration scripts and deployment evidence.

## Private service documents gate

R19–R23 ([ADR 009](adr/009-private-service-workspaces.md)) are currently documentation/prototype work. Before real client documents or durable service work are enabled, resolve PRD D24–D28 and implement the [SDD private-workspace contract](SDD.md#private-service-workspaces): relationship permissions, task/completion policy, types/quotas, file deletion/retention/account deletion, private storage provider and validation/scanning/recovery. Record storage credentials/configuration outside source control, migrations, cleanup, access-isolation tests, operations ownership and actual deployment checks. Progress adds no new email events. The prototype's generated samples are not production upload evidence.
