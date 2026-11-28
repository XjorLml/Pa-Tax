using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Application.Work;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>Service work on PostgreSQL (F03). A new context per operation (SDD). Logs carry no names, IDs or notes.</summary>
public sealed class WorkStore(IDbContextFactory<TaxBuddyDbContext> contextFactory, ILogger<WorkStore> logger) : IWorkStore
{
    private static readonly EventId DataUnavailable = new(1007, "WorkDataUnavailable");

    public Task<WorkAccountant?> FindListedAsync(AccountantId profileId, CancellationToken cancellationToken) =>
        Run(db => AccountantAsync(db.AccountantProfiles.Where(p => p.Id == profileId).Where(ListingRules.IsListed), cancellationToken), cancellationToken);

    public Task<WorkAccountant?> FindProfileOfAsync(Guid accountantAccountId, CancellationToken cancellationToken) =>
        Run(db => AccountantAsync(db.AccountantProfiles.Where(p => p.OwnerAccountId == accountantAccountId), cancellationToken), cancellationToken);

    public Task<bool> IsSavedAsync(Guid clientId, AccountantId profileId, CancellationToken cancellationToken) =>
        Run(db => db.SavedAccountants.AnyAsync(s => s.ClientAccountId == clientId && s.ProfileId == profileId, cancellationToken), cancellationToken);

    public Task<bool> IsListedAsync(AccountantId profileId, CancellationToken cancellationToken) =>
        Run(db => db.AccountantProfiles.Where(p => p.Id == profileId).Where(ListingRules.IsListed).AnyAsync(cancellationToken), cancellationToken);

