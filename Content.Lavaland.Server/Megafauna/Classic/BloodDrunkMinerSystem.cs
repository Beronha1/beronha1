// All modifications and additions under the Corvax-Wega tag and _Wega directories are licensed under GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using System.Numerics;
using Content.Lavaland.Server.NPC;
using Content.Lavaland.Shared.Megafauna.Components;
using Content.Lavaland.Shared.Artifacts;
using Content.Lavaland.Shared.Megafauna.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.SSDIndicator;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.Classic;

/// <summary>
/// Distance-driven Blood-Drunk Miner combat based on Paradise: dash from long range, fire the KA at
/// medium range, let HTN melee at close range, and periodically transform the cleaving saw.
/// </summary>
public sealed partial class BloodDrunkMinerSystem : EntitySystem
{
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedGunSystem _gun = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedItemSystem _item = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private NPCUseActionsOnTargetSystem _npcActions = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodDrunkMinerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BloodDrunkMinerComponent, BloodDrunkMinerDashAction>(OnCombatAction);
        SubscribeLocalEvent<BloodDrunkMinerComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<BloodDrunkMinerComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMapInit(Entity<BloodDrunkMinerComponent> ent, ref MapInitEvent args)
    {
        RemComp<SSDIndicatorComponent>(ent);
        ApplySawMode(ent);
    }

    private void OnCombatAction(Entity<BloodDrunkMinerComponent> ent, ref BloodDrunkMinerDashAction args)
    {
        if (args.Handled || !Exists(args.Target) || _mobState.IsDead(ent.Owner))
            return;

        var source = _transform.GetMapCoordinates(ent);
        var target = _transform.GetMapCoordinates(args.Target);
        if (source.MapId != target.MapId)
            return;

        var delta = target.Position - source.Position;
        var distance = delta.Length();
        var performedAttack = false;

        if (distance > ent.Comp.DashDistanceThreshold && _timing.CurTime >= ent.Comp.NextDash)
        {
            performedAttack = TryDash(ent, args.Target, source, target, args.DashSound);
        }
        else if (distance > ent.Comp.MinimumRangedDistance &&
                 distance <= ent.Comp.DashDistanceThreshold)
        {
            performedAttack = TryShoot(ent, args.Target);
        }
        else if (distance <= ent.Comp.MinimumRangedDistance)
        {
            performedAttack = TrySawAttack(ent, args.Target);
        }

        var transformed = TryTransformSaw(ent);
        if (!performedAttack && !transformed)
            return;

        args.Handled = true;
    }

    private bool TryDash(
        Entity<BloodDrunkMinerComponent> ent,
        EntityUid targetEntity,
        MapCoordinates source,
        MapCoordinates target,
        Robust.Shared.Audio.SoundSpecifier sound)
    {
        var delta = target.Position - source.Position;
        if (delta == Vector2.Zero)
            return false;

        var travel = Math.Min(delta.Length(), ent.Comp.MaximumDashRange);
        if (travel <= 0f)
            return false;

        // Stop just short of the victim so the HTN melee follow-up can connect instead of stacking both
        // entities on the same coordinates.
        var direction = Vector2.Normalize(delta);
        var landingDistance = Math.Max(0f, travel - 1f);
        var landing = new MapCoordinates(source.Position + direction * landingDistance, source.MapId);

        ent.Comp.NextDash = _timing.CurTime + ent.Comp.DashCooldown;
        _npcActions.LockActions(ent.Owner, TimeSpan.FromSeconds(0.5));
        _transform.SetMapCoordinates(ent.Owner, landing);
        _audio.PlayPvs(sound, ent.Owner);

        Timer.Spawn(TimeSpan.FromSeconds(0.2), () =>
        {
            if (Exists(ent.Owner) && Exists(targetEntity) && !_mobState.IsDead(ent.Owner))
                TryShoot(ent, targetEntity);
        });

        return true;
    }

