using Content.Shared.AlertLevel;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Shared.DoAfter;
using Content.Shared.WhiteDream.BloodCult.Runes;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.WhiteDream.BloodCult.Runes.Rending;

public sealed partial class CultRuneRendingSystem : EntitySystem
{
    [Dependency] private AlertLevelSystem _alertLevel = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private BloodCultRuleSystem _cultRule = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private NavMapSystem _navMap = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CultRuneRendingComponent, AfterRunePlaced>(OnRendingRunePlaced);
        SubscribeLocalEvent<CultRuneRendingComponent, TryInvokeCultRuneEvent>(OnRendingRuneInvoked);
        SubscribeLocalEvent<CultRuneRendingComponent, RendingRuneDoAfter>(SpawnNarSie);
    }

    private void OnRendingRunePlaced(Entity<CultRuneRendingComponent> rune, ref AfterRunePlaced args)
    {
        var position = _transform.GetMapCoordinates(rune);
        var message = Loc.GetString(
            "cult-rending-drawing-finished",
            ("location", FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString(position))));

        _chat.DispatchGlobalAnnouncement(
            message,
            Loc.GetString("blood-cult-title"),
            true,
            rune.Comp.FinishedDrawingAudio,
            Color.DarkRed);
    }

    private void OnRendingRuneInvoked(Entity<CultRuneRendingComponent> rune, ref TryInvokeCultRuneEvent args)
    {
        if (!_cultRule.IsObjectiveFinished())
        {
            _popup.PopupEntity(Loc.GetString("cult-rending-target-alive"), rune, args.User);
            args.Cancel();
            return;
        }

        if (rune.Comp.CurrentDoAfter.HasValue)
        {
            _popup.PopupEntity(Loc.GetString("cult-rending-already-summoning"), rune, args.User);
            args.Cancel();
            return;
        }

        var ev = new RendingRuneDoAfter();
        var argsDoAfterEvent = new DoAfterArgs(EntityManager, args.User, rune.Comp.SummonTime, ev, rune)
        {
            BreakOnMove = true
        };

        if (!_doAfter.TryStartDoAfter(argsDoAfterEvent, out rune.Comp.CurrentDoAfter))
        {
            args.Cancel();
            return;
        }

        var message = Loc.GetString(
            "cult-rending-started",
            ("location", FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString(rune.Owner))));
        _chat.DispatchGlobalAnnouncement(
            message,
            Loc.GetString("blood-cult-title"),
            false,
            colorOverride: Color.DarkRed);

        if (_station.GetOwningStation(rune.Owner) is { } station)
            _alertLevel.SetLevel(station, "gamma", true, true, true);

        _appearance.SetData(rune, RendingRuneVisuals.Active, true);
        rune.Comp.AudioEntity =
            _audio.PlayGlobal(rune.Comp.SummonAudio, Filter.Broadcast(), false, AudioParams.Default.WithLoop(true));
    }

    private void SpawnNarSie(Entity<CultRuneRendingComponent> rune, ref RendingRuneDoAfter args)
    {
        rune.Comp.CurrentDoAfter = null;
        _audio.Stop(rune.Comp.AudioEntity);
        _appearance.SetData(rune, RendingRuneVisuals.Active, false);
        if (args.Cancelled)
        {
            _chat.DispatchGlobalAnnouncement(
                Loc.GetString("cult-rending-prevented"),
                Loc.GetString("blood-cult-title"),
                false,
                colorOverride: Color.DarkRed);
            return;
        }

        var ev = new BloodCultNarsieSummoned();
        RaiseLocalEvent(ev);
        Spawn(rune.Comp.NarsiePrototype, _transform.GetMapCoordinates(rune));
    }
}