    public Task SaveAccountantAsync(Guid clientId, AccountantId profileId, DateTimeOffset at, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            // ON CONFLICT: saving twice, or in two tabs at once, keeps one row (R20).
            await db.Database.ExecuteSqlAsync(
                $"INSERT INTO saved_accountants (client_account_id, profile_id, saved_at) VALUES ({clientId}, {profileId.Value}, {at}) ON CONFLICT DO NOTHING",
                cancellationToken);
            return true;
        }, cancellationToken);

    public Task RemoveSavedAsync(Guid clientId, AccountantId profileId, CancellationToken cancellationToken) =>
        Run(db => db.SavedAccountants.Where(s => s.ClientAccountId == clientId && s.ProfileId == profileId).ExecuteDeleteAsync(cancellationToken), cancellationToken);

    public Task<ClientWorkspaceView> ClientWorkspaceAsync(Guid clientId, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            var saved = await db.SavedAccountants.AsNoTracking().Where(s => s.ClientAccountId == clientId).OrderBy(s => s.SavedAt)
                .Select(s => s.ProfileId).ToListAsync(cancellationToken);
            var engagements = await db.ServiceEngagements.AsNoTracking().Include(e => e.Tasks)
                .Where(e => e.ClientAccountId == clientId).OrderBy(e => e.CreatedAt).ToListAsync(cancellationToken);
            var profiles = await db.AccountantProfiles.AsNoTracking()
                .Where(p => saved.Contains(p.Id) || (p.OwnerAccountId != null && engagements.Select(e => e.AccountantAccountId).Contains(p.OwnerAccountId.Value)))
                .Select(p => new { p.Id, p.OwnerAccountId, p.DisplayName, p.Title })
                .ToListAsync(cancellationToken);
            var listed = await db.AccountantProfiles.Where(ListingRules.IsListed).Where(p => profiles.Select(x => x.Id).Contains(p.Id))
                .Select(p => p.Id).ToListAsync(cancellationToken);
            var conversations = await db.Conversations.AsNoTracking().Where(c => c.ClientAccountId == clientId)
                .Select(c => new { c.Id, c.AccountantAccountId }).ToListAsync(cancellationToken);
            var documents = await DocumentCountsAsync(db, engagements, cancellationToken);

            var entries = new List<ClientAccountantEntry>();
            foreach (var profileId in saved)
            {
                var p = profiles.FirstOrDefault(x => x.Id == profileId);
                if (p is null)
                {
                    continue;
                }

                entries.Add(Entry(p.Id, p.OwnerAccountId, p.DisplayName, p.Title, listed.Contains(p.Id), true));
            }

            foreach (var accountantId in engagements.Select(e => e.AccountantAccountId).Distinct())
            {
                if (entries.Any(e => e.AccountId == accountantId))
                {
                    continue;
                }

                var p = profiles.FirstOrDefault(x => x.OwnerAccountId == accountantId);
                entries.Add(p is null
                    ? Entry(null, accountantId, "Accountant no longer listed", null, false, false)
                    : Entry(p.Id, accountantId, p.DisplayName, p.Title, listed.Contains(p.Id), false));
            }

            return new ClientWorkspaceView(entries);

            ClientAccountantEntry Entry(AccountantId? profileId, Guid? accountId, string name, string? title, bool isListed, bool isSaved) =>
                new(profileId, accountId, name, title, isListed, isSaved,
                    conversations.FirstOrDefault(c => c.AccountantAccountId == accountId)?.Id,
                    engagements.Where(e => e.AccountantAccountId == accountId).Select(e => Card(e, documents)).ToList());
        }, cancellationToken);

    public Task<AccountantWorkspaceView> AccountantWorkspaceAsync(Guid accountantId, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            var engagements = await db.ServiceEngagements.AsNoTracking().Include(e => e.Tasks)
                .Where(e => e.AccountantAccountId == accountantId).ToListAsync(cancellationToken);
            var clientIds = engagements.Select(e => e.ClientAccountId).Distinct().ToList();
            var names = await db.Users.AsNoTracking().Where(u => clientIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);
            var clients = clientIds
                .Select(id => new AccountantClientEntry(id, names.GetValueOrDefault(id, "Client"),
                    engagements.Count(e => e.ClientAccountId == id),
                    engagements.Count(e => e.ClientAccountId == id && e.Phase == ServicePhase.Requested)))
                .OrderByDescending(c => c.Requests).ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ThenBy(c => c.ClientId)
                .ToList();
            return new AccountantWorkspaceView(clients,
                engagements.Count(e => e.Phase == ServicePhase.Active && !e.IsCompleted),
                engagements.Count(e => e.Phase == ServicePhase.Requested));
        }, cancellationToken);

    public Task<ClientServicesView?> ClientServicesAsync(Guid accountantId, Guid clientId, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            var engagements = await db.ServiceEngagements.AsNoTracking().Include(e => e.Tasks)
                .Where(e => e.AccountantAccountId == accountantId && e.ClientAccountId == clientId)
                .OrderBy(e => e.CreatedAt).ToListAsync(cancellationToken);
            if (engagements.Count == 0)
            {
                return null;
            }

            // F17 (R25): the client shares their details with accountants they have a service with, as here.
            var client = await db.Users.Where(u => u.Id == clientId)
                .Select(u => new { u.DisplayName, u.PhoneNumber, u.City, u.BusinessName, u.BusinessType, u.Industry }).FirstOrDefaultAsync(cancellationToken);
            var name = client?.DisplayName ?? "Client";
            var details = client is null ? ClientDetails.Empty : new ClientDetails(client.PhoneNumber, client.City, client.BusinessName, client.BusinessType, client.Industry);
            var conversation = await db.Conversations.Where(c => c.ClientAccountId == clientId && c.AccountantAccountId == accountantId)
                .Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            var canAdd = new List<string>();
            if (engagements.Any(e => e.Phase == ServicePhase.Active)
                && await AccountantAsync(db.AccountantProfiles.Where(p => p.OwnerAccountId == accountantId), cancellationToken) is { } profile)
            {
                var open = engagements.Where(e => e.Phase != ServicePhase.Declined).Select(e => e.ServiceName).ToHashSet();
                canAdd.AddRange(profile.Services.Where(s => !open.Contains(s)));
            }

            var documents = await DocumentCountsAsync(db, engagements, cancellationToken);
            return new ClientServicesView(clientId, name, conversation, engagements.Select(e => Card(e, documents)).ToList(), canAdd, details);
        }, cancellationToken);

    public Task<EngagementView?> ServiceAsync(Guid engagementId, Guid viewerId, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            var e = await db.ServiceEngagements.AsNoTracking().Include(x => x.Tasks).FirstOrDefaultAsync(x => x.Id == engagementId, cancellationToken);
            if (e is null || !e.Includes(viewerId))
            {
                return null; // Nothing about someone else's service, not even that it exists (BR15).
            }

            var client = await db.Users.Where(u => u.Id == e.ClientAccountId)
                .Select(u => new { u.DisplayName, u.PhoneNumber, u.City, u.BusinessName, u.BusinessType, u.Industry }).FirstOrDefaultAsync(cancellationToken);
            var clientName = client?.DisplayName ?? "Client";
            // F17 (R25): only the service's accountant sees the client's details here.
            var clientDetails = e.AccountantAccountId == viewerId && client is not null
                ? new ClientDetails(client.PhoneNumber, client.City, client.BusinessName, client.BusinessType, client.Industry)
                : null;
            var profile = await db.AccountantProfiles.Where(p => p.OwnerAccountId == e.AccountantAccountId)
                .Select(p => new { p.Id, p.DisplayName }).FirstOrDefaultAsync(cancellationToken);
            var accountantName = profile?.DisplayName
                                 ?? await db.Users.Where(u => u.Id == e.AccountantAccountId).Select(u => u.DisplayName).FirstOrDefaultAsync(cancellationToken)
                                 ?? "Accountant";
            var listedProfile = profile is not null && await db.AccountantProfiles.Where(p => p.Id == profile.Id).Where(ListingRules.IsListed).AnyAsync(cancellationToken)
                ? profile.Id
                : (AccountantId?)null;
            var updates = await db.ServiceUpdates.AsNoTracking().Where(u => u.EngagementId == e.Id)
                .OrderByDescending(u => u.CreatedAt).ThenByDescending(u => u.Id)
                .Select(u => new { u.AuthorAccountId, u.CreatedAt, u.Text, u.IsNote }).ToListAsync(cancellationToken);
            var conversation = await db.Conversations.Where(c => c.ClientAccountId == e.ClientAccountId && c.AccountantAccountId == e.AccountantAccountId)
                .Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            var files = await db.ServiceFiles.AsNoTracking().Where(f => f.EngagementId == e.Id)
                .OrderBy(f => f.CreatedAt).ThenBy(f => f.Id).ToListAsync(cancellationToken);
            var tasks = e.Tasks.ToDictionary(t => t.Id, t => t.Title);
            string NameOf(Guid accountId) => accountId == e.AccountantAccountId ? accountantName : clientName;

            return new EngagementView(e.Id, e.ServiceName, e.Phase, e.StatusText, e.IsCompleted, e.ClientAccountId, clientName, accountantName, listedProfile,
                e.AccountantAccountId == viewerId, e.Version,
                e.Tasks.OrderBy(t => t.Position).Select(t => new TaskView(t.Id, t.Title, t.Status)).ToList(),
                updates.Select(u => new UpdateView(NameOf(u.AuthorAccountId), u.CreatedAt, u.Text, u.IsNote)).ToList(),
                conversation,
                files.Select(f => new FileView(f.Id, f.DisplayName, NameOf(f.UploaderAccountId), f.CreatedAt,
                    f.TaskId is { } taskId ? tasks.GetValueOrDefault(taskId, "Service task") : null,
                    f.UploaderAccountId == viewerId, f.Length)).ToList(),
                clientDetails);
        }, cancellationToken);

    public Task<ServiceEngagement?> FindAsync(Guid engagementId, CancellationToken cancellationToken) =>
        Run(db => db.ServiceEngagements.AsNoTracking().Include(e => e.Tasks).FirstOrDefaultAsync(e => e.Id == engagementId, cancellationToken), cancellationToken);

    public Task<Guid?> FindOpenAsync(Guid clientId, Guid accountantId, string serviceName, CancellationToken cancellationToken) =>
        Run(db => db.ServiceEngagements
            .Where(e => e.ClientAccountId == clientId && e.AccountantAccountId == accountantId && e.ServiceName == serviceName && e.Phase != ServicePhase.Declined)
            .Select(e => (Guid?)e.Id).FirstOrDefaultAsync(cancellationToken), cancellationToken);

    public Task<bool> HasActiveAsync(Guid clientId, Guid accountantId, CancellationToken cancellationToken) =>
        Run(db => db.ServiceEngagements.AnyAsync(e => e.ClientAccountId == clientId && e.AccountantAccountId == accountantId && e.Phase == ServicePhase.Active, cancellationToken), cancellationToken);

    public async Task<WorkResult> AddAsync(ServiceEngagement engagement, Guid authorId, string activity, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            db.ServiceEngagements.Add(engagement);
            db.ServiceUpdates.Add(new ServiceUpdate(Guid.NewGuid(), engagement.Id, authorId, activity, false, engagement.CreatedAt));
            await db.SaveChangesAsync(cancellationToken);
            return new WorkResult(WorkOutcome.Done, engagement.Id);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // The same service was requested at the same moment (another tab): keep that one.
            var existing = await FindOpenAsync(engagement.ClientAccountId, engagement.AccountantAccountId, engagement.ServiceName, cancellationToken);
            return new WorkResult(existing is null ? WorkOutcome.Failed : WorkOutcome.Existing, existing);
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    public async Task<WorkOutcome> ChangeAsync(Guid engagementId, int? expectedVersion, Guid authorId, EngagementChange change, DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            var engagement = await db.ServiceEngagements.Include(e => e.Tasks).FirstOrDefaultAsync(e => e.Id == engagementId, cancellationToken);
            if (engagement is null)
            {
                return WorkOutcome.NotAllowed;
            }

            if (expectedVersion is { } version && version != engagement.Version)
            {
                return WorkOutcome.Stale;
            }

            if (change(engagement) is not { } lines)
            {
                return WorkOutcome.NotAllowed;
            }

            foreach (var (text, isNote) in lines)
            {
                db.ServiceUpdates.Add(new ServiceUpdate(Guid.NewGuid(), engagement.Id, authorId, text, isNote, at));
            }

            await db.SaveChangesAsync(cancellationToken);
            return WorkOutcome.Done;
        }
        catch (DbUpdateConcurrencyException)
        {
            return WorkOutcome.Stale;
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    public Task<UploadTarget?> UploadTargetAsync(Guid engagementId, Guid viewerId, Guid operationId, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            var e = await db.ServiceEngagements.AsNoTracking().Include(x => x.Tasks).FirstOrDefaultAsync(x => x.Id == engagementId, cancellationToken);
            if (e is null || !e.Includes(viewerId))
            {
                return null;
            }

            var files = await db.ServiceFiles.CountAsync(f => f.EngagementId == engagementId, cancellationToken);
            var sent = await db.ServiceFiles.AnyAsync(f => f.UploaderAccountId == viewerId && f.OperationId == operationId, cancellationToken);
            return new UploadTarget(e.Phase, e.Tasks.ToDictionary(t => t.Id, t => t.Title), files, sent);
        }, cancellationToken);

    public async Task<WorkOutcome> AddFileAsync(ServiceFile file, string activity, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            // Locks the service row, so two uploads at once can't both take the last place.
            var phase = (await db.Database
                .SqlQuery<string>($"SELECT phase AS \"Value\" FROM service_engagements WHERE id = {file.EngagementId} AND (client_account_id = {file.UploaderAccountId} OR accountant_account_id = {file.UploaderAccountId}) FOR UPDATE")
                .ToListAsync(cancellationToken)).FirstOrDefault();
            var uploaderActive = await db.Users.AnyAsync(u => u.Id == file.UploaderAccountId && u.DeletedAt == null, cancellationToken);
            if (phase != nameof(ServicePhase.Active) || !uploaderActive
                || (file.TaskId is { } taskId && !await db.ServiceTasks.AnyAsync(t => t.Id == taskId && t.EngagementId == file.EngagementId, cancellationToken)))
            {
                return WorkOutcome.NotAllowed;
            }

            if (await db.ServiceFiles.AnyAsync(f => f.UploaderAccountId == file.UploaderAccountId && f.OperationId == file.OperationId, cancellationToken))
            {
                return WorkOutcome.Existing;
            }

            if (await db.ServiceFiles.CountAsync(f => f.EngagementId == file.EngagementId, cancellationToken) >= DocumentRules.FilesPerService)
            {
                return WorkOutcome.Full;
            }

            db.ServiceFiles.Add(file);
            db.ServiceUpdates.Add(new ServiceUpdate(Guid.NewGuid(), file.EngagementId, file.UploaderAccountId, activity, false, file.CreatedAt));
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return WorkOutcome.Done;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return WorkOutcome.Existing; // the same form sent twice at once: the other request stored it
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    public Task<StoredFile?> FileAsync(Guid engagementId, Guid fileId, Guid viewerId, CancellationToken cancellationToken) =>
        Run(db => db.ServiceFiles.AsNoTracking()
            .Where(f => f.Id == fileId && f.EngagementId == engagementId)
            .Join(db.ServiceEngagements.Where(e => e.ClientAccountId == viewerId || e.AccountantAccountId == viewerId), f => f.EngagementId, e => e.Id, (f, _) => f)
            .Select(f => new StoredFile(f.ObjectName, f.DisplayName, f.ContentType))
            .FirstOrDefaultAsync(cancellationToken), cancellationToken);

    public async Task<string?> RemoveFileAsync(Guid engagementId, Guid fileId, Guid uploaderId, DateTimeOffset at, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            var file = await db.ServiceFiles.FirstOrDefaultAsync(f => f.Id == fileId && f.EngagementId == engagementId, cancellationToken);
            if (file is null || file.UploaderAccountId != uploaderId
                || !await db.ServiceEngagements.AnyAsync(e => e.Id == engagementId && (e.ClientAccountId == uploaderId || e.AccountantAccountId == uploaderId), cancellationToken))
            {
                return null;
            }

            db.ServiceFiles.Remove(file);
            db.ServiceUpdates.Add(new ServiceUpdate(Guid.NewGuid(), engagementId, uploaderId, $"Removed {file.DisplayName} from this service.", false, at));
            await db.SaveChangesAsync(cancellationToken);
            return file.ObjectName;
        }
        catch (DbUpdateConcurrencyException)
        {
            return null; // removed meanwhile, in another tab
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    private static ServiceCard Card(ServiceEngagement e, IReadOnlyDictionary<Guid, int> documents)
    {
        var (done, total) = e.Progress;
        return new ServiceCard(e.Id, e.ServiceName, e.StatusText, e.Phase, e.IsCompleted, done, total, documents.GetValueOrDefault(e.Id));
    }

    private static async Task<IReadOnlyDictionary<Guid, int>> DocumentCountsAsync(TaxBuddyDbContext db, IReadOnlyList<ServiceEngagement> engagements,
        CancellationToken cancellationToken)
    {
        var ids = engagements.Select(e => e.Id).ToList();
        return await db.ServiceFiles.Where(f => ids.Contains(f.EngagementId)).GroupBy(f => f.EngagementId)
            .Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
    }

    private static async Task<WorkAccountant?> AccountantAsync(IQueryable<AccountantProfile> profiles, CancellationToken cancellationToken)
    {
        var p = await profiles.AsNoTracking().Include(x => x.Services).FirstOrDefaultAsync(cancellationToken);
        return p?.OwnerAccountId is { } owner
            ? new WorkAccountant(p.Id, owner, p.DisplayName, p.Title, p.Services.Select(s => s.Name).ToList())
            : null; // A profile without an account can't take requests, as it can't receive messages (F07).
    }

    private async Task<T> Run<T>(Func<TaxBuddyDbContext, Task<T>> work, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await work(db);
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    private async Task Run(Func<TaxBuddyDbContext, Task<int>> work, CancellationToken cancellationToken) => await Run<int>(work, cancellationToken);

    private static bool IsDataFailure(Exception ex) => ex switch
    {
        DbException or TimeoutException or RetryLimitExceededException or DbUpdateException => true,
        InvalidOperationException { InnerException: DbException or TimeoutException } => true,
        _ => false,
    };

    private DataUnavailableException Unavailable(Exception ex)
    {
        logger.LogWarning(DataUnavailable, "Work data unavailable ({ErrorType})", ex.GetType().Name);
        return new DataUnavailableException(ex);
    }
}
