using System.Globalization;
using Content.IntegrationTests.Fixtures;
using Robust.Shared.Localization;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests.Chat;

public sealed class PtBrChatLocalizationTest : GameTest
{
    private const string Message = "Mensagem com acentuação: ação, órgão, você.";

    [Test]
    public void ChatTemplatesPreserveMessagesVariablesAndMarkup()
    {
        var localization = Pair.Server.ResolveDependency<ILocalizationManager>();
        var originalCulture = localization.DefaultCulture;

        try
        {
            localization.SetCulture(CultureInfo.GetCultureInfo("pt-BR"));

            AssertMarkupContains(localization,
                "chat-manager-entity-say-wrap-message",
                Message,
                ("entityName", "José da Conceição"),
                ("verb", "diz"),
                ("fontType", "Default"),
                ("fontSize", 12),
                ("color", "#ffffff"),
                ("message", Message));
            AssertMarkupContains(localization,
                "chat-manager-entity-say-bold-wrap-message",
                Message,
                ("entityName", "José da Conceição"),
                ("verb", "grita"),
                ("fontType", "Default"),
                ("fontSize", 12),
                ("color", "#ffffff"),
                ("message", Message));
            AssertMarkupContains(localization,
                "chat-manager-entity-say-bolded-language-wrap-message",
                Message,
                ("entityName", "José da Conceição"),
                ("verb", "declara"),
                ("fontType", "Default"),
                ("boldFontType", "DefaultBold"),
                ("fontSize", 12),
                ("color", "#ffffff"),
                ("message", Message));

            AssertMarkupContains(localization,
                "chat-manager-entity-looc-wrap-message",
                Message,
                ("entityName", "José da Conceição"),
                ("message", Message));
            AssertMarkupContains(localization,
                "chat-manager-send-ooc-wrap-message",
                Message,
                ("playerName", "Jogador"),
                ("message", Message));
            AssertMarkupContains(localization,
                "chat-manager-send-ooc-patron-wrap-message",
                Message,
                ("playerName", "Jogador"),
                ("patronColor", "#ffffff"),
                ("message", Message));
            AssertMarkupContains(localization,
                "chat-manager-send-admin-chat-wrap-message",
                Message,
                ("adminChannelName", "ADMIN"),
                ("playerName", "Administrador"),
                ("message", Message));
            AssertMarkupContains(localization,
                "chat-manager-send-admin-announcement-wrap-message",
                Message,
                ("adminChannelName", "ADMIN"),
                ("message", Message));
            AssertMarkupContains(localization,
                "chat-manager-send-dead-chat-wrap-message",
                Message,
                ("deadChannelName", "MORTOS"),
                ("playerName", "Fantasma"),
                ("verb", "lamenta"),
                ("message", Message));
            AssertMarkupContains(localization,
                "chat-manager-send-admin-dead-chat-wrap-message",
                Message,
                ("adminChannelName", "ADMIN"),
                ("userName", "Administrador"),
                ("verb", "diz"),
                ("message", Message));
            AssertMarkupContains(localization,
                "chat-manager-send-hook-ooc-wrap-message",
                Message,
                ("senderName", "Discord"),
                ("message", Message));

            var typing = localization.GetString(
                "bwoink-system-typing-indicator",
                ("players", "Ana"),
                ("count", 1));
            Assert.That(typing, Does.Contain("Ana"));
            Assert.That(typing, Does.Contain("digitando"));
        }
        finally
        {
            if (originalCulture != null)
                localization.SetCulture(originalCulture);
        }
    }

    private static void AssertMarkupContains(
        ILocalizationManager localization,
        string id,
        string expected,
        params (string, object)[] args)
    {
        Assert.That(localization.HasString(id), Is.True, $"Missing localization ID: {id}");
        var markup = localization.GetString(id, args);
        Assert.That(markup, Does.Contain(expected), $"Template discarded message text: {id}");
        Assert.That(() => FormattedMessage.FromMarkupOrThrow(markup), Throws.Nothing, $"Invalid markup: {id}");
    }
}
