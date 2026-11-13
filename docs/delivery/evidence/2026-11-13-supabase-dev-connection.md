# Supabase development database connection evidence

## Identity and attribution

- Date: 2026-11-13, about 02:10 UTC (10:10 Asia/Manila).
- Observer: the user created the Supabase project and stored the connection string. The agent ran the connection check and corrected one stored host name, as described below. No acceptance is claimed.
- Gate: [STATUS](../STATUS.md) G6, the development database before F01 ([plan prerequisite 4](../../../specs/browse-accountants/plan.md)). Contracts: [SDD data and persistence](../../SDD.md#data-and-persistence-contracts), [ADR 007](../../adr/007-supabase-brevo-container-host.md).
- Source: `5ceee374da2f2622930fa4a1e5b69601852d22f3` plus uncommitted documentation changes. No application source exists. The check was a throwaway console program in the agent's session scratch folder, not retained in the repository. It used .NET SDK 11.0.100 and Npgsql 10.0.3.
- What the check does: reads `ConnectionStrings:TaxBuddy` from user-secrets ID `taxbuddy-web`, connects, and prints only non-secret facts. It never prints the password, the username or the project ID. It shows the host only when it is a shared Supabase pooler name, which contains no project ID.
- Environment: WSL2 Ubuntu 24.04.1 on Windows 10.0.26300.9974, with no IPv6 address or route in WSL.
- Fixtures: a new, empty project. No data was written.

## Execution

| Step | Expected | Actual observation | Outcome |
|---|---|---|---|
| Project region (user's dashboard screenshot) | Singapore, per ADR 007 | Southeast Asia (Singapore), `ap-southeast-1`, Nano compute. The user's first project had gone to Sydney (`ap-southeast-2`) because the "Asia-Pacific" general region picks any location with capacity, so the user created this one with the specific region | Pass |
| First check | Value stored | The key existed but its value was empty: nothing reached the hidden `read` prompt | Fail; the user stored it again |
| Second check | Connects | Session pooler, port 5432, `SSL Mode=Require`, password present. Connection failed: DNS host not found. The stored host was literally `aws-?-ap-southeast-1.pooler.supabase.com`, copied from a placeholder in the agent's own instructions | Fail; cause found |
| Host correction (agent) | Only a host that connects is saved | `aws-0-…` was refused with SQLSTATE `XX000`; `aws-1-…` connected. Only the `aws-?-` text was replaced, and only in the user's local secret file. The file stayed at mode 600 | Pass |
| Final check | Connects with encryption required | Connected through `aws-1-ap-southeast-1.pooler.supabase.com:5432`. PostgreSQL 17.11, role `postgres`, 0 tables in `public`. `pg_stat_ssl` reports no SSL for the pooler-to-database hop inside Supabase. The client-to-pooler hop is encrypted, because `SSL Mode=Require` makes Npgsql refuse an unencrypted connection | Pass, database boundary |

## Restoration and limits

The secret file stays at `~/.microsoft/usersecrets/taxbuddy-web/secrets.json`, outside the repository. The throwaway check program stays in the session scratch folder and holds no secret. No tables, roles or settings were created or changed in the database.

Not exercised: EF Core, migrations or row-level security behaviour, which come with the first data test. Whether the Data API is switched off was not confirmed (the user's screenshot did not show it). Whether the Sydney project still exists was not checked. Certificate validation (`VerifyFull`) was not set up; it belongs to production hardening. Acceptance decision: not applicable; this is a prerequisite check.
