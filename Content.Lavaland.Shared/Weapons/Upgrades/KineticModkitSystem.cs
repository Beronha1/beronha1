// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Lavaland.Common.Weapons.Ranged;
using Content.Lavaland.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Drone;
using Content.Shared.Gatherable;
using Content.Shared.Gatherable.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Trauma.Common.Weapons.Ranged;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Lavaland.Shared.Weapons.Upgrades;

/// <summary>
/// Native ECS implementation of the reusable PKA modkits found in the SS13
/// mining ecosystem. Entity-producing effects are bounded and projectile
/// callbacks are split into prepare, impact and post-impact phases.
/// </summary>
public sealed partial class KineticModkitSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private GatherableSystem _gather = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MovementModStatusSystem _movementMod = default!;
    [Dependency] private TagSystem _tags = default!;

    private static readonly ProtoId<DamageTypePrototype> Blunt = "Blunt";
    private static readonly ProtoId<TagPrototype> Bot = "Bot";
    private static readonly ProtoId<TagPrototype> Pickaxe = "Pickaxe";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KineticMiningAreaProjectileComponent, ProjectileHitEvent>(OnMiningAreaHit);
        SubscribeLocalEvent<KineticMobPenetrationProjectileComponent, BeforeProjectileHitEvent>(OnMobPenetration);
        SubscribeLocalEvent<KineticHumanPassthroughProjectileComponent, PreventCollideEvent>(OnHumanPassthrough);
        SubscribeLocalEvent<KineticDronePassthroughProjectileComponent, PreventCollideEvent>(OnDronePassthrough);

        SubscribeLocalEvent<KineticRapidRepeaterUpgradeComponent, RechargeBasicEntityAmmoGetCooldownModifiersEvent>(OnRapidCooldown);
        SubscribeLocalEvent<KineticRapidRepeaterUpgradeComponent, GunShotProjectileEvent>(OnRapidShot);
        SubscribeLocalEvent<KineticRapidRepeaterProjectileComponent, ProjectileHitEvent>(OnRapidHit);

        SubscribeLocalEvent<KineticResonatorUpgradeComponent, GunShotProjectileEvent>(OnResonatorShot);
        SubscribeLocalEvent<KineticResonatorProjectileComponent, ProjectileHitEvent>(OnResonatorHit);

        SubscribeLocalEvent<KineticDeathSyphonUpgradeComponent, GunShotProjectileEvent>(OnSyphonShot);
        SubscribeLocalEvent<KineticDeathSyphonProjectileComponent, BeforeProjectileHitEvent>(OnSyphonBeforeHit);
        SubscribeLocalEvent<KineticDeathSyphonProjectileComponent, ProjectileHitEvent>(OnSyphonHit);
        SubscribeLocalEvent<KineticDeathSyphonMarkComponent, MobStateChangedEvent>(OnSyphonTargetStateChanged);
    }

    private void OnMobPenetration(Entity<KineticMobPenetrationProjectileComponent> ent,
        ref BeforeProjectileHitEvent args)
    {
        if (!TryComp<ProjectileComponent>(ent, out var projectile))
            return;

        projectile.Penetrate = HasComp<MobStateComponent>(args.Target);
        Dirty(ent, projectile);
    }

    private void OnMiningAreaHit(Entity<KineticMiningAreaProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (!_net.IsServer)
            return;

        var origin = Transform(ent).Coordinates;
        foreach (var gatherable in _lookup.GetEntitiesInRange<GatherableComponent>(origin, ent.Comp.Radius))
        {
            if (Deleted(gatherable))
                continue;
            _gather.Gather(gatherable.AsNullable(), args.Shooter);
        }
    }

    private void OnHumanPassthrough(Entity<KineticHumanPassthroughProjectileComponent> ent, ref PreventCollideEvent args)
    {
        if (HasComp<HumanoidProfileComponent>(args.OtherEntity))
            args.Cancelled = true;
    }

    private void OnDronePassthrough(Entity<KineticDronePassthroughProjectileComponent> ent, ref PreventCollideEvent args)
    {
        // SPLURT's module protects mining support mobs. Whiskey has both DroneComponent
        // drones and the silicon minebot, which is identified by its Bot + Pickaxe tags.
        if (HasComp<DroneComponent>(args.OtherEntity) ||
            (_tags.HasTag(args.OtherEntity, Bot) && _tags.HasTag(args.OtherEntity, Pickaxe)))
        {
            args.Cancelled = true;
        }
    }

    private void OnRapidCooldown(Entity<KineticRapidRepeaterUpgradeComponent> ent,
        ref RechargeBasicEntityAmmoGetCooldownModifiersEvent args)
        => args.Multiplier *= ent.Comp.MissCooldownMultiplier;

    private void OnRapidShot(Entity<KineticRapidRepeaterUpgradeComponent> ent, ref GunShotProjectileEvent args)
    {
        var projectile = EnsureComp<KineticRapidRepeaterProjectileComponent>(args.FiredProjectile);
        projectile.SourceUpgrade = ent;
        Dirty(args.FiredProjectile, projectile);
    }

    private void OnRapidHit(Entity<KineticRapidRepeaterProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (!_net.IsServer ||
            !TryComp<KineticRapidRepeaterUpgradeComponent>(ent.Comp.SourceUpgrade, out var upgrade) ||
            !TryComp<ProjectileComponent>(ent, out var projectile) ||
            projectile.Weapon is not { } weapon ||
            !TryComp<RechargeBasicEntityAmmoComponent>(weapon, out var recharge) ||
            (!HasComp<MobStateComponent>(args.Target) && !HasComp<GatherableComponent>(args.Target)))
        {
            return;
        }

        var hitCooldown = recharge.RechargeCooldown *
                          upgrade.MissCooldownMultiplier *
                          upgrade.HitCooldownFraction;
        recharge.NextCharge = _timing.CurTime + TimeSpan.FromSeconds(hitCooldown);
        Dirty(weapon, recharge);
    }

    private void OnResonatorShot(Entity<KineticResonatorUpgradeComponent> ent, ref GunShotProjectileEvent args)
    {
        var projectile = EnsureComp<KineticResonatorProjectileComponent>(args.FiredProjectile);
        projectile.SourceUpgrade = ent;
        Dirty(args.FiredProjectile, projectile);
    }

    private void OnResonatorHit(Entity<KineticResonatorProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (!_net.IsServer ||
            args.Shooter is not { } shooter ||
            HasComp<GatherableComponent>(args.Target) ||
            !TryComp<KineticResonatorUpgradeComponent>(ent.Comp.SourceUpgrade, out var upgrade))
        {
            return;
        }

        var coordinates = Transform(args.Target).Coordinates;
        foreach (var field in _lookup.GetEntitiesInRange<KineticResonanceFieldComponent>(
                     coordinates,
                     upgrade.TriggerRadius))
        {
            if (field.Comp.SourceUpgrade != ent.Comp.SourceUpgrade || field.Comp.Shooter != shooter)
                continue;

            foreach (var target in _lookup.GetEntitiesInRange<DamageableComponent>(coordinates, upgrade.DamageRadius))
            {
                if (target.Owner == shooter)
                    continue;
                _damage.TryChangeDamage(target.Owner, upgrade.BurstDamage, origin: shooter);
                if (HasComp<MobStateComponent>(target.Owner))
                {
                    _movementMod.TryUpdateMovementSpeedModDuration(
                        target.Owner,
                        upgrade.SlowdownEffect,
                        upgrade.SlowdownDuration,
                        upgrade.SlowdownModifier);
                }
            }
            QueueDel(field);
            return;
        }

        var spawned = Spawn(upgrade.FieldPrototype, coordinates);
        var resonance = EnsureComp<KineticResonanceFieldComponent>(spawned);
        resonance.SourceUpgrade = ent.Comp.SourceUpgrade;
        resonance.Shooter = shooter;
    }

    private void OnSyphonShot(Entity<KineticDeathSyphonUpgradeComponent> ent, ref GunShotProjectileEvent args)
    {
        var projectile = EnsureComp<KineticDeathSyphonProjectileComponent>(args.FiredProjectile);
        projectile.SourceUpgrade = ent;
    }

    private void OnSyphonBeforeHit(Entity<KineticDeathSyphonProjectileComponent> ent, ref BeforeProjectileHitEvent args)
    {
        if (!TryComp<KineticDeathSyphonUpgradeComponent>(ent.Comp.SourceUpgrade, out var upgrade) ||
            MetaData(args.Target).EntityPrototype?.ID is not { } targetPrototype ||
            !upgrade.Bounties.TryGetValue(new EntProtoId(targetPrototype), out var bonus) ||
            bonus <= 0f)
        {
            return;
        }

        args.Damage.DamageDict.TryGetValue(Blunt, out var current);
        args.Damage.DamageDict[Blunt] = current + bonus;
    }

    private void OnSyphonHit(Entity<KineticDeathSyphonProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (!_net.IsServer ||
            !HasComp<MobStateComponent>(args.Target) ||
            !TryComp<KineticDeathSyphonUpgradeComponent>(ent.Comp.SourceUpgrade, out _))
        {
            return;
        }

        EnsureComp<KineticDeathSyphonMarkComponent>(args.Target).Sources.Add(ent.Comp.SourceUpgrade);
    }

    private void OnSyphonTargetStateChanged(Entity<KineticDeathSyphonMarkComponent> ent, ref MobStateChangedEvent args)
    {
        if (!_net.IsServer || args.NewMobState != MobState.Dead ||
            MetaData(ent).EntityPrototype?.ID is not { } targetPrototype)
        {
            return;
        }

        var targetId = new EntProtoId(targetPrototype);
        foreach (var source in ent.Comp.Sources.ToArray())
        {
            if (!TryComp<KineticDeathSyphonUpgradeComponent>(source, out var upgrade))
                continue;

            upgrade.Bounties.TryGetValue(targetId, out var current);
            var gain = HasComp<BossMusicComponent>(ent) ? upgrade.MegafaunaBounty : upgrade.NormalBounty;
            upgrade.Bounties[targetId] = Math.Min(upgrade.MaximumBounty, current + gain);
            Dirty(source, upgrade);
        }

        RemCompDeferred<KineticDeathSyphonMarkComponent>(ent);
    }
}
