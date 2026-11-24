using TaxBuddy.Web.Components.Layout;

namespace TaxBuddy.Tests.Components;

/// <summary>The site bar's current item follows the prototype's desktopShell map (2026-11-24).</summary>
public sealed class NavSectionTests
{
    [Theory]
    [InlineData("", NavSection.Directory)]
    [InlineData("accountants/acc-1", NavSection.Directory)]
    [InlineData("accountants/acc-1/request-service", NavSection.Workspace)]
    [InlineData("accountants/acc-1/review", null)]
    [InlineData("chats", NavSection.Chats)]
    [InlineData("chats/new/acc-1", NavSection.Chats)]
    [InlineData("workspace", NavSection.Workspace)]
    [InlineData("clients/2b9c3f0e-0000-0000-0000-000000000000", NavSection.Workspace)]
    [InlineData("work/2b9c3f0e-0000-0000-0000-000000000000", NavSection.Workspace)]
    [InlineData("account", NavSection.Account)]
    [InlineData("account/profile", NavSection.Account)]
    [InlineData("account/delete", NavSection.Account)]
    [InlineData("account/agree", null)]
    [InlineData("report/profile/acc-1", null)]
    [InlineData("help", null)]
    public void Each_page_marks_the_prototypes_item(string path, string? expected) =>
        Assert.Equal(expected, NavSection.Of(path));
}
