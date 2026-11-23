using System.Text.RegularExpressions;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Domain;

/// <summary>
/// F11 LP14: the site's version of the terms and privacy policy is the version written in the approved documents
/// (docs/legal). The pages carry the text by hand, so a text change must move both, and the version with them.
/// </summary>
public sealed partial class LegalTextTests
{
    [Theory]
    [InlineData("terms-of-use.md")]
    [InlineData("privacy-policy.md")]
    public void The_site_shows_the_version_written_in_the_approved_document(string document)
    {
        var text = File.ReadAllText(Path.Combine(RepositoryRoot(), "docs", "legal", document));

        var version = VersionLine().Match(text);

        Assert.True(version.Success, $"{document} has no 'Version:' line.");
        Assert.Equal(LegalText.CurrentVersion, version.Groups[1].Value);
    }

    private static string RepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "docs", "legal")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("The repository's docs/legal folder was not found above the test output.");
    }

    [GeneratedRegex(@"^Version: (\S+)", RegexOptions.Multiline)]
    private static partial Regex VersionLine();
}
