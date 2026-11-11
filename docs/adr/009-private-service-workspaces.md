# ADR 009: Private service workspaces alongside text-only chat

Status: Accepted scope expansion and private-save/request/accept model; Proposed remaining lifecycle, editing/deletion policy and storage implementation. Date: 2026-11-11. Evidence: the user requested client lists, per-client service checklists, added-accountant lists and files for service progress, then said “now do that update thedocs and the prototype.” The owner subsequently selected the private-save/request option in a question explicitly describing accountant acceptance before a shared relationship. No release or provider approval is inferred.

## Context

[PRD](../PRD.md) R19–R23 add an ongoing-work view to the directory/portfolio/chat product. BR04 still limits chat to plain text; SDD previously used Supabase only as a database. The new documents are private service artifacts, not public portfolio uploads or chat attachments. [F03](../../specs/client-services/spec.md) defines the prototype task.

## Decision

Add private per-client/per-accountant service workspaces inside the existing Blazor application. Store relationship, engagement, task, update and file metadata in the application's database. Authorize list, read, change, upload, download and removal operations against the authenticated participant and parent engagement. The [SDD](../SDD.md#private-service-workspaces) owns the contract.

Keep bytes behind a private storage abstraction; no provider is selected. Choosing Supabase Storage or any other new storage service requires the D28 decision and SDD update. Existing Supabase database-only use is unchanged until that decision. Application-owned endpoints for private file forms/downloads are part of the same site and do not introduce a separate API/client.

Confirmed model: a private saved-accountant list, followed by a client service request and accountant acceptance to start shared work. Other prototype proposals: decline handling, accountant-added work for existing clients, accountant edits tasks/shared notes, client reads progress; both participants upload, uploader can remove an own file with confirmation. These illustrate D24–D27 and are not settled production policy. Real file intake is deferred until permissions, limits, scanning, retention, deletion and provider choices are resolved.

## Alternatives and rationale

| Option | Tradeoff |
|---|---|
| Separate service workspace (chosen scope) | Contextual tasks/documents without changing message semantics; adds a new private authorization boundary |
| Put files in chat | Simple location but loses task association and contradicts text-only chat; not authorized |
| Treat saving as a shared relationship | Fewer steps but can expose a client's interest without consent; not the default prototype proposal |
| Choose a storage vendor immediately | Makes implementation concrete but invents approval, capacity, operations and policy choices; deferred D28 |

## Consequences and verification

PRD R11 privacy and R09 account deletion now need service/file handling. D07/D14 and D24–D28 block production lifecycle/storage acceptance. Progress does not change review eligibility (BR09) and does not add payments, bookings or email events. Verify the prototype through CS01–CS10; later verify participant isolation and invalid parent/file IDs in real server/data/storage integration tests. A DOM guard is not a security boundary.

## Provenance and supersession

Recorded during the authorized task, 2026-11-11. Supersedes no ADR. Extends ADR 008's product with service tracking and documents; BR04's chat-only attachment exclusion remains. Earlier observations and approvals retain their provenance.
