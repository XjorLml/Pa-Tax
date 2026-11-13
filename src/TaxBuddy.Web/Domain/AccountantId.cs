namespace TaxBuddy.Web.Domain;

/// <summary>Server-generated identifier of an accountant profile (SDD: identity and mutation).</summary>
public readonly record struct AccountantId(Guid Value)
{
    public static AccountantId New() => new(Guid.NewGuid());

    /// <summary>Parses an identifier from an address. Anything that is not a GUID is rejected, never guessed.</summary>
    public static bool TryParse(string? text, out AccountantId id)
    {
        if (Guid.TryParse(text, out var value) && value != Guid.Empty)
        {
            id = new AccountantId(value);
            return true;
        }

        id = default;
        return false;
    }

    public override string ToString() => Value.ToString("D");
}
