// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using System.Linq;
using Content.Lavaland.Common.Mobs;
using Content.Lavaland.Common.Weapons.Marker;
using Content.Lavaland.Shared.Audio;
using Content.Lavaland.Shared.Megafauna.Harvesting;
using Content.Lavaland.Shared.Megafauna.Utility;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Marker;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Network;

namespace Content.Lavaland.Shared.Weapons.Upgrades;

public sealed partial class CrusherUpgradeEffectsSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MobThresholdSystem _threshold = default!;
    [Dependency] private NpcFactionSystem _npcFaction = default!;
    [Dependency] private MegafaunaHeatProtectionSystem _heatProtection = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IGameTiming _timing = default!;

    private static readonly ProtoId<TagPrototype> SlowImmune = "SlowImmune";
    private static readonly ProtoId<TagPrototype> StunImmune = "StunImmune";
    private static readonly ProtoId<NpcFactionPrototype> PetsNt = "PetsNT";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrusherLegionSkullUpgradeComponent, GunRefreshModifiersEvent>(OnLegionRefresh);
        SubscribeLocalEvent<CrusherLegionSkullUpgradeComponent, AfterMarkerAttackedEvent>(OnLegionMarker);
        SubscribeLocalEvent<CrusherGoliathTentacleUpgradeComponent, MarkerAttackAttemptEvent>(OnGoliathMarker);
        SubscribeLocalEvent<CrusherGoliathTentacleUpgradeComponent, MeleeHitEvent>(OnGoliathMelee);
        SubscribeLocalEvent<CrusherAncientGoliathTentacleUpgradeComponent, MarkerAttackAttemptEvent>(OnAncientGoliathMarker);
        SubscribeLocalEvent<CrusherAncientGoliathTentacleUpgradeComponent, MeleeHitEvent>(OnAncientGoliathMelee);
        SubscribeLocalEvent<CrusherWatcherWingUpgradeComponent, GunShotEvent>(OnWatcherShot);
        SubscribeLocalEvent<CrusherMagmaWingUpgradeComponent, AfterMarkerAttackedEvent>(OnMagmaMarker);
        SubscribeLocalEvent<CrusherMagmaWingUpgradeComponent, GunShotEvent>(OnMagmaShot);
        SubscribeLocalEvent<CrusherPoisonFangUpgradeComponent, AfterMarkerAttackedEvent>(OnPoisonMarker);
        SubscribeLocalEvent<CrusherFrostGlandUpgradeComponent, GunShotEvent>(OnFrostShot);
        SubscribeLocalEvent<CrusherEyeBloodDrunkMinerUpgradeComponent, AfterMarkerAttackedEvent>(OnMinerMarker);
        SubscribeLocalEvent<CrusherIceBlockTalismanUpgradeComponent, AfterMarkerAttackedEvent>(OnIceBlockMarker);
        SubscribeLocalEvent<CrusherAshDrakeSpikeUpgradeComponent, AfterMarkerAttackedEvent>(OnDrakeMarker);
        SubscribeLocalEvent<CrusherDemonClawsUpgradeComponent, MarkerAttackAttemptEvent>(OnDemonMarker);
        SubscribeLocalEvent<CrusherDemonClawsUpgradeComponent, MeleeHitEvent>(OnDemonMelee);
        SubscribeLocalEvent<CrusherBlasterTubesUpgradeComponent, AfterMarkerAttackedEvent>(OnColossusMarker);
        SubscribeLocalEvent<CrusherBlasterTubesUpgradeComponent, GunRefreshModifiersEvent>(OnColossusRefresh);
        SubscribeLocalEvent<CrusherBlasterTubesUpgradeComponent, GunShotEvent>(OnColossusShot);

        SubscribeLocalEvent<IncreasedDamageComponent, BeforeDamageChangedEvent>(OnIncreasedDamage);
        SubscribeLocalEvent<DamageMarkerComponent, MeleeHitEvent>(OnWeakeningMelee);
        SubscribeLocalEvent<GunUpgradeAreaDamageComponent, GunShotEvent>(OnAreaDamageShot);
        SubscribeLocalEvent<ProjectileAreaDamageComponent, ProjectileHitEvent>(OnAreaDamageHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<IncreasedDamageComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime >= component.EndTime)
                RemCompDeferred<IncreasedDamageComponent>(uid);
        }
    }

    private void OnLegionRefresh(Entity<CrusherLegionSkullUpgradeComponent> ent, ref GunRefreshModifiersEvent args)
        => args.FireRate *= ent.Comp.FireRateCoefficient;

    private void OnLegionMarker(Entity<CrusherLegionSkullUpgradeComponent> ent, ref AfterMarkerAttackedEvent args)
    {
        if (!_net.IsServer ||
            _timing.CurTime < ent.Comp.NextRaise ||
            !HasComp<FaunaComponent>(args.Target) ||
            HasComp<BossMusicComponent>(args.Target) ||
            HasComp<MegafaunaHarvestableComponent>(args.Target) ||
            HasComp<LegionTrophyRaisedAllyComponent>(args.Target))
        {
            return;
        }

        var target = args.Target;
        var user = args.User;

        // AttackedEvent is raised immediately before the melee damage is committed.
        // Defer one task so only a marker-consuming killing blow can raise the fauna.
        Timer.Spawn(TimeSpan.Zero, () => TryRaiseLegionAlly(ent, user, target));
    }

    private void TryRaiseLegionAlly(
        Entity<CrusherLegionSkullUpgradeComponent> trophy,
        EntityUid user,
        EntityUid target)
    {
        if (!Exists(trophy) ||
            !Exists(user) ||
            !Exists(target) ||
            _timing.CurTime < trophy.Comp.NextRaise ||
            !_mobState.IsDead(target) ||
            HasComp<LegionTrophyRaisedAllyComponent>(target))
        {
            return;
        }

        if (trophy.Comp.ActiveAlly is { } current && Exists(current) && !_mobState.IsDead(current))
            return;

        _damage.ClearAllDamage(target);
        _mobState.ChangeMobState(target, MobState.Alive, origin: user);

        _npcFaction.ClearFactions(target, dirty: false);
        if (TryComp<NpcFactionMemberComponent>(user, out var userFaction) && userFaction.Factions.Count > 0)
            _npcFaction.AddFactions(target, new HashSet<ProtoId<NpcFactionPrototype>>(userFaction.Factions));
        else
            _npcFaction.AddFaction(target, PetsNt, dirty: false);

        // Protect the owner even when their faction is unusual or changes later.
        _npcFaction.IgnoreEntity(target, user);

        EnsureComp<LegionTrophyRaisedAllyComponent>(target).Master = user;
        trophy.Comp.ActiveAlly = target;
        trophy.Comp.NextRaise = _timing.CurTime + trophy.Comp.RaiseCooldown;
        Dirty(trophy);

        SpawnAttachedTo(trophy.Comp.RaiseEffect, Transform(target).Coordinates);
        _popup.PopupEntity(
            Loc.GetString("crusher-legion-trophy-raised", ("target", target)),
            target,
            user,
            PopupType.Medium);
    }

    private bool TryGetDamageFraction(EntityUid uid, MobState thresholdState, out float fraction)
    {
        fraction = 0f;
        if (!TryComp<DamageableComponent>(uid, out var damageable)
            || !_threshold.TryGetThresholdForState(uid, thresholdState, out var threshold)
            || threshold.Value <= 0)
            return false;

        fraction = Math.Clamp(_damage.GetTotalDamage((uid, damageable)).Float() / threshold.Value.Float(), 0f, 1f);
        return true;
    }

    private void OnGoliathMarker(Entity<CrusherGoliathTentacleUpgradeComponent> ent, ref MarkerAttackAttemptEvent args)
    {
        if (TryGetDamageFraction(args.User, ent.Comp.TargetState, out var fraction))
            args.DamageModifier += ent.Comp.MaxCoefficient * fraction;
    }

    private void OnGoliathMelee(Entity<CrusherGoliathTentacleUpgradeComponent> ent, ref MeleeHitEvent args)
    {
        if (TryGetDamageFraction(args.User, ent.Comp.TargetState, out var fraction))
            args.BonusDamage += args.BaseDamage * (ent.Comp.MaxCoefficient * fraction);
    }

    private bool IsAncientBonusTarget(EntityUid target, CrusherAncientGoliathTentacleUpgradeComponent component)
    {
        if (!TryComp<DamageableComponent>(target, out var damageable)
            || !_threshold.TryGetThresholdForState(target, MobState.Dead, out var threshold))
            return false;

        return _damage.GetTotalDamage((target, damageable)) <= threshold * (1f - component.HealthThreshold);
    }

    private void OnAncientGoliathMarker(Entity<CrusherAncientGoliathTentacleUpgradeComponent> ent, ref MarkerAttackAttemptEvent args)
    {
        if (IsAncientBonusTarget(args.Target, ent.Comp))
            args.DamageModifier += ent.Comp.Coefficient;
    }

    private void OnAncientGoliathMelee(Entity<CrusherAncientGoliathTentacleUpgradeComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Any(target => IsAncientBonusTarget(target, ent.Comp)))
            args.BonusDamage += args.BaseDamage * ent.Comp.Coefficient;
    }

    private void OnWatcherShot(Entity<CrusherWatcherWingUpgradeComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (ammo is not { } projectile || !HasComp<ProjectileComponent>(projectile))
                continue;

            EnsureComp<ProjectileTimerResetUpgradeComponent>(projectile).CooldownIncrease = ent.Comp.CooldownIncrease;
        }
    }

    private void OnMagmaMarker(Entity<CrusherMagmaWingUpgradeComponent> ent, ref AfterMarkerAttackedEvent args)
        => ent.Comp.Active = true;

    private void OnMagmaShot(Entity<CrusherMagmaWingUpgradeComponent> ent, ref GunShotEvent args)
        => ApplyNextShotDamage(ent.Comp.Damage, args.Ammo, ref ent.Comp.Active);

    private void OnPoisonMarker(Entity<CrusherPoisonFangUpgradeComponent> ent, ref AfterMarkerAttackedEvent args)
    {
        var component = EnsureComp<IncreasedDamageComponent>(args.Target);
        component.DamageModifier = ent.Comp.DamageModifier;
        component.EndTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.Duration);
    }

    private void OnFrostShot(Entity<CrusherFrostGlandUpgradeComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (ammo is not { } projectile || !TryComp<DamageMarkerOnCollideComponent>(projectile, out var marker))
                continue;

            marker.Weakening = true;
            marker.WeakeningModifier = ent.Comp.DamageModifier;
            Dirty(projectile, marker);
        }
    }

    private void OnMinerMarker(Entity<CrusherEyeBloodDrunkMinerUpgradeComponent> ent, ref AfterMarkerAttackedEvent args)
    {
        var addedStun = !_tag.HasTag(args.User, StunImmune) && _tag.TryAddTag(args.User, StunImmune);
        var addedSlow = !_tag.HasTag(args.User, SlowImmune) && _tag.TryAddTag(args.User, SlowImmune);

        var user = args.User;
        Timer.Spawn(TimeSpan.FromSeconds(ent.Comp.ImmunityDuration), () =>
        {
            if (!Exists(user))
                return;

            if (addedStun)
                _tag.RemoveTag(user, StunImmune);
            if (addedSlow)
                _tag.RemoveTag(user, SlowImmune);
        });
    }

    private void OnDrakeMarker(Entity<CrusherAshDrakeSpikeUpgradeComponent> ent, ref AfterMarkerAttackedEvent args)
    {
        if (!Exists(args.Target))
            return;

        var markedTarget = args.Target;
        var user = args.User;
        var targets = _lookup.GetEntitiesInRange<DamageableComponent>(
                Transform(markedTarget).Coordinates,
                ent.Comp.DamageRadius)
            .Where(target => target.Owner != markedTarget
                             && target.Owner != user
                             && HasComp<MobStateComponent>(target.Owner))
            .ToList();

        foreach (var target in targets)
        {
            _damage.TryChangeDamage(target.Owner, args.Damage * ent.Comp.DamageMultiplier, origin: user);

            var direction = (_transform.GetWorldPosition(target.Owner) - _transform.GetWorldPosition(markedTarget)).Normalized();
            direction = new Angle(_random.NextFloat(-0.2f, 0.2f)).RotateVec(direction);
            _throwing.TryThrow(target.Owner, direction);
        }

        var generation = _heatProtection.AddOrRefreshSource(user, ent);

        Timer.Spawn(TimeSpan.FromSeconds(ent.Comp.HeatImmunityDuration), () =>
        {
            if (!Exists(user))
                return;

            _heatProtection.RemoveSource(user, ent, generation);
        });
    }

    private void OnDemonMarker(Entity<CrusherDemonClawsUpgradeComponent> ent, ref MarkerAttackAttemptEvent args)
        => args.HealModifier += ent.Comp.DamageMultiplier * 4f;

    private void OnDemonMelee(Entity<CrusherDemonClawsUpgradeComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.HitEntities.Any(target => HasComp<MobStateComponent>(target) && !_mobState.IsDead(target)))
            return;

        args.BonusDamage += args.BaseDamage * ent.Comp.DamageMultiplier;
        if (!ent.Comp.MeleeHeal.Empty)
            _damage.TryChangeDamage(args.User, ent.Comp.MeleeHeal, true, false, origin: args.Weapon);
    }

    private void OnColossusMarker(Entity<CrusherBlasterTubesUpgradeComponent> ent, ref AfterMarkerAttackedEvent args)
        => ent.Comp.Active = true;

    private void OnColossusRefresh(Entity<CrusherBlasterTubesUpgradeComponent> ent, ref GunRefreshModifiersEvent args)
        => args.ProjectileSpeed *= ent.Comp.ProjectileSpeedCoefficient;

    private void OnColossusShot(Entity<CrusherBlasterTubesUpgradeComponent> ent, ref GunShotEvent args)
    {
        if (!ent.Comp.Active)
            return;

        ApplyNextShotDamage(ent.Comp.Damage, args.Ammo, ref ent.Comp.Active);
        foreach (var (uid, _) in args.Ammo)
        {
            if (uid is not { } projectile || !HasComp<ProjectileComponent>(projectile))
                continue;

            var area = EnsureComp<ProjectileAreaDamageComponent>(projectile);
            area.DamageRadius = ent.Comp.ShockwaveRadius;
            area.DamageMultiplier = ent.Comp.ShockwaveDamageMultiplier;
            break;
        }
    }

    private void OnIceBlockMarker(Entity<CrusherIceBlockTalismanUpgradeComponent> ent, ref AfterMarkerAttackedEvent args)
    {
        if (!Exists(args.Target) || _mobState.IsDead(args.Target))
            return;

        _stun.TryKnockdown(args.Target, ent.Comp.FreezeDuration, refresh: true, force: true);
        SpawnAttachedTo(ent.Comp.EffectPrototype, Transform(args.Target).Coordinates);
    }

    private void ApplyNextShotDamage(
        DamageSpecifier damage,
        List<(EntityUid? Uid, IShootable Shootable)> ammo,
        ref bool active)
    {
        if (!active)
            return;

        foreach (var (uid, _) in ammo)
        {
            if (uid is not { } projectile || !TryComp<ProjectileComponent>(projectile, out var projectileComponent))
                continue;

            projectileComponent.Damage += damage;
            Dirty(projectile, projectileComponent);
            active = false;
            break;
        }
    }

    private void OnIncreasedDamage(Entity<IncreasedDamageComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (args.Damage.GetTotal() > 0)
            args.Damage *= 1f + ent.Comp.DamageModifier;
    }

    private void OnWeakeningMelee(Entity<DamageMarkerComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.Weakening)
            args.BonusDamage -= args.BaseDamage * (1f - ent.Comp.WeakeningModifier);
    }

    private void OnAreaDamageShot(Entity<GunUpgradeAreaDamageComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (ammo is { } projectile && HasComp<ProjectileComponent>(projectile))
                EnsureComp<ProjectileAreaDamageComponent>(projectile);
        }
    }

    private void OnAreaDamageHit(Entity<ProjectileAreaDamageComponent> ent, ref ProjectileHitEvent args)
    {
        if (!Exists(args.Target))
            return;

        var targets = _lookup.GetEntitiesInRange<DamageableComponent>(
            Transform(args.Target).Coordinates,
            ent.Comp.DamageRadius);

        foreach (var target in targets)
        {
            if (target.Owner == args.Target || target.Owner == args.Shooter || target.Owner == ent.Owner)
                continue;

            _damage.TryChangeDamage(
                target.Owner,
                args.Damage * ent.Comp.DamageMultiplier,
                origin: args.Shooter);
        }
    }
}
