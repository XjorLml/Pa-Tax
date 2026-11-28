# "Add task" and shared-note repeat guards (PRD D31) execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-25, about 19:00–20:00 Asia/Manila (+08).
- Observer: agent-observed (Claude Code), at the user's instruction "fix the two forms first".
- Requirements/scenarios/procedures: [PRD D31](../../PRD.md#open-decisions), BR18; [F03 CS18–CS20](../../../specs/client-services/spec.md#repeated-posts-2026-11-25); TP20 and TP28 in [TEST-PLAN](../TEST-PLAN.md#scenario-mapping).
- Source revision and working-tree changes: HEAD `06e9be6`, with the existing uncommitted work (F17 and earlier) and these changes on top, uncommitted. SHA-256 prefixes:
  - `src/TaxBuddy.Web/Domain/Work.cs` `d493713ba4b5`
  - `src/TaxBuddy.Web/Data/TaxBuddyDbContext.cs` `5ac0035a70b5`
  - `src/TaxBuddy.Web/Application/Work/WorkModels.cs` `cca2af5df23d`
  - `src/TaxBuddy.Web/Application/Work/WorkService.cs` `8e3ca2641426`
  - `src/TaxBuddy.Web/Data/WorkStore.cs` `bad46614831d`
  - `src/TaxBuddy.Web/Components/Pages/ServicePage.razor` `40716e20c187`
  - `src/TaxBuddy.Web/Data/Migrations/20261125113011_AddUpdateFormKeys.cs` `b9be88e47cbf`
  - `tests/TaxBuddy.Web.Tests/ClientServicesTests.cs` `593241b6607c`
- Artifact: Debug build of the working tree; no published image.
- Environment: WSL2, Ubuntu 24.04.1 LTS; .NET SDK 11.0.100; `dotnet-ef` 10.0.15 for the migration; Tailwind 4.3.3 in the build. The web tests use throwaway schemas on the Supabase development database, as the existing suite does.
- Preconditions/fixtures: synthetic accounts and services created by the tests.

## What changed

- `service_updates.form_key` (nullable `uuid`), unique with `engagement_id`, through migration `AddUpdateFormKeys`. Applied to the development database the same day at the user's request ("apply the migration to the dev database"; below).
- "Add task" and the shared note draw a one-time hidden key (`NewTask.Key`, `Note.Key`). A form drawn again after a failure keeps the key it was sent with.
- `WorkStore.ChangeAsync` takes a `FormPost` (key and line):
  - the same key with the same line by the same person returns Existing without changing anything;
  - a lost version check or a unique-index clash looks again and returns Existing when the other post was this form;
  - the same key with another line is stored without the key.
- `WorkService` refuses a post without a key as a failed save. `ServicePage` treats Existing as done.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet ef migrations add AddUpdateFormKeys --project src/TaxBuddy.Web --output-dir Data/Migrations` | One column and one unique index; nothing else in `Up` | "Build succeeded." `Up` adds `form_key` and `ix_service_updates_engagement_id_form_key` (unique); `Down` drops both | Pass |
| `dotnet test tests/TaxBuddy.Web.Tests --filter "FullyQualifiedName~ClientServicesTests"` | The 13 existing and 4 new tests pass | "Passed! - Failed: 0, Passed: 17, Skipped: 0, Total: 17", 3 min 47 s | Pass |
| `dotnet test` (both test projects, Debug) | All pass | TaxBuddy.Tests: "Passed! - Failed: 0, Passed: 223". TaxBuddy.Web.Tests: "Failed: 4, Passed: 177, Total: 181". All four are `Assert.DoesNotContain("Microsoft")` on a page (`/privacy`, `/terms`, the archived Microsoft sign-in, About and Help). A Debug build lists `./_framework/Microsoft.NET.Sdk.BlazorWeb.HotReload…` in each page's import map | Fail, Debug only |
| `dotnet test tests/TaxBuddy.Web.Tests -c Release` (SETUP runs the tests in Release) | All pass | "Passed! - Failed: 0, Passed: 181, Skipped: 0, Total: 181", 3 min 40 s | Pass |
| `dotnet ef migrations list`, then `migrations script 20261125074243_AddClientDetails 20261125113011_AddUpdateFormKeys`, then `database update`, each `--project src/TaxBuddy.Web --no-build` on the 19:40 Debug build, which holds the migration | Only `AddUpdateFormKeys` pending; the SQL adds the column and the index in one transaction | `AddUpdateFormKeys (Pending)` only. SQL: `ALTER TABLE service_updates ADD form_key uuid;` and `CREATE UNIQUE INDEX ix_service_updates_engagement_id_form_key ON service_updates (engagement_id, form_key);` with the history row, between `START TRANSACTION` and `COMMIT`. Update: "Done."; the list then shows nothing pending | Pass |

New tests in `ClientServicesTests`:
- `Sending_the_same_task_or_note_form_twice_stores_it_once` (CS18): two posts each, one after the other.
- `The_same_task_or_note_form_sent_twice_at_once_is_stored_once` (CS18): four posts at once through `Task.WhenAll`. Whether the posts actually overlapped on the server was not observed, so this test passes on either path.
- `A_different_note_sent_from_an_older_copy_of_the_page_is_shared_too` (CS19).
- `A_note_form_without_its_key_is_refused_with_the_text_kept` (CS20).

The new tests were not run against the code before the change.

## Follow-up: Accept, Decline, the checklist and uploads

Later the same day, at the user's instruction "fix the other three too" ([F03 CS21–CS23](../../../specs/client-services/spec.md#repeated-posts-2026-11-25)). These stored nothing twice before, but a repeat showed the wrong result. Source after the change, SHA-256 prefixes:
- `src/TaxBuddy.Web/Application/Work/ServiceDocuments.cs` `28670bc26353`
- `src/TaxBuddy.Web/Application/Work/WorkService.cs` `3af6b2d40e23`
- `src/TaxBuddy.Web/Application/Work/WorkModels.cs` `598314d71b19`
- `src/TaxBuddy.Web/Data/WorkStore.cs` `85047f39ae36`
- `src/TaxBuddy.Web/Domain/Work.cs` `a340353df9d6`
- `src/TaxBuddy.Web/Components/Pages/ServicePage.razor` `570acd246514`
- `tests/TaxBuddy.Web.Tests/ClientServicesTests.cs` `5e25ea0e626b`
- `tests/TaxBuddy.Web.Tests/ServiceDocumentsTests.cs` `82f0daeb21c2`

What changed:
- **Accept and Decline:** accepting active work, or declining a declined request, is "nothing to change". Blazor hands a post only to a form the page draws, so while handling a post for an answered request, the page draws the two forms empty and hidden for its accountant. An answer that no longer applies (Decline after Accept, from an older tab) goes to the service as it is now.
- **Checklist:** a save from an older version that asks for nothing that isn't saved already is Done; one that asks for something else is still Stale.
- **Races:** a change that loses the service's version check is tried once more against the saved service (`WorkService`).
- **Uploads:** the stored key is a SHA-256 of the form's key, the task and the file (`ServiceDocuments.UploadKey`). No migration: it goes in the existing `service_files.operation_id`.

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet test tests/TaxBuddy.Web.Tests -c Release --filter "…ClientServicesTests\|…ServiceDocumentsTests"`, first run | All pass | "Failed: 1, Passed: 28, Total: 29". `Accepting_or_declining_a_request_twice…` at the second Accept: no redirect | Fail |
| The same test with a temporary probe on that post, removed afterwards | Find out what the second post gets | `TEMP 400` with an empty body: the page didn't draw the Accept form any more, so Blazor refused the post before the handler ran. Gate G19 in STATUS and the agent's report had said "This workspace is not available", from reading the code; that was wrong, and G19 now says otherwise | Observed |
| `dotnet test tests/TaxBuddy.Web.Tests -c Release`, after drawing the hidden forms | All pass | `SignInTests` failed while its fixture opened a database connection to create its schema (`RelationalConnection.OpenAsync` in `TestDatabase.CreateSchemaAsync`); the summary line was cut off by the command's output filter | Blocked (database connection) |
| The same command again | All pass | "Failed: 1, Passed: 186, Total: 187". The failure is `TemporaryScreensForEvidence.Save_editor_pages`, which isn't part of this work: another session's temporary test file, compiled into this run and gone from the tree afterwards. Every `ClientServicesTests` and `ServiceDocumentsTests` test passed | Pass for this work |

New tests: `Accepting_or_declining_a_request_twice_changes_nothing_more_and_says_done`, `Accepting_a_request_twice_at_once_accepts_it_once`, `Saving_the_same_checklist_twice_says_saved_and_records_it_once`, `An_older_copy_of_the_upload_form_with_another_file_or_task_stores_a_new_document`. The checklist's and the upload's old answers were read from the code, not run. Whether the "at once" posts overlapped on the server was not observed.

## Restoration and limits

The test fixture drops its throwaway schema afterwards, and any left by a crashed run after six hours (`tests/Shared/TestDatabase.cs`); the database wasn't inspected after the run. Nothing else was changed. The development database keeps the new column and index. To undo: `dotnet ef database update 20261125074243_AddClientDetails --project src/TaxBuddy.Web`.

Not exercised: a browser (nothing was clicked) and the running site; TaxBuddy.Tests in Release. Acceptance decision: pending.
