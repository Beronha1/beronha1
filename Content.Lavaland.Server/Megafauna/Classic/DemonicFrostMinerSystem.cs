// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Lavaland.Shared.Megafauna.Events;
using Content.Lavaland.Server.NPC;
using Content.Server.Administration.Logs;
using Content.Server.NPC.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.Classic;

/// <summary>
/// Native projectile patterns and one-time second health phase for the Demonic Frost Miner.
/// </summary>
public sealed partial class DemonicFrostMinerSystem : EntitySystem
{
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedGunSystem _gun = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MobThresholdSystem _threshold = default!;
    [Dependency] private NPCUseActionsOnTargetSystem _npcActions = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DemonicFrostMinerComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<DemonicFrostMinerComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<DemonicFrostMinerComponent, DemonicFrostOrbActionEvent>(OnFrostOrbs);
        SubscribeLocalEvent<DemonicFrostMinerComponent, DemonicFrostMachineGunActionEvent>(OnMachineGun);
        SubscribeLocalEvent<DemonicFrostMinerComponent, DemonicFrostShotgunActionEvent>(OnShotgun);
    }

    private void OnDamageChanged(Entity<DemonicFrostMinerComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || ent.Comp.Enraged || ent.Comp.Transforming ||
            !TryComp<DamageableComponent>(ent, out var damageable) ||
            !_threshold.TryGetThresholdForState(ent, MobState.Dead, out var deadThreshold) ||
            deadThreshold.Value <= 0)
        {
            return;
        }

        var fraction = _damage.GetTotalDamage((ent.Owner, damageable)).Float() / deadThreshold.Value.Float();
        if (fraction < ent.Comp.EnrageDamageFraction)
            return;

        ent.Comp.Transforming = true;
        EnsureComp<GodmodeComponent>(ent);
        _npcActions.SetDelaySpeed(ent, 2f);
        _popup.PopupEntity(Loc.GetString("demonic-frost-miner-enraging"), ent, ent, PopupType.LargeCaution);

        var uid = ent.Owner;
        Timer.Spawn(ent.Comp.EnrageInvulnerability, () => CompleteEnrage(uid));
    }

    private void CompleteEnrage(EntityUid uid)
    {
        if (!TryComp<DemonicFrostMinerComponent>(uid, out var component) || _mobState.IsDead(uid))
            return;

        component.Transforming = false;
        component.Enraged = true;
        _damage.ClearAllDamage(uid);
        RemCompDeferred<GodmodeComponent>(uid);
        _npcActions.SetDelaySpeed(uid, 0.65f);
        _appearance.SetData(uid, DemonicFrostMinerVisuals.Enraged, true);
        _popup.PopupEntity(Loc.GetString("demonic-frost-miner-enraged"), uid, uid, PopupType.LargeCaution);
        _adminLog.Add(LogType.Action, $"{ToPrettyString(uid):boss} entered its Demonic Frost Miner second phase");
    }

    private void OnMeleeHit(Entity<DemonicFrostMinerComponent> ent, ref MeleeHitEvent args)
    {
        foreach (var target in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(target) || _mobState.IsDead(target) || ent.Comp.MeleeHeal.Empty)
                continue;

            _damage.TryChangeDamage(ent.Owner, ent.Comp.MeleeHeal, true, false, origin: ent.Owner);
            break;
        }
    }

    private void OnFrostOrbs(Entity<DemonicFrostMinerComponent> ent, ref DemonicFrostOrbActionEvent args)
    {
        if (args.Handled || ent.Comp.Transforming)
            return;

        args.Handled = true;
        var count = ent.Comp.Enraged ? 9 : 5;
        ShootSpread(ent, args.Target, ent.Comp.FrostOrbProjectile, count, ent.Comp.Enraged ? 0.35f : 0.2f, ent.Comp.Enraged ? 13f : 9f);
    }

    private void OnMachineGun(Entity<DemonicFrostMinerComponent> ent, ref DemonicFrostMachineGunActionEvent args)
    {
        if (args.Handled || ent.Comp.Transforming)
            return;

        args.Handled = true;
        var shots = ent.Comp.Enraged ? 18 : 10;
        var uid = ent.Owner;
        var target = args.Target;
        for (var i = 0; i < shots; i++)
        {
            var delay = TimeSpan.FromSeconds(i * (ent.Comp.Enraged ? 0.08f : 0.12f));
            Timer.Spawn(delay, () =>
            {
                if (TryComp<DemonicFrostMinerComponent>(uid, out var current) && !current.Transforming &&
                    Exists(target) && !_mobState.IsDead(uid))
                {
                    ShootSpread((uid, current), target, current.SnowballProjectile, 1, 0.16f, current.Enraged ? 20f : 16f);
                }
            });
        }
    }

    private void OnShotgun(Entity<DemonicFrostMinerComponent> ent, ref DemonicFrostShotgunActionEvent args)
    {
        if (args.Handled || ent.Comp.Transforming)
            return;

        args.Handled = true;
        ShootSpread(ent, args.Target, ent.Comp.IceBlastProjectile, ent.Comp.Enraged ? 13 : 7, ent.Comp.Enraged ? 0.75f : 0.5f, 14f);
    }

    private void ShootSpread(
        Entity<DemonicFrostMinerComponent> source,
        EntityUid target,
        EntProtoId projectilePrototype,
        int count,
        float spread,
        float speed)
    {
        if (!Exists(target))
            return;

        var sourceMap = _transform.GetMapCoordinates(source);
        var targetMap = _transform.GetMapCoordinates(target);
        if (sourceMap.MapId != targetMap.MapId)
            return;

        var direction = targetMap.Position - sourceMap.Position;
        if (direction.LengthSquared() <= float.Epsilon)
            return;

        for (var i = 0; i < count; i++)
        {
            var projectile = Spawn(projectilePrototype, Transform(source).Coordinates);
            var shotDirection = direction.Normalized();
            if (count > 1)
            {
                var normalizedIndex = i / (float) (count - 1) - 0.5f;
                shotDirection = new Angle(normalizedIndex * spread + _random.NextFloat(-0.025f, 0.025f)).RotateVec(shotDirection);
            }

            _gun.ShootProjectile(projectile, shotDirection, Vector2.Zero, source, source, speed);
        }
    }
}
