using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Work;

/// <summary>
/// Client services (F03; PRD R19–R22, BR14–BR16; SDD permission matrix). A client saves accountants privately and
/// requests a service; the accountant accepts or declines, adds services for clients they already serve, keeps the
/// checklist and posts shared notes. Every operation checks the signed-in participant against the service itself:
/// an ID in an address never grants access, and anything not the viewer's reads as unavailable.
/// </summary>
public sealed class WorkService(IWorkStore store, IClock clock)
{
    public static bool IsClient(ChatViewer? viewer) => viewer?.Role == AccountRole.Client;

    public static bool IsAccountant(ChatViewer? viewer) => viewer?.Role == AccountRole.Accountant;

    /// <exception cref="DataUnavailableException">The database could not be read.</exception>
    public Task<ClientWorkspaceView> ClientWorkspaceAsync(ChatViewer viewer, CancellationToken cancellationToken = default) =>
        store.ClientWorkspaceAsync(viewer.AccountId, cancellationToken);

    public Task<AccountantWorkspaceView> AccountantWorkspaceAsync(ChatViewer viewer, CancellationToken cancellationToken = default) =>
        store.AccountantWorkspaceAsync(viewer.AccountId, cancellationToken);

    public async Task<ClientServicesView?> ClientServicesAsync(ChatViewer? viewer, Guid clientId, CancellationToken cancellationToken = default) =>
        IsAccountant(viewer) ? await store.ClientServicesAsync(viewer!.AccountId, clientId, cancellationToken) : null;

    public async Task<EngagementView?> ServiceAsync(ChatViewer? viewer, Guid engagementId, CancellationToken cancellationToken = default) =>
        viewer is null ? null : await store.ServiceAsync(engagementId, viewer.AccountId, cancellationToken);

    /// <summary>The request page's accountant, for a signed-in client; null otherwise or when not listed.</summary>
    public async Task<WorkAccountant?> RequestTargetAsync(ChatViewer? viewer, AccountantId profileId, CancellationToken cancellationToken = default) =>
        IsClient(viewer) ? await store.FindListedAsync(profileId, cancellationToken) : null;

    public async Task<bool> IsSavedAsync(ChatViewer? viewer, AccountantId profileId, CancellationToken cancellationToken = default) =>
        IsClient(viewer) && await store.IsSavedAsync(viewer!.AccountId, profileId, cancellationToken);

    /// <summary>R20: a client adds a listed accountant to their private list, once. Nothing is shared.</summary>
    public async Task<WorkOutcome> SaveAsync(ChatViewer? viewer, AccountantId profileId, bool saved, CancellationToken cancellationToken = default)
    {
        if (!IsClient(viewer))
        {
            return WorkOutcome.NotAllowed;
        }

        try
        {
            if (!saved)
            {
                await store.RemoveSavedAsync(viewer!.AccountId, profileId, cancellationToken);
                return WorkOutcome.Done;
            }

            if (!await store.IsListedAsync(profileId, cancellationToken))
            {
                return WorkOutcome.NotAllowed;
            }

            await store.SaveAccountantAsync(viewer!.AccountId, profileId, clock.UtcNow, cancellationToken);
            return WorkOutcome.Done;
        }
        catch (DataUnavailableException)
        {
            return WorkOutcome.Failed;
        }
    }

    /// <summary>
    /// BR14: a client requests one of the accountant's offered services. It shares the client's name and the service with
    /// that accountant only, and adds them to the client's list. An open request or active service of the same name is
    /// returned instead of a second one (D24 proposal).
    /// </summary>
    public async Task<WorkResult> RequestAsync(ChatViewer? viewer, AccountantId profileId, string? serviceName, CancellationToken cancellationToken = default)
    {
        if (!IsClient(viewer))
        {
            return new WorkResult(WorkOutcome.NotAllowed);
        }

        try
        {
            if (await store.FindListedAsync(profileId, cancellationToken) is not { } accountant)
            {
                return new WorkResult(WorkOutcome.NotAllowed);
            }

            if (serviceName is null || !accountant.Services.Contains(serviceName))
            {
                return new WorkResult(WorkOutcome.Invalid, Problem: "Choose a service this accountant offers.");
            }

            var now = clock.UtcNow;
            await store.SaveAccountantAsync(viewer!.AccountId, profileId, now, cancellationToken);
            if (await store.FindOpenAsync(viewer.AccountId, accountant.AccountId, serviceName, cancellationToken) is { } existing)
            {
                return new WorkResult(WorkOutcome.Existing, existing);
            }

            var engagement = ServiceEngagement.Request(viewer.AccountId, accountant.AccountId, serviceName, now);
            return await store.AddAsync(engagement, viewer.AccountId, $"Requested {serviceName}. Awaiting accountant acceptance.", cancellationToken);
        }
        catch (DataUnavailableException)
        {
            return new WorkResult(WorkOutcome.Failed);
        }
    }

