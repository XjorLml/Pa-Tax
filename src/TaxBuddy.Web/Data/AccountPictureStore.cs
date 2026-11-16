using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Application.Pictures;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>Which picture each account has (F06), on the <c>users</c> table.</summary>
public sealed class AccountPictureStore(IDbContextFactory<TaxBuddyDbContext> contextFactory, ILogger<AccountPictureStore> logger)
    : IAccountPictureStore
{
    private static readonly EventId DataUnavailable = new(1003, "PictureDataUnavailable");

    public async Task<AccountPicture?> FindAsync(Guid accountId, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await db.Users.AsNoTracking()
                .Where(u => u.Id == accountId)
                .Select(u => new AccountPicture(u.Role == AccountRole.Accountant, u.PictureObject))
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is DbException or TimeoutException or InvalidOperationException { InnerException: DbException })
        {
            throw Unavailable(ex);
        }
    }

    public async Task<string?> SetAsync(Guid accountId, string? objectName, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == accountId, cancellationToken)
                       ?? throw new InvalidOperationException("The account no longer exists.");
            var previous = user.PictureObject;
            user.PictureObject = objectName;
            await db.SaveChangesAsync(cancellationToken);
            return previous;
        }
        catch (Exception ex) when (ex is DbException or TimeoutException or DbUpdateException or InvalidOperationException { InnerException: DbException })
        {
            throw Unavailable(ex);
        }
    }

    private DataUnavailableException Unavailable(Exception ex)
    {
        logger.LogWarning(DataUnavailable, "Picture data unavailable ({ErrorType})", ex.GetType().Name);
        return new DataUnavailableException(ex);
    }
}
