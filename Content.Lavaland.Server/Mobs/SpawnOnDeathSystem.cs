// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Megafauna.Harvesting;
using Content.Lavaland.Common.Weapons.Ranged;
using Content.Lavaland.Server.Weapons;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityTable;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Timing;

// ReSharper disable EnforceForeachStatementBraces
// ReSharper disable EnforceIfStatementBraces
namespace Content.Lavaland.Server.Mobs;

public sealed partial class SpawnOnDeathSystem : EntitySystem
{
    [Dependency] private EntityTableSystem _entityTable = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MobThresholdSystem _mobThreshold = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SpawnLootOnDeathComponent, AttackedEvent>(OnDropAttacked);
        SubscribeLocalEvent<SpawnLootOnDeathComponent, DamageChangedEvent>(OnDamageChanged,
            before: [typeof(MobThresholdSystem)]);
        SubscribeLocalEvent<SpawnLootOnDeathComponent, MobStateChangedEvent>(OnDropKilled);
        SubscribeLocalEvent<MegafaunaWeaponLooterComponent, GunShotEvent>(OnQualifyingGunShot);
        SubscribeLocalEvent<MegafaunaWeaponLooterComponent, GunShotProjectileEvent>(OnQualifyingProjectileShot);
        SubscribeLocalEvent<MegafaunaWeaponLooterProjectileComponent, ProjectileHitEvent>(OnQualifyingProjectileHit);
    }

    private void OnDropAttacked(EntityUid uid, SpawnLootOnDeathComponent comp, ref AttackedEvent args)
    {
        if (_mobState.IsDead(uid))
            return;

        TryMarkQualifyingWeapon((uid, comp), args.User, args.Used);
    }

    private void OnQualifyingGunShot(Entity<MegafaunaWeaponLooterComponent> ent, ref GunShotEvent args)
    {
        foreach (var (projectile, _) in args.Ammo)
        {
            if (projectile is not { } uid || !HasComp<ProjectileComponent>(uid))
                continue;

            EnsureComp<MegafaunaWeaponLooterProjectileComponent>(uid).SourceWeapon = ent.Owner;
        }
    }

    private void OnQualifyingProjectileShot(Entity<MegafaunaWeaponLooterComponent> ent, ref GunShotProjectileEvent args)
    {
        EnsureComp<MegafaunaWeaponLooterProjectileComponent>(args.FiredProjectile).SourceWeapon = ent.Owner;
    }

    private void OnQualifyingProjectileHit(
        Entity<MegafaunaWeaponLooterProjectileComponent> ent,
        ref ProjectileHitEvent args)
    {
        if (args.Shooter is not { } shooter ||
            !TryComp<SpawnLootOnDeathComponent>(args.Target, out var loot) ||
            _mobState.IsDead(args.Target))
        {
            return;
        }

        // The marker is only attached by a MegafaunaWeaponLooter weapon. Checking the
        // source again also respects any boss-specific whitelist configuration.
        TryMarkQualifyingWeapon((args.Target, loot), shooter, ent.Comp.SourceWeapon);
    }

    /// <summary>
    /// Opens a one-damage-event accounting window when an encounter entity is
    /// struck with a qualifying portable kinetic weapon. Multi-entity bosses
    /// relay their descendants through this API to the root reward owner.
    /// </summary>
    public bool TryMarkQualifyingWeapon(
        Entity<SpawnLootOnDeathComponent> target,
        EntityUid origin,
        EntityUid weapon)
    {
        if (!_whitelist.IsWhitelistPassOrNull(target.Comp.SpecialWeaponWhitelist, weapon))
            return false;

        MarkQualifyingOrigin(target, origin);
        return true;
    }

    private void MarkQualifyingOrigin(Entity<SpawnLootOnDeathComponent> target, EntityUid origin)
    {
        target.Comp.PendingQualifyingOrigins.Add(origin);

        // AttackedEvent/ProjectileHitEvent is immediately followed by the damage call.
        // Expire an unused marker after that synchronous operation so a blocked hit
        // can never make a later unrelated attack count as kinetic contribution.
        Timer.Spawn(TimeSpan.Zero, () =>
        {
            if (Exists(target))
                target.Comp.PendingQualifyingOrigins.Remove(origin);
        });
    }

    private void OnDamageChanged(Entity<SpawnLootOnDeathComponent> ent, ref DamageChangedEvent args)
        => AccumulateQualifyingDamage(ent, args);

    /// <summary>
    /// Adds positive, post-resistance damage to an encounter's shared kinetic
    /// contribution pool. Non-qualifying damage is intentionally ignored.
    /// </summary>
    public void AccumulateQualifyingDamage(
        Entity<SpawnLootOnDeathComponent> ent,
        DamageChangedEvent args)
    {
        if (!args.DamageIncreased ||
            args.DamageDelta is not { } delta ||
            args.Origin is not { } origin ||
            !ent.Comp.PendingQualifyingOrigins.Remove(origin))
        {
            return;
        }

        foreach (var amount in delta.DamageDict.Values)
        {
            if (amount > 0)
                ent.Comp.QualifiedDamage += amount;
        }
    }

    private void OnDropKilled(EntityUid uid, SpawnLootOnDeathComponent comp, ref MobStateChangedEvent args)
    {
        if (!_mobState.IsDead(uid))
            return;

        // A harvestable dead boss is the carcass. It is deleted after its final stage.
        if (comp.DeleteOnDeath && !HasComp<MegafaunaHarvestableComponent>(uid))
            QueueDel(uid);

        if (comp.DropOnDeath)
            TryDropLoot((uid, comp));
    }

    /// <summary>
    /// Resolves an entity's normal and special loot tables exactly once. This is
    /// public so multi-phase encounters can award their loot at their real end.
    /// </summary>
    public bool TryDropLoot(Entity<SpawnLootOnDeathComponent> ent)
    {
        if (ent.Comp.HasDropped || Deleted(ent))
            return false;

        ent.Comp.HasDropped = true;
        var coords = Transform(ent).Coordinates;

        ent.Comp.DoSpecialLoot = IsSpecialLootQualified(ent);

        var droppedSpecial = false;
        if (ent.Comp.DoSpecialLoot && ent.Comp.SpecialTable != null)
        {
            var specialLoot = _entityTable.GetSpawns(ent.Comp.SpecialTable);
            foreach (var item in specialLoot)
                Spawn(item, coords);

            droppedSpecial = true;
        }

        if (ent.Comp.Table == null)
            return true;

        var loot = _entityTable.GetSpawns(ent.Comp.Table);
        if (droppedSpecial)
        {
            if (ent.Comp.DropBoth)
                foreach (var item in loot)
                    Spawn(item, coords);
        }
        else
            foreach (var item in loot)
                Spawn(item, coords);

        return true;
    }

    private bool IsSpecialLootQualified(Entity<SpawnLootOnDeathComponent> ent)
    {
        if (ent.Comp.SpecialTable == null)
            return false;

        // A special table without a weapon requirement remains unconditional.
        if (ent.Comp.SpecialWeaponWhitelist == null)
            return true;

        if (!_mobThreshold.TryGetThresholdForState(ent, MobState.Dead, out var deathThreshold) ||
            deathThreshold <= 0)
        {
            return false;
        }

        return ent.Comp.QualifiedDamage >= deathThreshold * ent.Comp.SpecialDamageFraction;
    }
}