    /// <summary>D24 proposal: an accountant adds an offered service for a client they already have active work with.</summary>
    public async Task<WorkResult> AddServiceAsync(ChatViewer? viewer, Guid clientId, string? serviceName, CancellationToken cancellationToken = default)
    {
        if (!IsAccountant(viewer))
        {
            return new WorkResult(WorkOutcome.NotAllowed);
        }

        try
        {
            if (!await store.HasActiveAsync(clientId, viewer!.AccountId, cancellationToken))
            {
                return new WorkResult(WorkOutcome.NotAllowed);
            }

            var profile = await store.FindProfileOfAsync(viewer.AccountId, cancellationToken);
            if (serviceName is null || profile is null || !profile.Services.Contains(serviceName))
            {
                return new WorkResult(WorkOutcome.Invalid, Problem: "Choose one of your offered services.");
            }

            if (await store.FindOpenAsync(clientId, viewer.AccountId, serviceName, cancellationToken) is { } existing)
            {
                return new WorkResult(WorkOutcome.Existing, existing);
            }

            var engagement = ServiceEngagement.AddedByAccountant(clientId, viewer.AccountId, serviceName, clock.UtcNow);
            return await store.AddAsync(engagement, viewer.AccountId, $"Added {serviceName} to this client relationship.", cancellationToken);
        }
        catch (DataUnavailableException)
        {
            return new WorkResult(WorkOutcome.Failed);
        }
    }

    public Task<WorkOutcome> AcceptAsync(ChatViewer? viewer, Guid engagementId, CancellationToken cancellationToken = default) =>
        AsAccountantAsync(viewer, engagementId, null, e =>
        {
            if (e.Phase != ServicePhase.Requested)
            {
                return null;
            }

            e.Accept(clock.UtcNow);
            return [("Accepted the service request and created a starting checklist.", false)];
        }, cancellationToken);

    public Task<WorkOutcome> DeclineAsync(ChatViewer? viewer, Guid engagementId, CancellationToken cancellationToken = default) =>
        AsAccountantAsync(viewer, engagementId, null, e =>
        {
            if (e.Phase != ServicePhase.Requested)
            {
                return null;
            }

            e.Decline(clock.UtcNow);
            return [("Declined the service request.", false)];
        }, cancellationToken);

    /// <summary>
    /// BR16: the accountant saves task statuses from the version they saw. A stale version is refused so the draft can be
    /// reviewed against the current values (SDD optimistic concurrency).
    /// </summary>
    public Task<WorkOutcome> SaveStatusesAsync(ChatViewer? viewer, Guid engagementId, int version, IReadOnlyDictionary<Guid, ServiceTaskStatus> statuses,
        CancellationToken cancellationToken = default) =>
        AsAccountantAsync(viewer, engagementId, version, e =>
        {
            if (e.Phase != ServicePhase.Active)
            {
                return null;
            }

            var now = clock.UtcNow;
            var lines = new List<string>();
            foreach (var (taskId, status) in statuses)
            {
                if (e.Tasks.FirstOrDefault(t => t.Id == taskId) is { } task && e.SetStatus(taskId, status, now))
                {
                    lines.Add($"{task.Title}: {WorkRules.Label(status)}");
                }
            }

            return lines.Count == 0 ? [] : [(string.Join("; ", lines), false)];
        }, cancellationToken);

    public async Task<WorkResult> AddTaskAsync(ChatViewer? viewer, Guid engagementId, string? title, CancellationToken cancellationToken = default)
    {
        var trimmed = title?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > WorkRules.TaskTitle)
        {
            return new WorkResult(WorkOutcome.Invalid, Problem: $"Enter a task name of 1 to {WorkRules.TaskTitle} characters.");
        }

        var outcome = await AsAccountantAsync(viewer, engagementId, null, e =>
        {
            if (e.Phase != ServicePhase.Active || e.Tasks.Count >= WorkRules.TasksPerService)
            {
                return null;
            }

            e.AddTask(trimmed, clock.UtcNow);
            return [($"Added checklist task: {trimmed}", false)];
        }, cancellationToken);
        return new WorkResult(outcome, engagementId);
    }

    /// <summary>R22: a shared progress note by the accountant (BR16), 1 to 1,000 characters.</summary>
    public async Task<WorkResult> PostNoteAsync(ChatViewer? viewer, Guid engagementId, string? text, CancellationToken cancellationToken = default)
    {
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > WorkRules.Note)
        {
            return new WorkResult(WorkOutcome.Invalid, Problem: $"Write a shared update of 1 to {WorkRules.Note:N0} characters.");
        }

        var outcome = await AsAccountantAsync(viewer, engagementId, null,
            e => e.Phase == ServicePhase.Active ? [(trimmed, true)] : null, cancellationToken);
        return new WorkResult(outcome, engagementId);
    }

    /// <summary>Runs a change as the service's accountant; anyone else, or a change that doesn't apply, is NotAllowed.</summary>
    private async Task<WorkOutcome> AsAccountantAsync(ChatViewer? viewer, Guid engagementId, int? version, EngagementChange change, CancellationToken cancellationToken)
    {
        if (!IsAccountant(viewer))
        {
            return WorkOutcome.NotAllowed;
        }

        try
        {
            return await store.ChangeAsync(engagementId, version, viewer!.AccountId,
                e => e.AccountantAccountId == viewer.AccountId ? change(e) : null, clock.UtcNow, cancellationToken);
        }
        catch (DataUnavailableException)
        {
            return WorkOutcome.Failed;
        }
    }
}
