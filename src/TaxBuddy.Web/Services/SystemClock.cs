using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
