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
| Hosting | Docker on one Oracle Cloud Infrastructure Compute VM in Singapore (`ap-singapore-1`), with Caddy for HTTPS ([ADR 012](adr/012-oci-compute-docker-host.md), 2026-11-24, replacing Render). Files: [Dockerfile](../Dockerfile) and [deploy/oci](../deploy/oci/compose.yaml). The test site's address is the VM's IP as an sslip.io name until a domain is chosen (D01). Owner lumabanls |
| Database | Supabase, with a separate project for the test site in the region closest to its users. The plan and backups are decided with the host |
| Sign-in registrations | One Google app registration per environment, with that environment's redirect address `https://<site>/signin-google` ([ADR 006](adr/006-web-sign-in.md)). Microsoft sign-in is archived (2026-11-23): leave `Authentication:Microsoft:Enabled` unset and register nothing at Microsoft |
| Email service and sender | Brevo; built in F14 (2026-11-23) with a development sender. Before real emails: create a Brevo account, verify a sender domain, and put `Email:Brevo:ApiKey`, `Email:SenderAddress` and `Email:SiteAddress` (the site's public HTTPS address) in the host's secret store. Without them a deployed site skips emails and logs event 1504 (G11) |
| Release decision owner | lumabanls. No release decision recorded yet |

There is no app package, store listing or code-signing certificate: nothing is installed on users' devices.

## Publish recipe

The image is the published build ([ADR 012](adr/012-oci-compute-docker-host.md)). From the repository root, on a machine with Docker (the VM itself, so the image matches its Arm processor):

```bash
docker build --build-arg VERSION=$(git rev-parse --short HEAD) -t taxbuddy-web .
```

The [Dockerfile](../Dockerfile) has four stages:
1. Node builds the Tailwind styles.
2. The .NET SDK 11.0.100 (as `global.json` pins) publishes the app with `SkipTailwind=true` and makes the EF migration bundle with the pinned `dotnet-ef` 10.0.15.
3. `migrations` holds that bundle.
4. `runtime` (the default) runs the framework-dependent app on `mcr.microsoft.com/dotnet/aspnet:11.0.0` as a non-root user, on port 8080.

On 2026-11-24 the image itself wasn't built, because the development PC has no Docker. The same publish (`SkipTailwind=true dotnet publish … -c Release`) and migration bundle were run locally, and the published files served `/health`, `/signin` and the styles with the container's settings ([evidence](delivery/evidence/2026-11-24-oci-docker.md)).

## Deployment

Host requirements, and how the OCI set-up ([ADR 012](adr/012-oci-compute-docker-host.md)) meets each:

| Need | Why |
|---|---|
| HTTPS with a valid certificate; HSTS | Sign-in and session cookies require it. Caddy gets and renews the certificate for `SITE_ADDRESS`; the app adds HSTS |
| WebSockets enabled | Interactive pages work best over WebSockets. Caddy passes them through |
| Forwarded-headers set-up, if behind a proxy | So the site sees the real scheme and address; sign-in redirects depend on it. The image sets `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`; only Caddy can reach the app |
| Data-protection keys stored outside the app's files | Otherwise sessions and form tokens can break after a redeploy. The `dpkeys` volume at `/keys` |
| Production secrets in the host's secret store | The Google client secret, the database connection, the Supabase key, the email service key: in `deploy/oci/.env.web` on the VM, readable only by its owner (`chmod 600`), never committed. Rotation owner: lumabanls |
| One server instance, or session affinity plus a shared chat notifier | Live chat updates are in-process ([SDD](SDD.md#live-chat-and-email-notifications)). One VM, one `web` container |
| Outbound email allowed | R08 |
| A health address and logs without personal data | To verify a deployment and diagnose failures. `GET /health` names the deployed commit; `docker compose logs web` reads the logs. Log retention follows PRD D14 |

### The OCI VM (first time)

1. In the OCI console, region Singapore: create a Compute instance (Ubuntu, an Ampere A1 shape if Always Free is wanted), with your SSH key and a public IP.
2. Open ports 80 and 443: in the subnet's security list (or a network security group) add ingress rules for TCP 80 and 443 from `0.0.0.0/0`, and on the VM allow them in its firewall too (Oracle's Ubuntu images ship with iptables rules that block them).
3. On the VM, install Docker Engine with the Compose plugin, and add your user to the `docker` group.
4. Clone the repository on the VM. In `deploy/oci`, copy `env.example` to `.env` and `env.web.example` to `.env.web`, fill them in, and run `chmod 600 .env.web`. `SITE_ADDRESS` is the public IP as an sslip.io name, for example `152-67-1-2.sslip.io`.
5. In Google Cloud, add `https://<SITE_ADDRESS>/signin-google` to the OAuth client's redirect URIs, and set `Email__SiteAddress` to `https://<SITE_ADDRESS>`.
6. The test site's Supabase project needs the `profile-pictures` and `service-documents` buckets ([SETUP](SETUP.md#local-development)).

### Each release, on the VM

```bash
cd deploy/oci
git pull
docker compose --profile migrate run --rm --build migrate           # database changes first (step 3 below)
VERSION=$(git rev-parse --short HEAD) docker compose up -d --build  # then the new build
curl -s https://<SITE_ADDRESS>/health                                # names the commit just deployed
```

Rolling back: check out the previous commit and run the second command again; a schema change follows that release's rollback plan.

Sequence:

1. Identify the source revision and any uncommitted changes; resolve required gates in TEST-PLAN and record remaining limitations.
2. Publish with SETUP's SDK. Keep secrets out of command history, logs, source files and build output.
3. Apply database changes with the migration bundle (`docker compose --profile migrate run --rm migrate`) or a reviewed SQL script, before the new build needs them. Migrating at app startup needs a recorded decision.
4. Deploy. Open chat pages lose their connection during a restart, so users see a reconnecting notice (TP15).
5. Verify the deployed site: `/health` names the deployed commit, one public page, sign-in with Google, one message, and one notification email (TP07, TP12).
6. Record the release decision, the deployed revision, the environment and the smoke results as evidence.

Schema changes follow a two-step rule. First add the new shape while the old one keeps working. Remove the old one in a later release. This keeps a rollback to the previous build from breaking on newly written data.

Before any public launch, PRD D10 (privacy review, including a lawyer's review of the [legal drafts](legal/drafting-notes.md)) must be completed. D09 and D19 were decided on 2026-11-21: there are no licence checks, and search indexing is switched on for the public site by setting `Seo:AllowIndexing` to true (the test site leaves it off).

## Recovery and support

Internal testing: testers report problems directly to lumabanls. Reports carry the support code (correlation ID) shown by the site, never screenshots of other users' chats or contact details.

- **Bad deployment:** redeploy the previous build. If the schema changed, follow that release's recorded rollback plan; do not assume the old build can read the new schema.
- **Data loss:** all account and chat data is in the server database, so recovery equals database backup and restore (PRD D11).
- **Lost data-protection keys:** everyone is signed out and must sign in again. No data is lost.
- **Leaked secret:** rotate it at the provider and in the host's secret store, then redeploy.

### Reading user reports (R15, D22)

Since F10 (2026-11-18), "Report" on a profile, a review or a chat stores a row in the `reports` table. There is no admin page and no email yet; the email follows with R08 once Brevo is set up (G11).

To read them:
1. Open the Supabase project, then Table Editor → `reports`. Sort by `created_at`, newest first.
2. Each row has the following columns:
   - `kind`: `Profile`, `Review` or `Conversation`.
   - `target_id`: the item.
   - `profile_id`: the accountant profile it belongs to, if any.
   - `reason` and `details`.
   - `reporter_account_id` and `created_at`.
3. Look up the item by its ID in `accountant_profiles`, `reviews` or `conversations`. For a conversation, its messages are in `messages` by `conversation_id`. Reading a private conversation is only for handling the report; keep it within the privacy terms (D10).

Reported content stays visible until you act (BR12). Review every report within 10 working days (D22, decided 2026-11-21); the report page tells reporters so.

### Acting on a report (D22)

Simple steps, asked for by the user on 2026-11-21. There's no admin page, so every action is a change in Supabase's Table Editor. Work in the test-site or production project, never with a copy on your own computer.

1. **Decide.** Does the item break the [Terms of Use](legal/terms-of-use.md) (sections 7–10) or the law? If not, take no action.
2. **Keep a record** in a private note outside this repository: the date, the report's `id`, what you decided and what you changed. Never put names or message text in the repository.
3. **Remove a review:** in `reviews`, delete the row with that `id`. The rating updates by itself.
4. **Remove only a reply:** in `reviews`, set `reply_text` and `reply_at` to NULL on that row.
5. **Remove a message:** in `messages`, delete the row with that `id`. Do this only for a message that breaks the rules; the rest of the conversation stays.
6. **Take a profile out of the directory:** in `accountant_profiles`, set `consent_to_show_contact` to false. The profile disappears at once. The accountant can list it again by editing it, so close the account (step 7) if the problem repeats.
7. **Close an account:** in `users`, find the row by `id` (a report's `reporter_account_id`, a profile's `owner_account_id`, or a message's `sender_account_id`). Check that `lockout_enabled` is true, then:
   - set `lockout_end` to `9999-12-31 00:00:00+00`, so they can't sign in again;
   - set `security_stamp` to any new text, so open sessions end within about a minute.

   For an accountant, also do step 6.
8. **Tell the people involved** by email, using the address in `users.email`. Tell the owner of the content what you removed and why. You may thank the reporter, but never tell anyone who reported them.
9. **Threats, fraud or other crimes:** keep a screenshot, close the account, and report it to the PNP Anti-Cybercrime Group or the NBI Cybercrime Division.

To undo step 7, set `lockout_end` back to NULL.

### Deletion requests by email (R09, F12)

Since F12 (2026-11-23) people delete their own account under Account → Delete account, so point them there first. If they can't sign in any more (for example, their Google account is gone), do by hand what the site does:

1. Do step 6 (an accountant's profile) and step 7 (close the account) above.
2. In `reviews`, delete the rows whose `client_account_id` is theirs.
3. In `user_logins`, delete their rows.
4. In `users`, on their row, set `display_name` to `Deleted user`, `email`, `normalized_email` and `picture_object` to NULL, and `deleted_at` to the current time.
5. Note the request and the date in your private record (step 2 above).

Their chats stay with the other person, who can no longer reply because the account is closed.

### A deleted account's picture left in storage

If the log shows event 1403, "A deleted account's picture stayed in storage", the account is deleted but its picture file isn't. In Supabase → Storage → `profile-pictures`, delete the folder `users/<account id>/`. The account ID isn't in the log; find it in `users` by a `deleted_at` near the log's time.

### Service documents left in storage

Event 1404, "N of a deleted account's documents stayed in storage", or event 1602, "A removed service document stayed in storage", means files no longer listed on the site are still in the `service-documents` bucket. They can't be downloaded through the site. To remove them:

1. In Supabase → SQL editor, list the stored names: `select object_name from service_files;`.
2. In Supabase → Storage → `service-documents`, open the `services/` folders and delete any object whose path isn't in that list. A folder is a service; an object is one document.
3. Don't delete an object that is in the list: that document is still on the site.

Preserve released migration scripts and deployment evidence.

## Private service documents gate

R19–R23 ([ADR 009](adr/009-private-service-workspaces.md)) are built in the application since 2026-11-23 (F03 stages 1 and 2), on the D24–D28 choices recorded in the PRD; some details are working values (50 documents per service) or still open (quotas, retention of service work, relationship ending). Before real client documents are accepted on a deployed site:

- create the private `service-documents` bucket in that environment's Supabase project (Public off), with the same server-only key as pictures ([SETUP](SETUP.md#local-development));
- apply migration `AddServiceDocuments`;
- decide on the privacy impact assessment the legal notes recommend before documents are accepted (legal issue 2.10, [drafting notes](legal/drafting-notes.md)), and have the owner approve legal text version `2026-11-23.2`, which describes documents;
- remember there is no virus scanning (D28): downloads are always attachments, never shown in the browser.

Progress adds no new email events. The prototype's generated samples are not production upload evidence.
