// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using System.Linq;
using System.Numerics;
using Content.Lavaland.Common.Mobs;
using Content.Lavaland.Common.Weapons.Marker;
using Content.Lavaland.Common.Weapons.Ranged;
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
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
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
    [Dependency] private SharedGunSystem _gun = default!;

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
        SubscribeLocalEvent<CrusherEyeBloodDrunkMinerUpgradeComponent, GunShotProjectileEvent>(OnMinerProjectile);
        SubscribeLocalEvent<CrusherIceBlockTalismanUpgradeComponent, AfterMarkerAttackedEvent>(OnIceBlockMarker);
        SubscribeLocalEvent<CrusherIceBlockTalismanUpgradeComponent, GunShotProjectileEvent>(OnIceProjectile);
        SubscribeLocalEvent<CrusherAshDrakeSpikeUpgradeComponent, AfterMarkerAttackedEvent>(OnDrakeMarker);
        SubscribeLocalEvent<CrusherAshDrakeSpikeUpgradeComponent, GunShotProjectileEvent>(OnDrakeProjectile);
        SubscribeLocalEvent<CrusherDemonClawsUpgradeComponent, MarkerAttackAttemptEvent>(OnDemonMarker);
        SubscribeLocalEvent<CrusherDemonClawsUpgradeComponent, MeleeHitEvent>(OnDemonMelee);
        SubscribeLocalEvent<CrusherDemonClawsUpgradeComponent, GunGetProjectileSpreadEvent>(OnDemonProjectileSpread);
        SubscribeLocalEvent<CrusherDemonClawsUpgradeComponent, GunShotProjectileEvent>(OnDemonProjectile);
        SubscribeLocalEvent<CrusherBlasterTubesUpgradeComponent, AfterMarkerAttackedEvent>(OnColossusMarker);
        SubscribeLocalEvent<CrusherBlasterTubesUpgradeComponent, GunRefreshModifiersEvent>(OnColossusRefresh);
        SubscribeLocalEvent<CrusherBlasterTubesUpgradeComponent, GunShotEvent>(OnColossusShot);
        SubscribeLocalEvent<CrusherBlasterTubesUpgradeComponent, GunShotProjectileEvent>(OnColossusProjectile);
        SubscribeLocalEvent<CrusherLegionSkullUpgradeComponent, GunShotProjectileEvent>(OnLegionProjectile);
        SubscribeLocalEvent<CrusherMercuryAlloyUpgradeComponent, GunShotProjectileEvent>(OnMercuryProjectile);
        SubscribeLocalEvent<CrusherOniHornUpgradeComponent, GunShotProjectileEvent>(OnOniProjectile);
        SubscribeLocalEvent<KineticTrophyProjectileComponent, PreventCollideEvent>(OnTrophyProjectilePreventCollide);
        SubscribeLocalEvent<KineticTrophyProjectileComponent, ProjectileHitEvent>(OnTrophyProjectileHit);

        SubscribeLocalEvent<IncreasedDamageComponent, BeforeDamageChangedEvent>(OnIncreasedDamage);
        SubscribeLocalEvent<DamageMarkerComponent, MeleeHitEvent>(OnWeakeningMelee);
        SubscribeLocalEvent<GunUpgradeAreaDamageComponent, GunShotProjectileEvent>(OnAreaDamageShot);
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

    private void OnMinerProjectile(Entity<CrusherEyeBloodDrunkMinerUpgradeComponent> ent, ref GunShotProjectileEvent args)
        => EnsureComp<KineticTrophyProjectileComponent>(args.FiredProjectile).BloodDrunkTrophy = ent;

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

    private void OnDrakeProjectile(Entity<CrusherAshDrakeSpikeUpgradeComponent> ent, ref GunShotProjectileEvent args)
    {
        var projectile = EnsureComp<KineticTrophyProjectileComponent>(args.FiredProjectile);
        projectile.AshDrakeTrophy = ent;
        Dirty(args.FiredProjectile, projectile);
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

    private void OnDemonProjectileSpread(Entity<CrusherDemonClawsUpgradeComponent> ent,
        ref GunGetProjectileSpreadEvent args)
    {
        if (!HasComp<ProjectileComponent>(args.Projectile) || args.Prototype == null)
            return;

        // SPLURT's shotgun-blast mod splits every existing bolt. This must be
        // multiplicative: a four-pellet kinetic shotgun becomes twelve pellets,
        // while an ordinary PKA becomes three.
        args.Count = Math.Clamp(
            args.Count * ent.Comp.ProjectileCount,
            1,
            ent.Comp.MaxProjectileCount);
        if (args.Spread.Theta < ent.Comp.ProjectileSpread.Theta)
            args.Spread = ent.Comp.ProjectileSpread;
    }

    private void OnDemonProjectile(Entity<CrusherDemonClawsUpgradeComponent> ent, ref GunShotProjectileEvent args)
    {
        var projectile = EnsureComp<KineticTrophyProjectileComponent>(args.FiredProjectile);
        projectile.DemonClawsTrophy = ent;
        Dirty(args.FiredProjectile, projectile);
    }

    private void OnColossusMarker(Entity<CrusherBlasterTubesUpgradeComponent> ent, ref AfterMarkerAttackedEvent args)
        => ent.Comp.Active = true;

    private void OnColossusRefresh(Entity<CrusherBlasterTubesUpgradeComponent> ent, ref GunRefreshModifiersEvent args)
        => args.ProjectileSpeed *= ent.Comp.ProjectileSpeedCoefficient;

    private void OnColossusShot(Entity<CrusherBlasterTubesUpgradeComponent> ent, ref GunShotEvent args)
    {
        if (!ent.Comp.Active)
            return;

        // GunShotProjectileEvent prepared every pellet belonging to this shot.
        ent.Comp.Active = false;
        Dirty(ent);
    }

    private void OnColossusProjectile(Entity<CrusherBlasterTubesUpgradeComponent> ent, ref GunShotProjectileEvent args)
    {
        var trophyProjectile = EnsureComp<KineticTrophyProjectileComponent>(args.FiredProjectile);
        trophyProjectile.ColossusTrophy = ent;
        Dirty(args.FiredProjectile, trophyProjectile);

        if (!ent.Comp.Active || !TryComp<ProjectileComponent>(args.FiredProjectile, out var projectile))
            return;

        projectile.Damage += ent.Comp.Damage;
        Dirty(args.FiredProjectile, projectile);
        var area = EnsureComp<ProjectileAreaDamageComponent>(args.FiredProjectile);
        area.DamageRadius = ent.Comp.ShockwaveRadius;
        area.DamageMultiplier = ent.Comp.ShockwaveDamageMultiplier;
    }

    private void OnLegionProjectile(Entity<CrusherLegionSkullUpgradeComponent> ent, ref GunShotProjectileEvent args)
    {
        var projectile = EnsureComp<KineticTrophyProjectileComponent>(args.FiredProjectile);
        projectile.LegionTrophy = ent;
        Dirty(args.FiredProjectile, projectile);
    }

    private void OnIceProjectile(Entity<CrusherIceBlockTalismanUpgradeComponent> ent, ref GunShotProjectileEvent args)
    {
        var projectile = EnsureComp<KineticTrophyProjectileComponent>(args.FiredProjectile);
        projectile.IceTalismanTrophy = ent;
        Dirty(args.FiredProjectile, projectile);
    }

    private void OnMercuryProjectile(Entity<CrusherMercuryAlloyUpgradeComponent> ent, ref GunShotProjectileEvent args)
    {
        var projectile = EnsureComp<KineticTrophyProjectileComponent>(args.FiredProjectile);
        projectile.MercuryTrophy = ent;
        Dirty(args.FiredProjectile, projectile);
    }

    private void OnOniProjectile(Entity<CrusherOniHornUpgradeComponent> ent, ref GunShotProjectileEvent args)
    {
        var projectile = EnsureComp<KineticTrophyProjectileComponent>(args.FiredProjectile);
        projectile.OniTrophy = ent;
        Dirty(args.FiredProjectile, projectile);
    }

    private void OnTrophyProjectileHit(Entity<KineticTrophyProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (!_net.IsServer ||
            args.Shooter is not { } shooter ||
            !Exists(shooter) ||
            !Exists(args.Target))
            return;

        // Stage 3: impact effects in stable boss order.
        if (ent.Comp.AshDrakeTrophy is { } drakeUid &&
            TryComp<CrusherAshDrakeSpikeUpgradeComponent>(drakeUid, out var drake))
        {
            ThrowAwayFrom(args.Target, shooter, drake.ProjectileKnockback);
            if (_timing.CurTime >= drake.NextProjectileExplosion)
            {
                drake.NextProjectileExplosion = _timing.CurTime + drake.ProjectileExplosionCooldown;
                ApplyAreaDamageAndThrow(args.Target,
                    shooter,
                    args.Damage * drake.DamageMultiplier,
                    drake.DamageRadius,
                    drake.ProjectileKnockback * 0.65f);
                Dirty(drakeUid, drake);
            }
        }

        if (ent.Comp.IceTalismanTrophy is { } iceUid &&
            TryComp<CrusherIceBlockTalismanUpgradeComponent>(iceUid, out var ice) &&
            HasComp<MobStateComponent>(args.Target) &&
            !_mobState.IsDead(args.Target))
        {
            CleanupIceTracking(ice);
            if (!ice.RangedCooldowns.ContainsKey(args.Target))
            {
                ice.RangedHits.TryGetValue(args.Target, out var hits);
                hits++;
                if (hits >= ice.RangedHitsRequired)
                {
                    ice.RangedHits.Remove(args.Target);
                    ice.RangedCooldowns[args.Target] = _timing.CurTime + ice.RangedCooldown;
                    _stun.TryKnockdown(args.Target, ice.FreezeDuration, refresh: true, force: true);
                    SpawnAttachedTo(ice.EffectPrototype, Transform(args.Target).Coordinates);
                }
                else
                    ice.RangedHits[args.Target] = hits;

                Dirty(iceUid, ice);
            }
        }

        if (ent.Comp.LegionTrophy is { } legionUid &&
            TryComp<CrusherLegionSkullUpgradeComponent>(legionUid, out var legion) &&
            HasComp<MobStateComponent>(args.Target) &&
            !_mobState.IsDead(args.Target))
        {
            legion.SkullHitProgress++;
            if (legion.SkullHitProgress >= legion.SkullHitsRequired &&
                _timing.CurTime >= legion.NextSkull &&
                (legion.ActiveSkull is not { } active || !Exists(active) || _mobState.IsDead(active)))
            {
                legion.SkullHitProgress = 0;
                legion.NextSkull = _timing.CurTime + legion.SkullCooldown;
                var skull = SpawnAttachedTo(legion.SkullPrototype, Transform(args.Target).Coordinates);
                legion.ActiveSkull = skull;
                ConfigureSkullFaction(skull, shooter);
            }
            Dirty(legionUid, legion);
        }

        if (!ent.Comp.Ricocheted &&
            ent.Comp.MercuryTrophy is { } mercuryUid &&
            TryComp<CrusherMercuryAlloyUpgradeComponent>(mercuryUid, out var mercury))
        {
            TryCreateMercuryRicochet(ent, args, shooter, mercury.RicochetRange);
        }

        if (ent.Comp.OniTrophy is { } oniUid &&
            TryComp<CrusherOniHornUpgradeComponent>(oniUid, out var oni))
        {
            ApplyAreaDamageAndThrow(args.Target, shooter, new DamageSpecifier(), oni.WaveRadius, oni.ThrowStrength);
        }

        // Stage 4: recovery and recharge after the impact effects above.
        if (HasComp<MobStateComponent>(args.Target) && !_mobState.IsDead(args.Target))
        {
            if (ent.Comp.DemonClawsTrophy is { } demonUid &&
                TryComp<CrusherDemonClawsUpgradeComponent>(demonUid, out var demon) &&
                !demon.RangedHeal.Empty)
            {
                _damage.TryChangeDamage(shooter, demon.RangedHeal, true, false, origin: demonUid);
            }

            if (ent.Comp.BloodDrunkTrophy is { } minerUid &&
                TryComp<CrusherEyeBloodDrunkMinerUpgradeComponent>(minerUid, out var miner) &&
                !miner.RangedHeal.Empty)
            {
                _damage.TryChangeDamage(shooter, miner.RangedHeal, true, false, origin: minerUid);
            }
        }

        if (ent.Comp.ColossusTrophy is { } colossusUid &&
            TryComp<CrusherBlasterTubesUpgradeComponent>(colossusUid, out var colossus) &&
            TryComp<ProjectileComponent>(ent, out var projectile) &&
            projectile.Weapon is { } weapon &&
            TryComp<RechargeBasicEntityAmmoComponent>(weapon, out var recharge))
        {
            var accelerated = _timing.CurTime + TimeSpan.FromSeconds(
                recharge.RechargeCooldown * colossus.RangedRechargeMultiplier);
            if (recharge.NextCharge == null || recharge.NextCharge > accelerated)
            {
                recharge.NextCharge = accelerated;
                Dirty(weapon, recharge);
            }
        }
    }

    /// <summary>
    /// Trophy impacts are resolved authoritatively by the server. Letting a predicted
    /// multi-pellet shot also collide with an entity that the server destroys in the
    /// same tick leaves stale client physics contacts during reconciliation. This is
    /// especially easy to trigger with Demon Claws plus the Ash Drake damage bonus.
    /// Movable targets remain predicted; only anchored/static structures defer their
    /// collision to the server.
    /// </summary>
    private void OnTrophyProjectilePreventCollide(
        Entity<KineticTrophyProjectileComponent> ent,
        ref PreventCollideEvent args)
    {
        if (!_net.IsClient || TerminatingOrDeleted(args.OtherEntity))
            return;

        if (Transform(args.OtherEntity).Anchored || !IsKnockbackTarget(args.OtherBody))
            args.Cancelled = true;
    }

    private void CleanupIceTracking(CrusherIceBlockTalismanUpgradeComponent component)
    {
        foreach (var (target, time) in component.RangedCooldowns.ToArray())
        {
            if (!Exists(target) || time <= _timing.CurTime)
                component.RangedCooldowns.Remove(target);
        }

        foreach (var target in component.RangedHits.Keys.ToArray())
        {
            if (!Exists(target))
                component.RangedHits.Remove(target);
        }
    }

    private void TryCreateMercuryRicochet(
        Entity<KineticTrophyProjectileComponent> source,
        ProjectileHitEvent hit,
        EntityUid shooter,
        float range)
    {
        if (!TryComp<ProjectileComponent>(source, out var projectile) ||
            projectile.Weapon is not { } weapon ||
            MetaData(source).EntityPrototype?.ID is not { } prototype)
        {
            return;
        }

        var origin = Transform(hit.Target).Coordinates;
        EntityUid? nearest = null;
        var nearestDistance = float.MaxValue;
        foreach (var candidate in _lookup.GetEntitiesInRange<DamageableComponent>(origin, range))
        {
            if (candidate.Owner == hit.Target ||
                candidate.Owner == shooter ||
                !HasComp<MobStateComponent>(candidate.Owner) ||
                _mobState.IsDead(candidate.Owner))
            {
                continue;
            }

            var distance = (_transform.GetWorldPosition(candidate.Owner) -
                            _transform.GetWorldPosition(hit.Target)).LengthSquared();
            if (distance >= nearestDistance)
                continue;

            nearest = candidate.Owner;
            nearestDistance = distance;
        }

        if (nearest is not { } target)
            return;

        var ricochet = Spawn(new EntProtoId(prototype), origin);
        var ricochetData = EnsureComp<KineticTrophyProjectileComponent>(ricochet);
        ricochetData.Ricocheted = true;
        var direction = (_transform.GetWorldPosition(target) - _transform.GetWorldPosition(hit.Target)).Normalized();
        _gun.ShootProjectile(ricochet, direction, Vector2.Zero, weapon, shooter);
    }

    private void ConfigureSkullFaction(EntityUid skull, EntityUid shooter)
    {
        _npcFaction.ClearFactions(skull, dirty: false);
        if (TryComp<NpcFactionMemberComponent>(shooter, out var userFaction) && userFaction.Factions.Count > 0)
            _npcFaction.AddFactions(skull, new HashSet<ProtoId<NpcFactionPrototype>>(userFaction.Factions));
        else
            _npcFaction.AddFaction(skull, PetsNt, dirty: false);

        _npcFaction.IgnoreEntity(skull, shooter);
    }

    private void ThrowAwayFrom(EntityUid target, EntityUid source, float strength)
    {
        if (TerminatingOrDeleted(target) ||
            TerminatingOrDeleted(source) ||
            !TryComp<PhysicsComponent>(target, out var physics) ||
            Transform(target).Anchored ||
            !IsKnockbackTarget(physics))
        {
            return;
        }

        var direction = _transform.GetWorldPosition(target) - _transform.GetWorldPosition(source);
        if (direction.LengthSquared() <= 0.001f)
            direction = _random.NextAngle().ToVec();
        _throwing.TryThrow(target, direction.Normalized(), strength);
    }

    private static bool IsKnockbackTarget(PhysicsComponent physics)
        => (physics.BodyType & (BodyType.Dynamic | BodyType.KinematicController)) != 0x0;

    private void ApplyAreaDamageAndThrow(
        EntityUid center,
        EntityUid shooter,
        DamageSpecifier damage,
        float radius,
        float throwStrength)
    {
        foreach (var target in _lookup.GetEntitiesInRange<DamageableComponent>(Transform(center).Coordinates, radius))
        {
            if (target.Owner == center || target.Owner == shooter)
                continue;

            if (!damage.Empty)
                _damage.TryChangeDamage(target.Owner, damage, origin: shooter);
            if (throwStrength > 0f)
                ThrowAwayFrom(target.Owner, center, throwStrength);
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

    private void OnAreaDamageShot(Entity<GunUpgradeAreaDamageComponent> ent, ref GunShotProjectileEvent args)
    {
        if (!HasComp<ProjectileComponent>(args.FiredProjectile))
            return;

        var area = EnsureComp<ProjectileAreaDamageComponent>(args.FiredProjectile);
        area.DamageRadius = ent.Comp.DamageRadius;
        area.DamageMultiplier = ent.Comp.DamageMultiplier;
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