    private bool TryShoot(Entity<BloodDrunkMinerComponent> ent, EntityUid target)
    {
        if (!Exists(target) || !TryComp<GunComponent>(ent, out var gun))
            return false;

        var source = _transform.GetMapCoordinates(ent);
        var targetCoordinates = _transform.GetMapCoordinates(target);
        if (source.MapId != targetCoordinates.MapId ||
            !source.InRange(targetCoordinates, ent.Comp.DashDistanceThreshold))
        {
            return false;
        }

        gun.NextFire = TimeSpan.Zero;
        _gun.AttemptShoot(ent.Owner, (ent.Owner, gun), Transform(target).Coordinates);
        return true;
    }

    private bool TrySawAttack(Entity<BloodDrunkMinerComponent> ent, EntityUid target)
    {
        if (!Exists(target) || !TryComp<MeleeWeaponComponent>(ent, out var melee))
            return false;

        var blocker = Comp<MegafaunaSpecialAttackOnlyComponent>(ent);
        blocker.AllowActionMelee = true;
        try
        {
            return _melee.AttemptLightAttack(ent.Owner, ent.Owner, melee, target);
        }
        finally
        {
            blocker.AllowActionMelee = false;
        }
    }

    private bool TryTransformSaw(Entity<BloodDrunkMinerComponent> ent)
    {
        if (_timing.CurTime < ent.Comp.NextTransform || !_random.Prob(ent.Comp.TransformChance))
            return false;

        ent.Comp.SawOpen = !ent.Comp.SawOpen;
        ent.Comp.NextTransform = _timing.CurTime + TimeSpan.FromSeconds(
            _random.NextFloat(
                (float) ent.Comp.TransformCooldownMin.TotalSeconds,
                (float) ent.Comp.TransformCooldownMax.TotalSeconds));
        ApplySawMode(ent);
        return true;
    }

    private void ApplySawMode(Entity<BloodDrunkMinerComponent> ent)
    {
        if (TryComp<MeleeWeaponComponent>(ent, out var melee))
        {
            melee.AttackRate = ent.Comp.SawOpen ? ent.Comp.OpenAttackRate : ent.Comp.ClosedAttackRate;
            melee.Angle = ent.Comp.SawOpen ? Angle.FromDegrees(120) : Angle.FromDegrees(30);
            melee.Damage = new DamageSpecifier(ent.Comp.SawOpen ? ent.Comp.OpenDamage : ent.Comp.ClosedDamage);
            Dirty(ent.Owner, melee);
        }

        foreach (var held in _hands.EnumerateHeld(ent.Owner))
        {
            if (!TryComp<CleavingSawComponent>(held, out var saw))
                continue;

            saw.Open = ent.Comp.SawOpen;
            Dirty(held, saw);
            _appearance.SetData(held, CleavingSawVisuals.Open, saw.Open);
            _item.SetHeldPrefix(held, saw.Open ? "open" : null);

            if (TryComp<MeleeWeaponComponent>(held, out var heldMelee))
            {
                heldMelee.AttackRate = saw.Open ? saw.OpenAttackRate : saw.ClosedAttackRate;
                heldMelee.Angle = saw.Open ? saw.OpenAngle : saw.ClosedAngle;
                heldMelee.Damage = new DamageSpecifier(saw.Open ? saw.OpenDamage : saw.ClosedDamage);
                Dirty(held, heldMelee);
            }
        }
    }

    private void OnMeleeHit(Entity<BloodDrunkMinerComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.MeleeHeal.Empty)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(target) || _mobState.IsDead(target))
                continue;

            _damage.TryChangeDamage(ent.Owner, ent.Comp.MeleeHeal, true, false, origin: ent.Owner);
            break;
        }
    }

    private void OnMobStateChanged(Entity<BloodDrunkMinerComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            _npcActions.UnlockActions(ent.Owner);
    }
}
