using Content.Client.PDA;
using Content.IntegrationTests.Fixtures;

namespace Content.IntegrationTests.Tests.PDA;

[TestFixture]
public sealed class PdaMenuTest : GameTest
{
    [Test]
    public async Task OpensOnHomeAndRestoresNavigationViews()
    {
        await Client.WaitAssertion(() =>
        {
            var menu = new PdaMenu();

            AssertActiveView(menu, PdaMenu.HomeView);

            menu.RestoreView(PdaMenu.ProgramListView);
            AssertActiveView(menu, PdaMenu.ProgramListView);

            menu.RestoreView(PdaMenu.SettingsView);
            AssertActiveView(menu, PdaMenu.SettingsView);

            menu.RestoreView(PdaMenu.ProgramContentView);
            AssertActiveView(menu, PdaMenu.ProgramContentView);
            Assert.Multiple(() =>
            {
                Assert.That(menu.ProgramTitle.Visible, Is.True);
                Assert.That(menu.ProgramCloseButton.Visible, Is.True);
                Assert.That(menu.ProgramListButton.Visible, Is.False);
            });

            // Health scan is not a persisted navigation destination and must safely fall back to home.
            menu.RestoreView(PdaMenu.HealthScanViewIndex);
            AssertActiveView(menu, PdaMenu.HomeView);
            Assert.Multiple(() =>
            {
                Assert.That(menu.ProgramTitle.Visible, Is.False);
                Assert.That(menu.ProgramCloseButton.Visible, Is.False);
                Assert.That(menu.ProgramListButton.Visible, Is.True);
            });
        });
    }

    private static void AssertActiveView(PdaMenu menu, int expected)
    {
        Assert.That(menu.CurrentView, Is.EqualTo(expected));

        for (var i = 0; i < menu.ViewContainer.ChildCount; i++)
        {
            Assert.That(menu.ViewContainer.GetChild(i).Visible, Is.EqualTo(i == expected),
                $"PDA view {i} has the wrong visibility while view {expected} is active.");
        }
    }
}
