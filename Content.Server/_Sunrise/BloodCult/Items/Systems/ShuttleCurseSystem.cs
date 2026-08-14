using Content.Server._Sunrise.BloodCult.Items.Components;
using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Shared._Sunrise.BloodCult.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._Sunrise.BloodCult.Items.Systems;

public sealed partial class ShuttleCurseSystem : EntitySystem
{
    private const int MaxCurses = 3;
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private RoundEndSystem _roundEnd = default!;
    private int _currentCurses = 0;
    private TimeSpan? _nextCurse = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShuttleCurseComponent, UseInHandEvent>(OnUse);
        // 🌇Sunset🌇 - this fork has no standalone RoundEndedEvent; GameRunLevelChangedEvent -> PostRound
        // is the equivalent "the round just ended" hook (same pattern used elsewhere in this fork,
        // e.g. NukeopsRuleSystem.OnRunLevelChanged).
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (ev.New != GameRunLevel.PostRound)
            return;

        _currentCurses = 0;
        _nextCurse = TimeSpan.Zero;
    }

    private void OnUse(EntityUid uid, ShuttleCurseComponent component, UseInHandEvent args)
    {
        if (!HasComp<BloodCultistComponent>(args.User))
        {
            _hands.TryDrop(args.User);
            _popup.PopupEntity(Loc.GetString("shuttle-curse-not-cultist"), args.User, args.User);
            return;
        }

        if (!_roundEnd.IsRoundEndRequested())
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-shuttle-not-called"), args.User, args.User);
            return;
        }

        if (_currentCurses >= MaxCurses)
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-max-curses"), args.User, args.User);
            return;
        }

        if (_nextCurse > _gameTiming.CurTime)
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-cooldown"), args.User, args.User);
            return;
        }

        var shuttle = _entMan.System<EmergencyShuttleSystem>();

        if (shuttle.EmergencyShuttleArrived)
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-shuttle-arrived"), args.User, args.User);
            return;
        }

        // 🌇Sunset🌇 - this fork's RoundEndSystem has no DelayCursedShuttle helper; ExpectedCountdownEnd
        // is a public settable field on the system itself, so push it back directly.
        if (_roundEnd.ExpectedCountdownEnd is { } countdownEnd)
            _roundEnd.ExpectedCountdownEnd = countdownEnd + component.DelayTime;
        _popup.PopupEntity(Loc.GetString("shuttle-curse-shuttle-delayed"), args.User, args.User);

        _currentCurses++;
        _nextCurse = _gameTiming.CurTime + component.Cooldown;
    }
}
