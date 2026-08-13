// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Artifacts;
using Content.Server.Administration.Logs;
using Content.Server.Stunnable;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Artifacts;

/// <summary>
/// Reimplements a bounded subset of /tg/ anomalous-crystal behaviours as native ECS effects.
/// </summary>
public sealed partial class AnomalousCrystalSystem : EntitySystem
{
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private StunSystem _stun = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnomalousCrystalComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<AnomalousCrystalWardComponent, BeforeDamageChangedEvent>(OnWardDamage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AnomalousCrystalWardComponent>();
        while (query.MoveNext(out var uid, out var ward))
        {
            if (_timing.CurTime >= ward.EndTime)
                RemCompDeferred<AnomalousCrystalWardComponent>(uid);
        }
    }

    private void OnUse(Entity<AnomalousCrystalComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled ||
            !TryComp(ent, out UseDelayComponent? delay) ||
            _useDelay.IsDelayed((ent, delay)))
        {
            return;
        }

        switch (ent.Comp.Mode)
        {
            case AnomalousCrystalMode.Ward:
                var ward = EnsureComp<AnomalousCrystalWardComponent>(args.User);
                ward.EndTime = _timing.CurTime + ent.Comp.EffectDuration;
                break;
            case AnomalousCrystalMode.HealingPulse:
                ApplyHealingPulse(ent, args.User);
                break;
            case AnomalousCrystalMode.Repulsion:
                ApplyRepulsion(ent, args.User);
                break;
            case AnomalousCrystalMode.Stasis:
                ApplyStasis(ent, args.User);
                break;
        }

        _useDelay.TryResetDelay((ent, delay));
        _audio.PlayPvs("/Audio/_Lavaland/Effects/invoke_general.ogg", ent);
        _popup.PopupEntity(
            Loc.GetString($"anomalous-crystal-activate-{ent.Comp.Mode.ToString().ToLowerInvariant()}"),
            args.User,
            args.User,
            PopupType.Medium);
        _adminLog.Add(
            LogType.Action,
            $"{ToPrettyString(args.User):player} activated {ToPrettyString(ent):item} in {ent.Comp.Mode} mode");
        args.Handled = true;
    }

    private void ApplyHealingPulse(Entity<AnomalousCrystalComponent> ent, EntityUid user)
    {
        if (ent.Comp.Healing.Empty)
            return;

        foreach (var target in _lookup.GetEntitiesInRange<DamageableComponent>(Transform(user).Coordinates, ent.Comp.Radius))
        {
            if (!HasComp<MobStateComponent>(target))
                continue;

            _damage.TryChangeDamage(target.Owner, ent.Comp.Healing, true, false, origin: ent);
        }
    }

    private void ApplyRepulsion(Entity<AnomalousCrystalComponent> ent, EntityUid user)
    {
        var origin = _transform.GetWorldPosition(user);
        foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(Transform(user).Coordinates, ent.Comp.Radius))
        {
            if (target.Owner == user)
                continue;

            var direction = (_transform.GetWorldPosition(target.Owner) - origin).Normalized();
            _throwing.TryThrow(target.Owner, direction);
        }
    }

    private void ApplyStasis(Entity<AnomalousCrystalComponent> ent, EntityUid user)
    {
        foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(Transform(user).Coordinates, ent.Comp.Radius))
        {
            if (target.Owner != user)
                _stun.TryKnockdown(target.Owner, ent.Comp.EffectDuration, true);
        }
    }

    private void OnWardDamage(Entity<AnomalousCrystalWardComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (args.Damage.GetTotal() > 0)
            args.Damage *= ent.Comp.DamageCoefficient;
    }
}
