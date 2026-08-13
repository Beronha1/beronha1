// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Lavaland.Server.Mobs;
using Content.Lavaland.Server.NPC;
using Content.Lavaland.Shared.Megafauna.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.Classic;

public sealed partial class LegionSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MobThresholdSystem _threshold = default!;
    [Dependency] private NPCUseActionsOnTargetSystem _npcActions = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SpawnOnDeathSystem _spawnOnDeath = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LegionBossComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<LegionSplitComponent, MapInitEvent>(OnSplitMapInit);
        SubscribeLocalEvent<LegionBossComponent, MegaLegionAction>(OnAction);
        SubscribeLocalEvent<LegionBossComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<LegionBossComponent, MobStateChangedEvent>(OnBossKilled);
        SubscribeLocalEvent<LegionSplitComponent, MobStateChangedEvent>(OnSplitKilled);
        SubscribeLocalEvent<LegionSplitComponent, AttackedEvent>(OnSplitAttacked);
        SubscribeLocalEvent<LegionEncounterComponent, EntityTerminatingEvent>(OnEncounterTerminating);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<LegionBossComponent>();
        while (query.MoveNext(out _, out var component))
        {
            if (_timing.CurTime < component.NextStateSwitchTime)
                continue;

            component.CurrentState = component.CurrentState == LegionState.Summoning
                ? LegionState.Charging
                : LegionState.Summoning;
            component.NextStateSwitchTime = _timing.CurTime + TimeSpan.FromSeconds(component.StateSwitchInterval);
        }
    }

    private void OnMapInit(Entity<LegionBossComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextStateSwitchTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.StateSwitchInterval);
        ent.Comp.NextSummonTime = _timing.CurTime;
        ent.Comp.NextChargeTime = _timing.CurTime;
    }

    private void OnSplitMapInit(Entity<LegionSplitComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.SplitGroup = Guid.NewGuid();
    }

    private void OnAction(Entity<LegionBossComponent> ent, ref MegaLegionAction args)
    {
        if (!Exists(args.Target) || _mobState.IsIncapacitated(ent) || _mobState.IsIncapacitated(args.Target))
            return;

        // Paradise does not roll from a single undifferentiated summon. It alternates between a charge,
        // a telegraphed disintegration line, a large minion, and ordinary skulls. Excluding the previous
        // ranged pattern ensures the complete repertoire appears during a normal fight.
        if (_random.Prob(0.3f) && _timing.CurTime >= ent.Comp.NextChargeTime)
        {
            args.Handled = TryCharge(ent, args.Target);
            return;
        }

        if (_timing.CurTime < ent.Comp.NextSummonTime)
            return;

        var patterns = new List<LegionRangedPattern>();
        foreach (var pattern in Enum.GetValues<LegionRangedPattern>())
        {
            if (pattern != ent.Comp.LastRangedPattern)
                patterns.Add(pattern);
        }

        var selected = _random.Pick(patterns);
        var succeeded = selected switch
        {
            LegionRangedPattern.Laser => TryDisintegrationLaser(ent, args.Target),
            LegionRangedPattern.LargeSummon => TrySummon(ent, args.Target, ent.Comp.LargeMinionPrototype, 1),
            _ => TrySummon(ent, args.Target, ent.Comp.MinionPrototype, ent.Comp.SummonCount),
        };

        if (!succeeded)
            return;

        ent.Comp.LastRangedPattern = selected;
        args.Handled = true;
    }

    private bool TryCharge(Entity<LegionBossComponent> ent, EntityUid target)
    {
        var delta = Transform(target).Coordinates.Position - Transform(ent).Coordinates.Position;
        if (delta == Vector2.Zero)
            return false;

        var direction = Vector2.Normalize(delta);
        _npcActions.LockActions(ent.Owner, TimeSpan.FromSeconds(3));
        _popup.PopupEntity(Loc.GetString("legion-combat-charge"), ent.Owner, PopupType.LargeCaution);
        _throwing.TryThrow(ent, Transform(ent).Coordinates.Offset(direction * 7f), 15f);
        ent.Comp.NextChargeTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.ChargeInterval);
        return true;
    }

    private bool TrySummon(
        Entity<LegionBossComponent> ent,
        EntityUid target,
        EntProtoId prototype,
        int count)
    {
        if (count <= 0)
            return false;

        var encounterRoot = GetEncounterRoot(ent);
        var encounter = EnsureComp<LegionEncounterComponent>(encounterRoot);
        PruneSummons(encounter);

        var isLarge = prototype == ent.Comp.LargeMinionPrototype;
        var available = Math.Max(0, ent.Comp.MaximumActiveSummons - encounter.ActiveSummons.Count);
        if (isLarge)
            available = Math.Min(available,
                Math.Max(0, ent.Comp.MaximumActiveLargeSummons - encounter.ActiveLargeSummons.Count));

        count = Math.Min(count, available);
        if (count == 0)
            return false;

        var coordinates = Transform(ent).Coordinates;
        for (var index = 0; index < count; index++)
        {
            var angle = MathF.Tau * index / count;
            var summon = Spawn(prototype, coordinates.Offset(new Vector2(MathF.Cos(angle), MathF.Sin(angle))));
            SetTarget(summon, target);
            encounter.ActiveSummons.Add(summon);
            if (isLarge)
                encounter.ActiveLargeSummons.Add(summon);
        }

        _npcActions.LockActions(ent.Owner, TimeSpan.FromSeconds(prototype == ent.Comp.LargeMinionPrototype ? 5 : 2));
        ent.Comp.NextSummonTime = _timing.CurTime + TimeSpan.FromSeconds(
            prototype == ent.Comp.LargeMinionPrototype ? 5 : ent.Comp.SummonInterval);
        return true;
    }

    private bool TryDisintegrationLaser(Entity<LegionBossComponent> ent, EntityUid target)
    {
        var map = Transform(ent).MapUid;
        if (map == null)
            return false;

        var origin = Transform(ent).WorldPosition;
        var delta = Transform(target).WorldPosition - origin;
        if (delta == Vector2.Zero)
            return false;

        var direction = Vector2.Normalize(delta);
        var healthRatio = GetHealthRatio(ent);
        var windup = 0.25f + healthRatio;
        var line = new List<EntityCoordinates>();

        for (var step = 1; step <= (int) ent.Comp.LaserRange; step++)
        {
            var coordinates = new EntityCoordinates(map.Value, origin + direction * step);
            line.Add(coordinates);
            Spawn(ent.Comp.LaserMarkerPrototype, coordinates);
        }

        _npcActions.LockActions(ent.Owner, TimeSpan.FromSeconds(windup + 2));
        Timer.Spawn(TimeSpan.FromSeconds(windup), () =>
        {
            if (!Exists(ent.Owner) || _mobState.IsDead(ent.Owner))
                return;

            var damaged = new HashSet<EntityUid>();
            foreach (var coordinates in line)
            {
                foreach (var victim in _lookup.GetEntitiesInRange<MobStateComponent>(coordinates, 0.55f))
                {
                    if (victim.Owner == ent.Owner || !damaged.Add(victim.Owner))
                        continue;

                    _damage.TryChangeDamage(victim.Owner, ent.Comp.LaserDamage, origin: ent.Owner);
                }
            }
        });

        ent.Comp.NextSummonTime = _timing.CurTime + TimeSpan.FromSeconds(windup + 2);
        return true;
    }

    private float GetHealthRatio(Entity<LegionBossComponent> ent)
    {
        if (!_threshold.TryGetThresholdForState(ent, MobState.Dead, out var threshold) ||
            !TryComp<DamageableComponent>(ent, out var damageable) ||
            threshold <= 0)
        {
            return 1f;
        }

        return Math.Clamp(1f - (float) (_damage.GetTotalDamage((ent.Owner, damageable)) / threshold), 0f, 1f);
    }

    private void SetTarget(EntityUid summon, EntityUid target)
    {
        if (!TryComp<Content.Server.NPC.HTN.HTNComponent>(summon, out var htn))
            return;

        htn.Blackboard.SetValue("Target", target);
    }

    private void OnDamageChanged(Entity<LegionBossComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased ||
            _mobState.IsDead(ent.Owner) ||
            _timing.CurTime < ent.Comp.NextReactiveSummon ||
            !_random.Prob(0.33f))
        {
            return;
        }

        var encounterRoot = GetEncounterRoot(ent);
        var encounter = EnsureComp<LegionEncounterComponent>(encounterRoot);
        PruneSummons(encounter);
        if (encounter.ActiveSummons.Count >= ent.Comp.MaximumActiveSummons)
            return;

        var brood = Spawn(ent.Comp.MinionPrototype, Transform(ent).Coordinates);
        encounter.ActiveSummons.Add(brood);
        ent.Comp.NextReactiveSummon = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.ReactiveSummonCooldown);
    }

    private void OnBossKilled(Entity<LegionBossComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || HasComp<LegionSplitComponent>(ent))
            return;

        var coords = Transform(ent).Coordinates;
        var encounter = EnsureComp<LegionEncounterComponent>(ent);
        if (encounter.SplitGroup != Guid.Empty)
            return;

        encounter.SplitGroup = Guid.NewGuid();
        if (TryComp<PhysicsComponent>(ent, out var physics))
            _physics.SetCanCollide(ent, false, body: physics);

        var count = ent.Comp.SplitPrototypes.Count;
        if (count == 0)
        {
            CompleteEncounter(ent, encounter.SplitGroup);
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var angle = MathF.Tau * i / Math.Max(1, count);
            var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 0.8f;
            SpawnEncounterSplit(ent.Comp.SplitPrototypes[i], coords.Offset(offset), encounter.SplitGroup, ent);
        }
    }

    private void OnSplitKilled(Entity<LegionSplitComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var coords = Transform(ent).Coordinates;
        if (ent.Comp.NextSplitPrototype is { } nextSplit)
        {
            for (var i = 0; i < 2; i++)
                SpawnEncounterSplit(
                    nextSplit,
                    coords.Offset(new Vector2(i == 0 ? -0.6f : 0.6f, 0f)),
                    ent.Comp.SplitGroup,
                    ent.Comp.RootCarcass);
        }
        else if (IsLastLivingSplit(ent, ent.Comp.SplitGroup) && ent.Comp.RootCarcass is { } root)
        {
            CompleteEncounter(root, ent.Comp.SplitGroup);
        }

        QueueDel(ent);
    }

    private void OnSplitAttacked(Entity<LegionSplitComponent> ent, ref AttackedEvent args)
    {
        if (_mobState.IsDead(ent) ||
            ent.Comp.RootCarcass is not { } root ||
            !TryComp<SpawnLootOnDeathComponent>(root, out var rootLoot))
        {
            return;
        }

        rootLoot.DoSpecialLoot &= _whitelist.IsWhitelistPassOrNull(rootLoot.SpecialWeaponWhitelist, args.Used);
    }

    private void SpawnEncounterSplit(
        EntProtoId prototype,
        EntityCoordinates coordinates,
        Guid splitGroup,
        EntityUid? rootCarcass)
    {
        var split = Spawn(prototype, coordinates);
        if (!TryComp<LegionSplitComponent>(split, out var splitComponent))
            return;

        splitComponent.SplitGroup = splitGroup;
        splitComponent.RootCarcass = rootCarcass;
    }

    private void CompleteEncounter(EntityUid root, Guid splitGroup)
    {
        if (!TryComp<LegionEncounterComponent>(root, out var encounter) ||
            encounter.Completed ||
            encounter.SplitGroup != splitGroup)
        {
            return;
        }

        // Commit completion before spawning anything. Several final fragments can
        // enter their death event in the same tick.
        encounter.Completed = true;
        CleanupSummons(encounter);

        if (TryComp<SpawnLootOnDeathComponent>(root, out var loot))
            _spawnOnDeath.TryDropLoot((root, loot));

        SpawnAttachedTo("LightningCrackleNeutral", Transform(root).Coordinates);
        _popup.PopupEntity(Loc.GetString("legion-encounter-complete"), root, PopupType.LargeCaution);
    }

    private EntityUid GetEncounterRoot(Entity<LegionBossComponent> ent)
    {
        if (TryComp<LegionSplitComponent>(ent, out var split) &&
            split.RootCarcass is { } root &&
            Exists(root))
        {
            return root;
        }

        return ent.Owner;
    }

    private void PruneSummons(LegionEncounterComponent encounter)
    {
        encounter.ActiveSummons.RemoveWhere(uid => !Exists(uid) || _mobState.IsDead(uid));
        encounter.ActiveLargeSummons.RemoveWhere(uid => !encounter.ActiveSummons.Contains(uid));
    }

    private void CleanupSummons(LegionEncounterComponent encounter)
    {
        foreach (var summon in encounter.ActiveSummons)
        {
            if (Exists(summon))
                QueueDel(summon);
        }

        encounter.ActiveSummons.Clear();
        encounter.ActiveLargeSummons.Clear();
    }

    private void OnEncounterTerminating(Entity<LegionEncounterComponent> ent, ref EntityTerminatingEvent args)
        => CleanupSummons(ent.Comp);

    private bool IsLastLivingSplit(EntityUid dyingSplit, Guid splitGroup)
    {
        var query = EntityQueryEnumerator<LegionSplitComponent>();
        while (query.MoveNext(out var uid, out var split))
        {
            if (uid != dyingSplit &&
                split.SplitGroup == splitGroup &&
                !_mobState.IsDead(uid))
            {
                return false;
            }
        }

        return true;
    }
}
