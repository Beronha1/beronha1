// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Lavaland.Shared.BossRewards;
using Content.Server.Radiation.Components;
using Content.Shared.Body;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.BossRewards;

/// <summary>
/// Native ECS implementation of the two utility relic concepts introduced by
/// Goobstation's Spider of Mercury PR. No upstream C# is copied.
/// </summary>
public sealed partial class MercuryRelicSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MercuryEtherDrinkerComponent, UseInHandEvent>(OnEtherDrinkerUse);
        SubscribeLocalEvent<MercuryEtherDrinkerComponent, ExaminedEvent>(OnEtherDrinkerExamined);
        SubscribeLocalEvent<MercuryParadoxCancellerComponent, UseInHandEvent>(OnParadoxUse);
        SubscribeLocalEvent<MercuryParadoxCancellerComponent, EntityTerminatingEvent>(OnParadoxTerminating);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        RechargeEtherDrinkers(frameTime);
        ProcessParadoxRewinds();
    }

    private void RechargeEtherDrinkers(float frameTime)
    {
        var query = EntityQueryEnumerator<MercuryEtherDrinkerComponent, RadiationReceiverComponent, UseDelayComponent>();
        while (query.MoveNext(out var uid, out var relic, out var radiation, out var delay))
        {
            if (radiation.CurrentRadiation <= 0f ||
                !_useDelay.TryGetDelayInfo((uid, delay), out var info) ||
                info.EndTime <= _timing.CurTime)
            {
                continue;
            }

            var reduction = TimeSpan.FromSeconds(
                radiation.CurrentRadiation * relic.RadiationRechargeMultiplier * frameTime);
            info.EndTime = info.EndTime - reduction < _timing.CurTime
                ? _timing.CurTime
                : info.EndTime - reduction;
            Dirty(uid, delay);
        }
    }

    private void OnEtherDrinkerExamined(Entity<MercuryEtherDrinkerComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<UseDelayComponent>(ent, out var delay) ||
            !_useDelay.TryGetDelayInfo((ent.Owner, delay), out var info))
        {
            return;
        }

        args.PushMarkup(Loc.GetString("mercury-ether-drinker-charge",
            ("charge", (int) MathF.Round(GetCharge(info) * 100f))));
    }

    private void OnEtherDrinkerUse(Entity<MercuryEtherDrinkerComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled ||
            !TryComp<UseDelayComponent>(ent, out var delay) ||
            !_useDelay.TryGetDelayInfo((ent.Owner, delay), out var info))
        {
            return;
        }

        var charge = GetCharge(info);
        var strikes = Math.Min(ent.Comp.MaxStrikes,
            (int) MathF.Floor(charge * 100f / ent.Comp.ChargePerStrike));
        if (strikes <= 0)
        {
            _popup.PopupEntity(Loc.GetString("mercury-ether-drinker-empty"), ent, args.User);
            args.Handled = true;
            return;
        }

        // A completely charged capacitor enters resonance and doubles its discharge.
        if (charge >= 0.999f)
            strikes *= 2;

        var origin = _transform.GetMapCoordinates(args.User);
        for (var i = 0; i < strikes; i++)
        {
            var offset = new Vector2(
                _random.NextFloat(-ent.Comp.StrikeOffset, ent.Comp.StrikeOffset),
                _random.NextFloat(-ent.Comp.StrikeOffset, ent.Comp.StrikeOffset));
            Spawn(ent.Comp.LightningPrototype, origin.Offset(offset));
        }

        _audio.PlayPvs(ent.Comp.DischargeSound, args.User);
        _popup.PopupEntity(Loc.GetString("mercury-ether-drinker-discharged"), ent, args.User, PopupType.Medium);
        _useDelay.ResetAllDelays((ent.Owner, delay));
        args.Handled = true;
    }

    private float GetCharge(UseDelayInfo info)
    {
        if (info.Length <= TimeSpan.Zero || info.EndTime <= _timing.CurTime)
            return 1f;

        var remaining = (float) (info.EndTime - _timing.CurTime).TotalSeconds;
        return MathHelper.Clamp01(1f - remaining / (float) info.Length.TotalSeconds);
    }

    private void OnParadoxUse(Entity<MercuryParadoxCancellerComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || ent.Comp.RewindAt != null ||
            !TryComp<DamageableComponent>(args.User, out var damageable) ||
            !TryComp<UseDelayComponent>(ent, out var delay) ||
            !_useDelay.TryResetDelay((ent.Owner, delay), checkDelayed: true))
        {
            return;
        }

        ent.Comp.User = args.User;
        ent.Comp.SavedCoordinates = Transform(args.User).Coordinates;
        ent.Comp.SavedDamage = _damageable.GetAllDamage((args.User, damageable));
        ent.Comp.RewindAt = _timing.CurTime + ent.Comp.RewindTime;
        ent.Comp.Marker = Spawn(ent.Comp.MarkerPrototype, _transform.GetMapCoordinates(args.User));

        _audio.PlayPvs(ent.Comp.StartSound, args.User);
        _popup.PopupEntity(Loc.GetString("mercury-paradox-canceller-armed"), ent, args.User, PopupType.Medium);
        args.Handled = true;
    }

    private void ProcessParadoxRewinds()
    {
        var query = EntityQueryEnumerator<MercuryParadoxCancellerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.RewindAt is not { } rewindAt || _timing.CurTime < rewindAt)
                continue;

            Rewind((uid, comp));
        }
    }

    private void Rewind(Entity<MercuryParadoxCancellerComponent> ent)
    {
        var user = ent.Comp.User;
        var coordinates = ent.Comp.SavedCoordinates;
        var savedDamage = ent.Comp.SavedDamage;
        ClearSnapshot(ent);

        if (user is not { } target ||
            !Exists(target) ||
            Transform(ent).ParentUid != target ||
            coordinates is not { } destination ||
            !destination.IsValid(EntityManager) ||
            savedDamage == null ||
            !TryComp<DamageableComponent>(target, out var damageable))
        {
            return;
        }

        _transform.SetCoordinates(target, destination);

        var currentDamage = _damageable.GetAllDamage((target, damageable));
        var delta = new DamageSpecifier();
        foreach (var (type, saved) in savedDamage.DamageDict)
        {
            var current = currentDamage.DamageDict.TryGetValue(type, out var value)
                ? value
                : FixedPoint2.Zero;
            if (saved != current)
                delta.DamageDict[type] = saved - current;
        }

        foreach (var (type, current) in currentDamage.DamageDict)
        {
            if (!savedDamage.DamageDict.ContainsKey(type) && current != FixedPoint2.Zero)
                delta.DamageDict[type] = -current;
        }

        if (delta.DamageDict.Count > 0)
        {
            if (HasComp<BodyComponent>(target))
            {
                _damageable.TryChangeDamage(
                    target,
                    delta,
                    ignoreResistances: true,
                    origin: ent.Owner,
                    ignoreBlockers: true,
                    canMiss: false);
            }
            else
            {
                _damageable.SetDamage((target, damageable), savedDamage);
            }
        }

        _audio.PlayPvs(ent.Comp.RewindSound, target);
        _popup.PopupEntity(Loc.GetString("mercury-paradox-canceller-rewound"), ent, target, PopupType.LargeCaution);
    }

    private void OnParadoxTerminating(Entity<MercuryParadoxCancellerComponent> ent, ref EntityTerminatingEvent args)
    {
        ClearSnapshot(ent);
    }

    private void ClearSnapshot(Entity<MercuryParadoxCancellerComponent> ent)
    {
        if (ent.Comp.Marker is { } marker && Exists(marker))
            QueueDel(marker);

        ent.Comp.Marker = null;
        ent.Comp.User = null;
        ent.Comp.SavedCoordinates = null;
        ent.Comp.SavedDamage = null;
        ent.Comp.RewindAt = null;
    }
}
