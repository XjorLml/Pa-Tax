using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Domain;

/// <summary>F17 (PRD R25) CD04: the checks on a client's details, the same as the prototype's (F02 BD15).</summary>
public sealed class ClientDetailsRulesTests
{
    private static ClientDetailsInput Valid() => new()
    {
        Name = "Maria Santos",
        Phone = "+63 900 000 0201",
        City = "Pasig City",
        BusinessName = "Santos Bakeshop",
        BusinessType = "Sole proprietor",
        Industry = "Food and drink",
    };

    [Fact]
    public void Complete_details_pass()
    {
        Assert.Empty(ClientDetailsRules.Validate(Valid()));
    }

    [Fact]
    public void Only_a_name_is_needed()
    {
        Assert.Empty(ClientDetailsRules.Validate(new ClientDetailsInput { Name = "Maria Santos" }));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_missing_name_is_refused(string? name)
    {
        var input = Valid();
        input.Name = name;

        Assert.Equal("Enter your name.", ClientDetailsRules.Validate(input)[ClientDetailsRules.Fields.Name]);
    }

    [Fact]
    public void A_name_longer_than_an_account_name_is_refused()
    {
        var input = Valid();
        input.Name = new string('a', AccountLimits.DisplayName + 1);

        Assert.Equal($"Use {AccountLimits.DisplayName} characters or fewer.", ClientDetailsRules.Validate(input)[ClientDetailsRules.Fields.Name]);
    }

    [Theory]
    [InlineData("+63 900 000 0201")]
    [InlineData("(02) 8123-4567")]
    [InlineData("09171234567")]
    public void Ordinary_phone_numbers_pass(string phone)
    {
        Assert.True(ClientDetailsRules.IsPhone(phone));
    }

    [Theory]
    [InlineData("call me")]
    [InlineData("0917 123 45x")]
    [InlineData("12345")]
    [InlineData("+63 900 000 0201 000 000")]
    [InlineData("( ) - - - + ( )")]
    public void Phone_numbers_with_letters_too_few_digits_or_too_long_are_refused(string phone)
    {
        var input = Valid();
        input.Phone = phone;

        Assert.Equal("Enter a phone number using digits, spaces, + or -.", ClientDetailsRules.Validate(input)[ClientDetailsRules.Fields.Phone]);
    }

    [Fact]
    public void Each_text_field_has_its_limit()
    {
        var input = Valid();
        input.City = new string('c', ClientDetailsRules.City + 1);
        input.BusinessName = new string('b', ClientDetailsRules.BusinessName + 1);
        input.Industry = new string('i', ClientDetailsRules.Industry + 1);

        var problems = ClientDetailsRules.Validate(input);

        Assert.Equal("Use 80 characters or fewer.", problems[ClientDetailsRules.Fields.City]);
        Assert.Equal("Use 120 characters or fewer.", problems[ClientDetailsRules.Fields.BusinessName]);
        Assert.Equal("Use 80 characters or fewer.", problems[ClientDetailsRules.Fields.Industry]);
    }

    [Fact]
    public void A_type_must_come_from_the_list()
    {
        var input = Valid();
        input.BusinessType = "Cooperative";

        Assert.Equal("Choose a type from the list.", ClientDetailsRules.Validate(input)[ClientDetailsRules.Fields.BusinessType]);
    }

    [Fact]
    public void Clean_trims_and_stores_blanks_as_null()
    {
        var (name, details) = ClientDetailsRules.Clean(new ClientDetailsInput
        {
            Name = "  Maria Santos ",
            Phone = " ",
            City = " Pasig City ",
            BusinessName = "",
            BusinessType = null,
            Industry = " Food and drink",
        });

        Assert.Equal("Maria Santos", name);
        Assert.Equal(new ClientDetails(null, "Pasig City", null, null, "Food and drink"), details);
        Assert.Equal("Food and drink", details.BusinessLine);
        Assert.False(details.IsEmpty);
        Assert.True(ClientDetails.Empty.IsEmpty);
    }
}
