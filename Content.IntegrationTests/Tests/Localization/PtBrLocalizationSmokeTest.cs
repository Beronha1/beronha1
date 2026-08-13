using System.Globalization;
using Content.IntegrationTests.Fixtures;
using Robust.Shared.Localization;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests.Localization;

public sealed class PtBrLocalizationSmokeTest : GameTest
{
    [Test]
    public void CatalogLoadsAndFormatsRegressionMessages()
    {
        var localization = Pair.Server.ResolveDependency<ILocalizationManager>();
        var originalCulture = localization.DefaultCulture;

        try
        {
            localization.SetCulture(CultureInfo.GetCultureInfo("pt-BR"));

            Assert.Multiple(() =>
            {
                Assert.That(localization.HasString("tiles-cosmiccult-floor-void-unremovable"), Is.True);
                Assert.That(localization.HasString("tiles-fairy-grass"), Is.True);
                Assert.That(localization.HasString("replay-info-none-selected"), Is.True);

                AssertFormats(localization,
                    "armor-coefficient-value-trauma",
                    ("type", "Balístico"),
                    ("protect", true),
                    ("value", 25f));
                AssertFormats(localization,
                    "cmd-ftldisk-map-paused",
                    ("destination", "Destino"),
                    ("map", "Mapa"));
                AssertFormats(localization,
                    "power-monitoring-window-station-name",
                    ("stationName", "Estação"));
                AssertFormats(localization, "ghost-role-information-nonantagonist-rules");
                AssertFormats(localization,
                    "deathrattle-implant-dead-message",
                    ("user", "Tripulante"),
                    ("position", "na manutenção"));
                AssertFormats(localization,
                    "entity-effect-guidebook-regenerate-part",
                    ("chance", 1f),
                    ("slot", "coração"));

                AssertFormats(localization,
                    "contraband-examine-text-Major",
                    ("type", "item"),
                    ("color", "red"));
                Assert.That(
                    localization.GetString("contraband-job-plural", ("job", "engenheiro")),
                    Is.EqualTo("engenheiros"));
                AssertFormatsAsMarkup(localization,
                    "scannable-solution-chemical",
                    ("amount", 10),
                    ("color", "#ff0000"),
                    ("type", "sangue"));
            });
        }
        finally
        {
            if (originalCulture != null)
                localization.SetCulture(originalCulture);
        }
    }

    private static void AssertFormats(
        ILocalizationManager localization,
        string id,
        params (string, object)[] args)
    {
        Assert.That(localization.HasString(id), Is.True, $"Missing localization ID: {id}");
        Assert.That(() => localization.GetString(id, args), Throws.Nothing, $"Failed to format: {id}");
    }

    private static void AssertFormatsAsMarkup(
        ILocalizationManager localization,
        string id,
        params (string, object)[] args)
    {
        Assert.That(localization.HasString(id), Is.True, $"Missing localization ID: {id}");
        var markup = localization.GetString(id, args);
        Assert.That(() => FormattedMessage.FromMarkupOrThrow(markup), Throws.Nothing, $"Invalid markup: {id}");
    }
}
