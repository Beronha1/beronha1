using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Trauma.Common.Language.Systems;
using Content.Shared.Chat;
using Content.Trauma.Common.Language;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.WhiteDream.BloodCult;

public sealed partial class BloodCultChatSystem : EntitySystem
{
    [Dependency] private IAdminManager _adminManager = default!;
    [Dependency] private IChatManager _chatManager = default!;

    [Dependency] private CommonLanguageSystem _language = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultistComponent, EntitySpokeEvent>(OnSpeak);
    }

    private void OnSpeak(EntityUid uid, BloodCultistComponent component, EntitySpokeEvent args)
    {
        if (args.Source != uid || args.Language.ID != component.CultLanguageId || args.IsWhisper)
            return;

        SendMessage(args.Source, args.Message, false, args.Language);
    }

    private void SendMessage(EntityUid source, string message, bool hideChat, LanguagePrototype language)
    {
        var clients = GetClients(language.ID);
        var playerName = Name(source);
        var wrappedMessage = Loc.GetString("chat-manager-send-cult-chat-wrap-message",
            ("channelName", Loc.GetString("chat-manager-cult-channel-name")),
            ("player", playerName),
            ("message", FormattedMessage.EscapeText(message)));

        // Trauma - no Telepathic channel in this fork, the collective mind one is the equivalent
        _chatManager.ChatMessageToMany(ChatChannel.CollectiveMind,
            message,
            wrappedMessage,
            source,
            hideChat,
            true,
            clients.ToList(),
            language.SpeechOverride.Color);
    }

    private IEnumerable<INetChannel> GetClients(string languageId)
    {
        return Filter.Empty()
            .AddWhereAttachedEntity(entity => CanHearBloodCult(entity, languageId))
            .Recipients
            .Union(_adminManager.ActiveAdmins)
            .Select(p => p.Channel);
    }

    private bool CanHearBloodCult(EntityUid entity, string languageId)
    {
        // Trauma - CommonLanguageSystem exposes CanUnderstand instead of GetUnderstoodLanguages
        return _language.CanUnderstand(entity, languageId);
    }
}
