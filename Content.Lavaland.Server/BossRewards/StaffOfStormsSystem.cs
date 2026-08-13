// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Lavaland.Shared.BossRewards;
using Content.Server.Administration.Logs;
using Content.Server.Weather;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.Weather;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.BossRewards;

/// <summary>
/// ECS reimplementation of the Legion Staff of Storms gameplay loop.
/// No BYOND/DM source is copied; only its documented behavior is used as a design reference.
/// </summary>
public sealed partial class StaffOfStormsSystem : EntitySystem
{
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private WeatherSystem _weather = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StaffOfStormsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StaffOfStormsComponent, BeforeRangedInteractEvent>(OnRangedInteract);
        SubscribeLocalEvent<StaffOfStormsComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<StaffOfStormsComponent, StormStaffDispelDoAfterEvent>(OnDispelComplete);
        SubscribeLocalEvent<StaffOfStormsComponent, ExaminedEvent>(OnExamine);
    }

    private void OnMapInit(Entity<StaffOfStormsComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Charges = ent.Comp.MaxCharges;
    }

    private void OnRangedInteract(Entity<StaffOfStormsComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (HasComp<PacifiedComponent>(args.User))
        {
            _popup.PopupClient(Loc.GetString("storm-staff-pacified"), ent, args.User);
            return;
        }

        var userCoordinates = _transform.GetMapCoordinates(args.User);
        var targetCoordinates = _transform.ToMapCoordinates(args.ClickLocation);
        if (userCoordinates.MapId != targetCoordinates.MapId ||
            !userCoordinates.InRange(targetCoordinates, ent.Comp.MaxRange))
        {
            _popup.PopupClient(Loc.GetString("storm-staff-out-of-range"), ent, args.User);
            return;
        }

        if (ent.Comp.Charges <= 0)
        {
            _popup.PopupClient(Loc.GetString("storm-staff-no-charges"), ent, args.User);
            return;
        }

        if (ent.Comp.PendingStrikes.Any(pending =>
                pending.Coordinates.MapId == targetCoordinates.MapId &&
                pending.Coordinates.InRange(targetCoordinates, 0.25f)))
        {
            _popup.PopupClient(Loc.GetString("storm-staff-already-targeted"), ent, args.User);
            return;
        }

        var boosted = HasActiveWeather(targetCoordinates.MapId);
        ent.Comp.Charges--;
        ent.Comp.RechargeAt.Add(_timing.CurTime + ent.Comp.RechargeTime);
        ent.Comp.PendingStrikes.Add(new StormStaffPendingStrike(
            targetCoordinates,
            _timing.CurTime + ent.Comp.StrikeDelay,
            args.User,
            boosted));

        Spawn(ent.Comp.TelegraphPrototype, targetCoordinates);
        _popup.PopupClient(
            Loc.GetString(boosted ? "storm-staff-targeted-boosted" : "storm-staff-targeted"),
            ent,
            args.User);
        _adminLog.Add(
            LogType.Action,
            LogImpact.Medium,
            $"{ToPrettyString(args.User):user} targeted {targetCoordinates} with {ToPrettyString(ent):tool} (weather boosted: {boosted}).");
    }

    private void OnUseInHand(Entity<StaffOfStormsComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var mapId = Transform(args.User).MapID;
        if (!HasActiveWeather(mapId))
        {
            _popup.PopupClient(Loc.GetString("storm-staff-no-weather"), ent, args.User);
            args.Handled = true;
            return;
        }

        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            ent.Comp.DispelTime,
            new StormStaffDispelDoAfterEvent(),
            ent,
            used: ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        _popup.PopupEntity(Loc.GetString("storm-staff-dispel-start"), args.User, args.User);
        args.Handled = true;
    }

    private void OnDispelComplete(Entity<StaffOfStormsComponent> ent, ref StormStaffDispelDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var mapId = Transform(args.User).MapID;
        if (!HasActiveWeather(mapId) || !_weather.TrySetWeather(mapId, null, out _))
            return;

        _popup.PopupEntity(Loc.GetString("storm-staff-dispel-complete"), args.User, args.User);
        _adminLog.Add(
            LogType.Action,
            LogImpact.Medium,
            $"{ToPrettyString(args.User):user} dispelled the weather with {ToPrettyString(ent):tool}.");
        args.Handled = true;
    }

    private void OnExamine(Entity<StaffOfStormsComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(
            "storm-staff-examine-charges",
            ("charges", ent.Comp.Charges),
            ("maximum", ent.Comp.MaxCharges)));
        args.PushMarkup(Loc.GetString("storm-staff-examine-usage"));
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<StaffOfStormsComponent>();
        while (query.MoveNext(out var uid, out var staff))
        {
            for (var i = staff.RechargeAt.Count - 1; i >= 0; i--)
            {
                if (staff.RechargeAt[i] > _timing.CurTime)
                    continue;

                staff.RechargeAt.RemoveAt(i);
                if (staff.Charges >= staff.MaxCharges)
                    continue;

                staff.Charges++;
                _audio.PlayPvs(staff.RechargeSound, uid);
            }

            for (var i = staff.PendingStrikes.Count - 1; i >= 0; i--)
            {
                var pending = staff.PendingStrikes[i];
                if (pending.StrikeAt > _timing.CurTime)
                    continue;

                staff.PendingStrikes.RemoveAt(i);
                Strike((uid, staff), pending);
            }
        }
    }

    private void Strike(Entity<StaffOfStormsComponent> staff, StormStaffPendingStrike pending)
    {
        Spawn(staff.Comp.StrikePrototype, pending.Coordinates);

        var radius = pending.WeatherBoosted
            ? staff.Comp.WeatherStrikeRadius
            : staff.Comp.StrikeRadius;
        var damage = pending.WeatherBoosted
            ? staff.Comp.Damage * staff.Comp.WeatherDamageMultiplier
            : staff.Comp.Damage;

        foreach (var target in _lookup.GetEntitiesInRange(pending.Coordinates, radius))
        {
            if (!HasComp<DamageableComponent>(target))
                continue;

            _damage.TryChangeDamage(target, damage, origin: pending.User);
        }
    }

    private bool HasActiveWeather(MapId mapId)
    {
        if (mapId == MapId.Nullspace || !_map.TryGetMap(mapId, out var mapUid))
            return false;

        var query = EntityQueryEnumerator<WeatherStatusEffectComponent, StatusEffectComponent>();
        while (query.MoveNext(out _, out _, out var status))
        {
            if (status.AppliedTo != mapUid || !status.Applied || status.StartEffectTime > _timing.CurTime)
                continue;

            if (status.EndEffectTime == null || status.EndEffectTime > _timing.CurTime)
                return true;
        }

        return false;
    }
}
