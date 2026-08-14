#nullable enable
using Content.Client.UserInterface.Systems.Chat;
using Content.Client.UserInterface.Systems.Chat.Widgets;
using Content.Goobstation.UIKit.UserInterface.Controls;
using Content.Goobstation.Common.CCVar;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.Shared.Chat;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;

namespace Content.IntegrationTests.Tests.Chat;

public sealed class ChatBoxRepopulateTest : GameTest
{
    [SidedDependency(Side.Client)] private readonly IConfigurationManager _configuration = null!;
    [SidedDependency(Side.Client)] private readonly IUserInterfaceManager _uiManager = null!;

    [Test]
    [RunOnSide(Side.Client)]
    public void RepeatedRepopulateResetsCoalescenceState()
    {
        var controller = _uiManager.GetUIController<ChatUIController>();
        var previousHistory = controller.History.ToArray();
        var previousCoalescence = _configuration.GetCVar(GoobCVars.CoalesceIdenticalMessages);
        ChatWindow? window = null;

        try
        {
            controller.History.Clear();
            controller.History.Add((GameTick.Zero, new ChatMessage(
                ChatChannel.Local,
                "coalescence regression",
                "coalescence regression",
                NetEntity.Invalid,
                null)));
            _configuration.SetCVar(GoobCVars.CoalesceIdenticalMessages, true);

            Assert.That(() => window = new ChatWindow(), Throws.Nothing);
            Assert.That(window, Is.Not.Null);
            var chatBox = FindControl<ChatBox>(window!);
            Assert.That(chatBox, Is.Not.Null);
            var output = FindControl<CustomOutputPanel>(chatBox!);
            Assert.That(output, Is.Not.Null);
            Assert.That(output!.EntryCount, Is.EqualTo(1));

            Assert.That(chatBox!.Repopulate, Throws.Nothing);
            Assert.That(output.EntryCount, Is.EqualTo(1));

            controller.History.Clear();
            controller.History.Add((GameTick.Zero, new ChatMessage(
                ChatChannel.Local,
                "do not coalesce",
                "do not coalesce",
                NetEntity.Invalid,
                null,
                canCoalesce: false)));
            controller.History.Add((GameTick.Zero, new ChatMessage(
                ChatChannel.Local,
                "do not coalesce",
                "do not coalesce",
                NetEntity.Invalid,
                null)));

            Assert.That(chatBox.Repopulate, Throws.Nothing);
            Assert.That(output.EntryCount, Is.EqualTo(2),
                "A non-coalescible message must not be merged with the next message.");
        }
        finally
        {
#pragma warning disable CS0618 // Test-created controls must release their event subscriptions.
            window?.Dispose();
#pragma warning restore CS0618
            controller.History.Clear();
            controller.History.AddRange(previousHistory);
            _configuration.SetCVar(GoobCVars.CoalesceIdenticalMessages, previousCoalescence);
        }
    }

    [Test]
    public void ChatMessageCopyPreservesBehaviorFlags()
    {
        var original = new ChatMessage(
            ChatChannel.Radio,
            "radio message",
            "radio message",
            NetEntity.Invalid,
            null,
            canCoalesce: true,
            hidePopup: true)
        {
            Read = true,
        };

        var copy = new ChatMessage(original);

        Assert.Multiple(() =>
        {
            Assert.That(copy.CanCoalesce, Is.EqualTo(original.CanCoalesce));
            Assert.That(copy.HidePopup, Is.EqualTo(original.HidePopup));
            Assert.That(copy.Read, Is.EqualTo(original.Read));
        });
    }

    private static T? FindControl<T>(Control root) where T : Control
    {
        if (root is T match)
            return match;

        foreach (var child in root.Children)
        {
            if (FindControl<T>(child) is { } nested)
                return nested;
        }

        return null;
    }
}
