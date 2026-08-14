using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Preferences.Managers;
using Content.Shared._ES.Lobby;
using Content.Shared._ES.Lobby.Components;
using Content.Shared.Alert;
using Content.Shared.GameTicking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._ES.Lobby;

/// <summary>
/// Handles server-side diegetic lobby behavior, including ready triggers.
/// </summary>
public sealed partial class ESDiegeticLobbySystem : ESSharedDiegeticLobbySystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IServerPreferencesManager _preferences = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private GameTicker _ticker = default!;

    private static readonly ProtoId<AlertPrototype> NotReadiedAlert = "ESNotReadiedUp";

    private readonly Dictionary<ProtoId<JobPrototype>, int> _readiedJobCounts = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESTheatergoerMarkerComponent, ComponentInit>(OnTheatergoerInit);
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);

        _player.PlayerStatusChanged += OnPlayerStatusChanged;
        _preferences.ESOnAfterCharacterUpdated += RefreshReadiedJobCounts;
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus is SessionStatus.Disconnected or SessionStatus.Zombie or SessionStatus.Connecting)
            return;

        RaiseNetworkEvent(new ESUpdatePlayerReadiedJobCounts(_readiedJobCounts), args.Session);
    }

    protected override void OnPlayerReadyToggled(ESOnPlayerReadyToggled ev)
    {
        base.OnPlayerReadyToggled(ev);
        RefreshReadiedJobCounts();
    }

    private void RefreshReadiedJobCounts()
    {
        _readiedJobCounts.Clear();

        foreach (var session in _player.Sessions)
        {
            if (session.Status is SessionStatus.Disconnected or SessionStatus.Zombie)
                continue;
            if (!_ticker.PlayerGameStatuses.TryGetValue(session.UserId, out var status) ||
                status != PlayerGameStatus.ReadyToPlay)
                continue;
            if (!_preferences.TryGetCachedPreferences(session.UserId, out var preferences))
                continue;

            var profile = (HumanoidCharacterProfile) preferences.SelectedCharacter;
            foreach (var (job, priority) in profile.JobPriorities)
            {
                if (priority == JobPriority.Never)
                    continue;

                _readiedJobCounts[job] = _readiedJobCounts.GetOrNew(job) + 1;
            }
        }

        RaiseNetworkEvent(new ESUpdatePlayerReadiedJobCounts(_readiedJobCounts));
    }

    protected override void OnTriggerCollided(Entity<ESReadyTriggerMarkerComponent> ent, ref StartCollideEvent args)
    {
        if (!HasComp<ESTheatergoerMarkerComponent>(args.OtherEntity) ||
            !TryComp<ActorComponent>(args.OtherEntity, out var actor) ||
            ent.Comp.Behavior is not (PlayerGameStatus.NotReadyToPlay or PlayerGameStatus.ReadyToPlay))
            return;

        if (_ticker.RunLevel == GameRunLevel.PreRoundLobby)
            _ticker.ToggleReady(actor.PlayerSession, ent.Comp.Behavior == PlayerGameStatus.ReadyToPlay);
    }

    private void OnTheatergoerInit(Entity<ESTheatergoerMarkerComponent> ent, ref ComponentInit args)
    {
        if (_ticker.RunLevel == GameRunLevel.PreRoundLobby)
            _alerts.ShowAlert(ent.Owner, NotReadiedAlert);
    }

    private void OnRoundStarting(RoundStartingEvent ev)
    {
        var query = EntityQueryEnumerator<ESTheatergoerMarkerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            Actions.RemoveAction(uid, comp.ConfigurePrefsActionEntity);
            comp.ConfigurePrefsActionEntity = null;
        }
    }
}
