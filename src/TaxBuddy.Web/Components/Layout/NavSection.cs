namespace TaxBuddy.Web.Components.Layout;

/// <summary>
/// Which site bar item a page belongs to, as in the prototype's <c>desktopShell</c> map (2026-11-24). A profile counts
/// as the directory, a service request and every workspace page as the workspace, and the editor and deletion as
/// Account. The review page and the agree page belong to none; nor do report, sign-in and information pages.
/// </summary>
public static class NavSection
{
    public const string Directory = "directory";
    public const string Chats = "chats";
    public const string Workspace = "workspace";
    public const string Account = "account";

    /// <summary>The item for a base-relative path such as "/work/…" (no query or fragment), or null for none.</summary>
    public static string? Of(string path)
    {
        var parts = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var first = parts.Length > 0 ? parts[0].ToLowerInvariant() : "";
        return first switch
        {
            "" => Directory,
            "accountants" when parts.Length == 2 => Directory,
            "accountants" when parts.Length == 3 && Is(parts[2], "request-service") => Workspace,
            "chats" => Chats,
            "workspace" or "clients" or "work" => Workspace,
            "account" when parts.Length == 1 || Is(parts[1], "profile") || Is(parts[1], "delete") => Account,
            _ => null,
        };
    }

    private static bool Is(string part, string value) => string.Equals(part, value, StringComparison.OrdinalIgnoreCase);
}
