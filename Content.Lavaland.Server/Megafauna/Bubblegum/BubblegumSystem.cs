// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using System.Linq;
using System.Numerics;
using Content.Lavaland.Server.Megafauna.Bubblegum;
using Content.Lavaland.Server.NPC;
using Content.Lavaland.Shared.Megafauna.Harvesting;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared.Actions.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Fluids.Components;
using Content.Shared.Ghost.Components;
using Content.Shared.Gibbing;
using Content.Lavaland.Shared.Megafauna.Events;
using Content.Shared.Humanoid;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Visuals;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.Bubblegum;

public sealed partial class BubblegumSystem : EntitySystem
{
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private GibbingSystem _gibbing = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MobThresholdSystem _threshold = default!;
    [Dependency] private NPCUseActionsOnTargetSystem _npcActions = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private ITileDefinitionManager _tileDefinitions = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private NPCSystem _npc = default!;

    private const float LowHealthThreshold = 0.5f;
    private const float PassiveHandRadius = 5f;
    private const float PassiveHandInterval = 2f;
    private const float PassiveHandChance = 0.5f;

    private Dictionary<EntityUid, List<EntityUid>> _activeIllusions = new();
    private HashSet<EntityUid> _dashDamagedTargets = new();
    private readonly Dictionary<EntityUid, BubblegumArenaSession> _arenaSessions = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BubblegumBossComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<BubblegumBossComponent, MapInitEvent>(OnBubblegumMapInit);
        SubscribeLocalEvent<BubblegumBossComponent, MobStateChangedEvent>(OnBubblegumKilled);
        SubscribeLocalEvent<BubblegumBossComponent, MoveEvent>(OnBubblegumMoved);
        SubscribeLocalEvent<BubblegumBossComponent, EntityTerminatingEvent>(OnTerminating);

        SubscribeLocalEvent<BubblegumBossComponent, BubblegumRageActionEvent>(OnRageAction);
        SubscribeLocalEvent<BubblegumBossComponent, BubblegumBloodDiveActionEvent>(OnBloodDiveAction);
        SubscribeLocalEvent<BubblegumBossComponent, BubblegumTripleDashActionEvent>(OnTripleDash);
        SubscribeLocalEvent<BubblegumBossComponent, BubblegumIllusionDashActionEvent>(OnIllusionDash);
        SubscribeLocalEvent<BubblegumBossComponent, BubblegumPentagramDashActionEvent>(OnPentagramDashAction);
        SubscribeLocalEvent<BubblegumBossComponent, BubblegumChaoticIllusionDashActionEvent>(OnChaoticIllusionDashAction);
    }

    private void OnTerminating(Entity<BubblegumBossComponent> ent, ref EntityTerminatingEvent args)
    {
        CleanupIllusions(ent.Owner);

        if (_arenaSessions.Remove(ent.Owner, out var session))
            ReturnFromSecondLifeArena(session, ent.Owner, aborted: true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateRageState();
        UpdatePassiveHandAttack();
        UpdatePassiveBloodWarp();
    }

    #region Event Handlers

    private void OnBubblegumMoved(Entity<BubblegumBossComponent> ent, ref MoveEvent args)
    {
        if (!args.OnlyRotation && !_mobState.IsDead(ent.Owner))
            SpawnBloodPool(ent);
    }

    private void OnBubblegumMapInit(Entity<BubblegumBossComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.SecondLife)
            ent.Comp.CurrentPhase = BubblegumPhase.Enraged;

        // Initialize the selector from Bubblegum's phase configuration on every spawn. Relying only on the
        // NPC component's prototype list left phase-one weights unapplied until the first phase transition.
        UpdatePhaseActions(ent, ent.Comp);
    }

    private void OnDamageChanged(EntityUid uid, BubblegumBossComponent component, DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        var healthRatio = GetHealthRatio(uid);
        var newPhase = healthRatio > LowHealthThreshold
            ? BubblegumPhase.Normal : BubblegumPhase.Enraged;

        if (newPhase != component.CurrentPhase)
        {
            component.CurrentPhase = newPhase;
            UpdatePhaseActions(uid, component);
        }
    }

    private void UpdatePhaseActions(EntityUid uid, BubblegumBossComponent component)
    {
        if (!TryComp<NPCUseActionsOnTargetComponent>(uid, out var npcActions))
            return;

        if (component.CurrentPhase == BubblegumPhase.Normal)
        {
            _npcActions.SetActions(uid,
                component.Phase1Actions ?? new(),
                component.Phase1Chances ?? new(),
                npcActions);
        }
        else
        {
            _npcActions.SetActions(uid,
                component.Phase2Actions ?? new(),
                component.Phase2Chances ?? new(),
                npcActions);
        }

    }

    private void OnBubblegumKilled(EntityUid uid, BubblegumBossComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        _npcActions.UnlockActions(uid);
        CleanupIllusions(uid);
        component.IsRaging = false;
        RemCompDeferred<GodmodeComponent>(uid);

        if (component.EnableSecondLife && !component.SecondLife && !component.TransitionStarted)
        {
            BeginSecondLife((uid, component));
            return;
        }

        if (component.SecondLife && _arenaSessions.Remove(uid, out var session))
            ReturnFromSecondLifeArena(session, uid, aborted: false);

        var coords = Transform(uid).Coordinates;
        foreach (var reward in component.RewardsProto)
            Spawn(reward, coords);

        if (!HasComp<MegafaunaHarvestableComponent>(uid))
            QueueDel(uid);
    }

    private void BeginSecondLife(Entity<BubblegumBossComponent> ent)
    {
        ent.Comp.TransitionStarted = true;
        var returnAnchor = Transform(ent).Coordinates;
        var participants = new Dictionary<EntityUid, EntityCoordinates>();

        foreach (var target in _lookup.GetEntitiesInRange<HumanoidProfileComponent>(
                     returnAnchor,
                     ent.Comp.SecondLifeCaptureRadius))
        {
            if (_mobState.IsDead(target))
                continue;

            participants[target] = Transform(target).Coordinates;
        }

        EntityUid? arenaMap = null;
        EntityUid? arenaGrid = null;
        EntityCoordinates bossCoordinates = returnAnchor;

        if (participants.Count > 0)
        {
            arenaMap = _map.CreateMap(out var arenaMapId);
            var grid = _map.CreateGridEntity(arenaMapId);
            arenaGrid = grid.Owner;
            BuildSecondLifeArena(grid, ent.Comp);
            bossCoordinates = new EntityCoordinates(grid, new Vector2(0.5f, 0.5f));

            var index = 0;
            foreach (var participant in participants.Keys)
            {
                var angle = index++ * MathF.Tau / participants.Count;
                var arrival = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 5f + new Vector2(0.5f, 0.5f);
                _transform.SetCoordinates(participant, new EntityCoordinates(grid, arrival));
                _audio.PlayPvs("/Audio/_Goobstation/Misc/enter_blood.ogg", participant);
                PopupSecondLife(participant, "bubblegum-second-life-abducted", PopupType.LargeCaution);
            }
        }

        var secondLife = Spawn(ent.Comp.SecondLifePrototype, bossCoordinates);
        _arenaSessions[secondLife] = new BubblegumArenaSession(
            returnAnchor,
            participants,
            arenaMap,
            arenaGrid);

        _audio.PlayPvs("/Audio/_Goobstation/Misc/exit_blood.ogg", secondLife);
        QueueDel(ent);
    }

    private void BuildSecondLifeArena(Entity<MapGridComponent> grid, BubblegumBossComponent component)
    {
        var tile = (ContentTileDefinition) _tileDefinitions[component.ArenaFloor.Id];
        var radius = Math.Max(8, component.ArenaRadius);

        for (var x = -radius; x <= radius; x++)
        {
            for (var y = -radius; y <= radius; y++)
            {
                _map.SetTile(grid, grid.Comp, new Vector2i(x, y), new Tile(tile.TileId));
                if (Math.Abs(x) != radius && Math.Abs(y) != radius)
                    continue;

                Spawn(component.ArenaWall, new EntityCoordinates(grid, new Vector2(x + 0.5f, y + 0.5f)));
            }
        }
    }

    private void ReturnFromSecondLifeArena(BubblegumArenaSession session, EntityUid boss, bool aborted)
    {
        foreach (var (participant, coordinates) in session.Participants)
        {
            if (!Exists(participant) || !Exists(coordinates.EntityId))
                continue;

            _transform.SetCoordinates(participant, coordinates);
            _audio.PlayPvs("/Audio/_Goobstation/Misc/exit_blood.ogg", participant);
            PopupSecondLife(participant,
                aborted ? "bubblegum-second-life-aborted" : "bubblegum-second-life-returned",
                aborted ? PopupType.MediumCaution : PopupType.Large);
        }

        if (Exists(boss) && Exists(session.ReturnAnchor.EntityId))
            _transform.SetCoordinates(boss, session.ReturnAnchor);

        if (session.ArenaMap is not { } map || session.ArenaGrid is not { } grid)
            return;

        // Other death subscribers may spawn the trophy after this handler. Sweep direct loose contents one tick
        // later, then destroy the private map only after everything recoverable has returned to Lavaland.
        Timer.Spawn(TimeSpan.FromSeconds(1), () =>
        {
            if (!Exists(grid))
                return;

            var loose = new List<EntityUid>();
            var query = EntityQueryEnumerator<TransformComponent>();
            while (query.MoveNext(out var uid, out var xform))
            {
                if (uid == grid || xform.ParentUid != grid || xform.Anchored)
                    continue;
                loose.Add(uid);
            }

            var offset = 0;
            foreach (var uid in loose)
            {
                if (!Exists(uid) || !Exists(session.ReturnAnchor.EntityId))
                    continue;

                var dx = (offset % 5 - 2) * 0.25f;
                var dy = (offset / 5) * 0.25f;
                _transform.SetCoordinates(uid, session.ReturnAnchor.Offset(new Vector2(dx, dy)));
                offset++;
            }

            Timer.Spawn(TimeSpan.FromSeconds(1), () =>
            {
                if (_map.MapExists(Transform(map).MapID))
                    _map.DeleteMap(Transform(map).MapID);
            });
        });
    }

    private void PopupSecondLife(EntityUid target, string message, PopupType type)
    {
        _popup.PopupEntity(Loc.GetString(message), target, target, type);
    }

    #endregion

    #region Rage System

    private void OnRageAction(Entity<BubblegumBossComponent> ent, ref BubblegumRageActionEvent args)
    {
        if (ent.Comp.IsRaging || _mobState.IsDead(ent.Owner))
            return;

        TriggerRage(ent, ent.Comp);
        args.Handled = true;
    }

    private void TriggerRage(EntityUid uid, BubblegumBossComponent component)
    {
        if (component.IsRaging)
            return;

        component.IsRaging = true;
        var duration = _random.NextFloat(component.RageDurationMin, component.RageDurationMax);
        component.RageEndTime = _timing.CurTime + TimeSpan.FromSeconds(duration);

        EnsureComp<GodmodeComponent>(uid);
        _appearance.SetData(uid, VisualLayers.Enabled, true);
        _npcActions.SetDelaySpeed(uid, component.RageDelayModifier);
    }

    private void UpdateRageState()
    {
        var query = EntityQueryEnumerator<BubblegumBossComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsRaging)
                continue;

            if (_timing.CurTime >= comp.RageEndTime)
            {
                comp.IsRaging = false;
                RemCompDeferred<GodmodeComponent>(uid);
                _appearance.SetData(uid, VisualLayers.Enabled, false);
                _npcActions.SetDelaySpeed(uid, 1f);
            }
        }
    }

    #endregion

    #region Triple Dash

    private void OnTripleDash(Entity<BubblegumBossComponent> ent, ref BubblegumTripleDashActionEvent args)
    {
        var target = args.Target;
        if (!Exists(target) || _mobState.IsDead(ent.Owner))
            return;

        var mapId = Transform(ent.Owner).MapID;
        if (mapId == MapId.Nullspace)
            return;

        args.Handled = true;
        ent.Comp.LastDashStatus = "accepted";
        _npcActions.LockActions(ent.Owner, TimeSpan.FromSeconds(5));
        _npc.SleepNPC(ent.Owner);
        CleanupIllusions(ent.Owner);
        SpawnBloodPool(ent);

        PerformTripleDashStep(ent, target, mapId, args.DashDamage, args.DashDistance, args.MoveSpeed,
            args.UseSineWaveForLast, args.DashDelays, 0);
    }

    private void PerformTripleDashStep(Entity<BubblegumBossComponent> ent, EntityUid target, MapId mapId,
        DamageSpecifier dashDamage, float dashDistance, float moveSpeed, bool useSineWaveForLast,
        List<float> dashDelays, int stepIndex)
    {
        ent.Comp.LastDashStatus = "preparing";
        if (!Exists(ent.Owner) || _mobState.IsDead(ent.Owner) || !Exists(target))
        {
            ent.Comp.LastDashStatus = "invalid-entity";
            FinishAttackSequence(ent, target);
            return;
        }

        var bossPos = _transform.GetWorldPosition(ent);
        var currentTargetPos = _transform.GetWorldPosition(target);
        Vector2 dashTarget;

        if (stepIndex == 2)
        {
            if (useSineWaveForLast && _random.Prob(0.5f))
            {
                var direction = NormalizeOrZero(currentTargetPos - bossPos);
                var sineOffset = MathF.Sin(_timing.CurTime.Seconds * 4) * 2f;
                var perpendicular = new Vector2(-direction.Y, direction.X);
                dashTarget = currentTargetPos + perpendicular * sineOffset;
            }
            else
            {
                var direction = NormalizeOrZero(currentTargetPos - bossPos);
                dashTarget = currentTargetPos + direction * 3.5f;
            }
        }
        else
        {
            var direction = NormalizeOrZero(currentTargetPos - bossPos);
            dashTarget = bossPos + direction * dashDistance;
        }

        var centerDashTarget = GetTileCenter(mapId, dashTarget);
        var markerCoords = _transform.ToCoordinates(new MapCoordinates(centerDashTarget, mapId));

        if (!IsValidSpawnPosition(markerCoords))
        {
            var safeCoords = FindSafePositionNear(ent, markerCoords);
            if (safeCoords == null)
            {
                ent.Comp.LastDashStatus = "no-safe-position";
                FinishAttackSequence(ent, target);
                return;
            }
            markerCoords = safeCoords.Value;
        }

        ent.Comp.LastDashMarker = Spawn(ent.Comp.DashMarker, markerCoords);
        ent.Comp.LastDashStatus = "telegraphed";

        // Paradise revs each charge after placing its landing telegraph. Previously Whiskey dashed immediately
        // and applied this delay afterwards, making the warning effectively invisible and the sequence erratic.
        var revDelay = stepIndex < dashDelays.Count
            ? Math.Max(0f, dashDelays[stepIndex])
            : 0f;
        Timer.Spawn(TimeSpan.FromSeconds(revDelay), () =>
        {
            if (!Exists(ent.Owner) || _mobState.IsDead(ent.Owner) || !Exists(target))
            {
                FinishAttackSequence(ent, target);
                return;
            }

            PerformDash(ent, markerCoords, dashDamage, moveSpeed, stepIndex == 2,
                () => ScheduleNextDashStep(ent, target, mapId, dashDamage, dashDistance, moveSpeed,
                    useSineWaveForLast, dashDelays, stepIndex));
        });
    }

    private void ScheduleNextDashStep(Entity<BubblegumBossComponent> ent, EntityUid target, MapId mapId,
        DamageSpecifier dashDamage, float dashDistance, float moveSpeed, bool useSineWaveForLast,
        List<float> dashDelays, int currentStep)
    {
        if (currentStep >= dashDelays.Count - 1)
            return;

        PerformTripleDashStep(ent, target, mapId, dashDamage, dashDistance, moveSpeed,
            useSineWaveForLast, dashDelays, currentStep + 1);
    }

    #endregion

    #region Illusion Dash

    private void OnIllusionDash(Entity<BubblegumBossComponent> ent, ref BubblegumIllusionDashActionEvent args)
    {
        var target = args.Target;
        if (!Exists(target) || _mobState.IsDead(ent.Owner))
            return;

        var mapUid = _transform.GetMap(ent.Owner);
        if (mapUid == null)
            return;

        args.Handled = true;
        _npcActions.LockActions(ent.Owner, TimeSpan.FromSeconds(12));
        _npc.SleepNPC(ent.Owner);
        SpawnBloodPool(ent);

        PerformIllusionDashIteration(ent, target, mapUid.Value, args, 0);
    }

    private void PerformIllusionDashIteration(Entity<BubblegumBossComponent> ent, EntityUid target,
        EntityUid mapUid, BubblegumIllusionDashActionEvent args, int iteration)
    {
        if (!Exists(ent.Owner) || !Exists(target) || _mobState.IsDead(ent.Owner))
        {
            if (iteration == 0)
            {
                _npc.WakeNPC(ent.Owner);
                SetHTNTarget(ent, target);
            }
            return;
        }

        var targetCoords = Transform(target).Coordinates;

        var markerCoords = targetCoords;
        if (!IsValidSpawnPosition(markerCoords))
        {
            var safeCoords = FindSafePositionNear(ent, markerCoords);
            if (safeCoords == null)
            {
                ContinueToNextIteration(ent, target, mapUid, args, iteration);
                return;
            }
            markerCoords = safeCoords.Value;
        }

        Spawn(ent.Comp.DashMarker, markerCoords);

        var totalEntities = args.IllusionCount + 1;
        var positions = GetCircularPositions(targetCoords, mapUid, totalEntities, args.PlacementRadius);
        if (positions.Count < totalEntities)
        {
            ContinueToNextIteration(ent, target, mapUid, args, iteration);
            return;
        }

        var bossIndex = _random.Next(positions.Count);
        var illusions = SpawnIllusionCircle(ent, target, targetCoords, positions, bossIndex,
            args.IllusionPrototype, args.IllusionDamage);

        if (illusions.Count == 0)
        {
            ContinueToNextIteration(ent, target, mapUid, args, iteration);
            return;
        }

        _activeIllusions[ent.Owner] = illusions;

        var damage = args.IllusionDamage;

        Timer.Spawn(TimeSpan.FromSeconds(args.PreDashDelay), () =>
        {
            if (!Exists(ent.Owner) || !Exists(target))
            {
                CleanupIllusions(ent.Owner);
                ContinueToNextIteration(ent, target, mapUid, args, iteration);
                return;
            }

            StartIllusionDashForAll(illusions, targetCoords, damage);
            PerformDash(ent, targetCoords, ScaleDamage(damage, 2), 0.1f, false);

            Timer.Spawn(TimeSpan.FromSeconds(1.5f), () =>
            {
                CleanupIllusions(ent.Owner);

                var nextIteration = iteration + 1;
                if (nextIteration < 3)
                {
                    PerformIllusionDashIteration(ent, target, mapUid, args, nextIteration);
                }
                else
                {
                    if (Exists(ent.Owner) && Exists(target))
                    {
                        TriggerTripleDashAfterIllusion(ent, target, args.IllusionDamage);
                    }
                    else
                    {
                        _npc.WakeNPC(ent.Owner);
                        SetHTNTarget(ent, target);
                    }
                }
            });
        });
    }

    private void ContinueToNextIteration(Entity<BubblegumBossComponent> ent, EntityUid target,
        EntityUid mapUid, BubblegumIllusionDashActionEvent args, int currentIteration)
    {
        var nextIteration = currentIteration + 1;

        if (nextIteration < 3)
        {
            PerformIllusionDashIteration(ent, target, mapUid, args, nextIteration);
        }
        else
        {
            if (Exists(ent.Owner) && Exists(target))
            {
                TriggerTripleDashAfterIllusion(ent, target, args.IllusionDamage);
            }
            else
            {
                _npc.WakeNPC(ent.Owner);
                SetHTNTarget(ent, target);
            }
        }
    }

    private void TriggerTripleDashAfterIllusion(Entity<BubblegumBossComponent> ent, EntityUid target,
        DamageSpecifier illusionDamage)
    {
        var tripleDashEvent = new BubblegumTripleDashActionEvent
        {
            Target = target,
            DashDamage = new DamageSpecifier(illusionDamage)
            {
                DamageDict = illusionDamage.DamageDict.ToDictionary(
                    x => x.Key,
                    x => x.Value * 2)
            },
            DashDistance = 5f,
            MoveSpeed = 0.05f,
            UseSineWaveForLast = true,
            DashDelays = new List<float> { 0.9f, 0.6f, 0.3f },
            Performer = ent.Owner
        };

        OnTripleDash(ent, ref tripleDashEvent);
    }

    #endregion

    #region Blood Dive

    private void OnBloodDiveAction(Entity<BubblegumBossComponent> ent, ref BubblegumBloodDiveActionEvent args)
    {
        if (_timing.CurTime < ent.Comp.NextBloodDiveTime)
            return;

        if (!Exists(args.Target) || _mobState.IsDead(ent.Owner))
            return;

        var target = args.Target;
        var targetCoords = Transform(target).Coordinates;
        var mapUid = _transform.GetMap(ent.Owner);
        if (mapUid == null)
            return;

        var diveCoords = FindBloodDiveCoordinates(ent, targetCoords, mapUid.Value, args);

        if (diveCoords == null)
            return;

        if (!IsValidSpawnPosition(diveCoords.Value))
        {
            var safeCoords = FindSafePositionNear(ent, diveCoords.Value);
            if (safeCoords == null)
                return;
            diveCoords = safeCoords;
        }

        args.Handled = true;
        _npcActions.LockActions(ent.Owner, TimeSpan.FromSeconds(args.PreDiveDelay + 0.25f));
        _npc.SleepNPC(ent.Owner);

        Timer.Spawn(TimeSpan.FromSeconds(args.PreDiveDelay), () =>
        {
            if (!Exists(ent.Owner) || _mobState.IsDead(ent.Owner))
                return;

            _transform.SetCoordinates(ent.Owner, diveCoords.Value);
            SpawnBloodPool(ent.Owner);
            TriggerRage(ent.Owner, ent.Comp);
            FinishAttackSequence(ent, target);
        });

        ent.Comp.NextBloodDiveTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.BloodDiveCooldown);
    }

    private EntityCoordinates? FindBloodDiveCoordinates(Entity<BubblegumBossComponent> ent, EntityCoordinates targetCoords,
        EntityUid mapUid, BubblegumBloodDiveActionEvent args)
    {
        // Paradise only permits blood warp while Bubblegum is standing on a blood pool. Do not turn a rejected
        // warp into a free random teleport; returning null lets the selector immediately try a charge instead.
        var sourcePools = _lookup.GetEntitiesInRange<PuddleComponent>(Transform(ent).Coordinates, 1f)
            .Where(p => HasBloodPuddle(p.Owner))
            .ToList();
        if (sourcePools.Count == 0)
            return null;

        var targetWorld = _transform.ToMapCoordinates(targetCoords).Position;
        var bloodPuddles = _lookup
            .GetEntitiesInRange<PuddleComponent>(targetCoords, args.DiveRange)
            .Where(p => HasBloodPuddle(p.Owner))
            .Where(p =>
            {
                var distance = Vector2.Distance(targetWorld, _transform.GetWorldPosition(p.Owner));
                return distance > Math.Max(0f, args.DiveRange - 1f) && distance <= args.DiveRange;
            })
            .ToList();

        if (bloodPuddles.Count > 0)
        {
            var selectedPuddle = _random.Pick(bloodPuddles);
            var puddleCoords = Transform(selectedPuddle.Owner).Coordinates;
            var puddlePos = puddleCoords.Position;
            var tileCenter = GetTileCenter(mapUid, puddlePos);
            return WorldCoordinates(mapUid, tileCenter);
        }

        return null;
    }

    #endregion

    #region Pentagram Dash

    private void OnPentagramDashAction(Entity<BubblegumBossComponent> ent, ref BubblegumPentagramDashActionEvent args)
    {
        if (ent.Comp.CurrentPhase != BubblegumPhase.Enraged ||
            !Exists(args.Target) ||
            _mobState.IsDead(ent.Owner))
            return;

        args.Handled = true;
        _npcActions.LockActions(ent.Owner, TimeSpan.FromSeconds(3));
        _npc.SleepNPC(ent.Owner);
        SpawnBloodPool(ent);

        var target = args.Target;

        var targetCoords = Transform(target).Coordinates;
        var mapUid = _transform.GetMap(ent.Owner);
        if (mapUid == null)
        {
            _npc.WakeNPC(ent.Owner);
            SetHTNTarget(ent, target);
            return;
        }

        var markerCoords = targetCoords;
        if (!IsValidSpawnPosition(markerCoords))
        {
            var safeCoords = FindSafePositionNear(ent, markerCoords);
            if (safeCoords == null)
            {
                _npc.WakeNPC(ent.Owner);
                SetHTNTarget(ent, target);
                return;
            }
            markerCoords = safeCoords.Value;
        }

        Spawn(ent.Comp.DashMarker, markerCoords);

        // Paradise's mass hallucination charge uses six positions: Bubblegum plus five hallucinations.
        const int totalEntities = 6;
        var positions = GetCircularPositions(targetCoords, mapUid.Value, totalEntities, args.PlacementRadius);
        if (positions.Count < totalEntities)
        {
            _npc.WakeNPC(ent.Owner);
            SetHTNTarget(ent, target);
            return;
        }

        var bossIndex = _random.Next(positions.Count);
        var illusions = SpawnIllusionCircle(ent, target, targetCoords, positions, bossIndex,
            args.IllusionPrototype, args.IllusionDamage);

        if (illusions.Count == 0)
        {
            _npc.WakeNPC(ent.Owner);
            SetHTNTarget(ent, target);
            return;
        }

        _activeIllusions[ent.Owner] = illusions;

        var damage = args.IllusionDamage;
        Timer.Spawn(TimeSpan.FromSeconds(args.PreDashDelay), () =>
        {
            if (!Exists(ent.Owner) || !Exists(target))
            {
                CleanupIllusions(ent.Owner);
                _npc.WakeNPC(ent.Owner);
                SetHTNTarget(ent, target);
                return;
            }

            StartIllusionDashForAll(illusions, targetCoords, damage);
            PerformDash(ent, targetCoords, ScaleDamage(damage, 2), 0.1f, true);

            Timer.Spawn(TimeSpan.FromSeconds(1.5f), () =>
            {
                CleanupIllusions(ent.Owner);
                if (Exists(ent.Owner))
                {
                    _npc.WakeNPC(ent.Owner);
                    SetHTNTarget(ent, target);
                }
            });
        });
    }

    #endregion

    #region Chaotic Illusion Dash

    private void OnChaoticIllusionDashAction(Entity<BubblegumBossComponent> ent, ref BubblegumChaoticIllusionDashActionEvent args)
    {
        if (ent.Comp.CurrentPhase != BubblegumPhase.Enraged ||
            !Exists(args.Target) ||
            _mobState.IsDead(ent.Owner))
            return;

        var target = args.Target;
        args.Handled = true;
        _npcActions.LockActions(ent.Owner, TimeSpan.FromSeconds(12));
        CleanupIllusions(ent.Owner);

        var action = args;
        for (int wave = 0; wave < 5; wave++)
        {
            var currentWave = wave;
            var waveDelay = wave * 2.3f;

            Timer.Spawn(TimeSpan.FromSeconds(waveDelay), () =>
            {
                if (!Exists(ent.Owner) || !Exists(target) || _mobState.IsDead(ent.Owner))
                    return;

                ExecuteChaoticWave(ent, target, action, currentWave);
            });
        }
    }

    private void ExecuteChaoticWave(Entity<BubblegumBossComponent> ent, EntityUid target,
        BubblegumChaoticIllusionDashActionEvent args, int waveIndex)
    {
        CleanupIllusions(ent.Owner);

        _npc.SleepNPC(ent.Owner);
        SpawnBloodPool(ent);

        var mapUid = _transform.GetMap(ent.Owner);
        if (mapUid == null)
        {
            _npc.WakeNPC(ent.Owner);
            SetHTNTarget(ent, target);
            return;
        }

        var bossMarker = GenerateRandomMarker(target, mapUid.Value, args.PlacementRadius);
        var illusionMarkers = new List<EntityCoordinates>();

        if (!IsValidSpawnPosition(bossMarker))
        {
            var safeCoords = FindSafePositionNear(ent, bossMarker);
            if (safeCoords == null)
            {
                _npc.WakeNPC(ent.Owner);
                SetHTNTarget(ent, target);
                return;
            }
            bossMarker = safeCoords.Value;
        }

        Spawn(ent.Comp.DashMarker, bossMarker);
        var illusions = SpawnChaoticIllusions(ent, target, mapUid.Value, args, illusionMarkers);

        if (illusions.Count == 0)
        {
            _npc.WakeNPC(ent.Owner);
            SetHTNTarget(ent, target);
            return;
        }

        _activeIllusions[ent.Owner] = illusions;

        Timer.Spawn(TimeSpan.FromSeconds(args.PreDashDelay), () =>
        {
            if (!Exists(ent.Owner) || !Exists(target))
            {
                CleanupIllusions(ent.Owner);
                _npc.WakeNPC(ent.Owner);
                SetHTNTarget(ent, target);
                return;
            }

            StartChaoticIllusionAttacks(illusions, illusionMarkers, args.IllusionDamage);
            PerformDash(ent, bossMarker, ScaleDamage(args.IllusionDamage, 2), 0.1f, waveIndex == 4);

            Timer.Spawn(TimeSpan.FromSeconds(1f), () =>
            {
                CleanupIllusions(ent.Owner);
                if (Exists(ent.Owner) && waveIndex == 4)
                {
                    _npc.WakeNPC(ent.Owner);
                    SetHTNTarget(ent, target);
                }
            });
        });
    }

    private EntityCoordinates GenerateRandomMarker(EntityUid target, EntityUid mapUid, float placementRadius)
    {
        var angle = _random.NextFloat(0, MathF.PI * 2);
        var distance = _random.NextFloat(1f, placementRadius);
        var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;

        var targetPos = _transform.GetWorldPosition(target);
        var markerPos = targetPos + offset;
        var centerPos = GetTileCenter(mapUid, markerPos);

        return WorldCoordinates(mapUid, centerPos);
    }

    private List<EntityUid> SpawnChaoticIllusions(Entity<BubblegumBossComponent> ent, EntityUid target,
        EntityUid mapUid, BubblegumChaoticIllusionDashActionEvent args, List<EntityCoordinates> illusionMarkers)
    {
        var illusions = new List<EntityUid>();
        var bossPos = _transform.GetWorldPosition(ent);
        var bossTile = GetTileCenter(mapUid, bossPos);

        for (int i = 0; i < args.IllusionCount; i++)
        {
            var marker = GenerateRandomMarker(target, mapUid, args.PlacementRadius);

            if (!IsValidSpawnPosition(marker))
            {
                var safeCoords = FindValidPositionNear(marker, args.PlacementRadius);
                if (safeCoords == null)
                    continue;
                marker = safeCoords.Value;
            }

            illusionMarkers.Add(marker);

            Spawn(ent.Comp.DashMarker, marker);
            for (int attempts = 0; attempts < 30; attempts++)
            {
                var angle = _random.NextFloat(0, MathF.PI * 2);
                var distance = _random.NextFloat(2f, args.PlacementRadius);
                var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;

                var targetPos = _transform.GetWorldPosition(target);
                var illusionPos = targetPos + offset;
                var illusionTile = GetTileCenter(mapUid, illusionPos);
                var illusionCoords = WorldCoordinates(mapUid, illusionTile);

                if (!CanSpawnAt(illusionCoords) || illusionTile == bossTile)
                    continue;

                var direction = NormalizeOrZero(marker.Position - illusionTile);
                var illusion = SpawnAttachedTo(args.IllusionPrototype, illusionCoords, rotation: GetDirectionRotation(direction));

                if (TryComp<BubblegumIllusionComponent>(illusion, out var illusionComp))
                {
                    illusionComp.Master = ent.Owner;
                    illusionComp.Target = target;
                    illusionComp.TargetPosition = marker;
                    illusionComp.Damage = args.IllusionDamage;
                    illusions.Add(illusion);
                }
                break;
            }
        }

        return illusions;
    }

    private void StartChaoticIllusionAttacks(List<EntityUid> illusions, List<EntityCoordinates> markers, DamageSpecifier damage)
    {
        var damagedTargets = new HashSet<EntityUid>();

        for (int i = 0; i < illusions.Count; i++)
        {
            var illusion = illusions[i];
            var marker = markers[i];

            if (!Exists(illusion))
                continue;

            var randomDelay = _random.NextFloat(0f, 0.3f);
            Timer.Spawn(TimeSpan.FromSeconds(randomDelay), () =>
            {
                if (Exists(illusion))
                    StartIllusionDash(illusion, marker, damage, damagedTargets);
            });
        }
    }

    #endregion

    #region Dash Execution

    private void PerformDash(EntityUid uid, EntityCoordinates target, DamageSpecifier damage,
        float moveSpeed, bool isLastDash, Action? onComplete = null)
    {
        if (!IsValidSpawnPosition(target))
        {
            onComplete?.Invoke();
            return;
        }

        var startPos = _transform.GetWorldPosition(uid);
        var targetPos = target.Position;

        var map = _transform.GetMap(uid);
        if (map == null)
        {
            onComplete?.Invoke();
            return;
        }

        var startTile = GetTileCenter(map.Value, startPos);
        var targetTile = target.Position;

        var direction = NormalizeOrZero(targetTile - startTile);
        var distance = Vector2.Distance(startTile, targetTile);
        var steps = Math.Max(1, (int)Math.Ceiling(distance));

        var mapUid = _transform.GetMap(uid);
        if (mapUid == null)
        {
            onComplete?.Invoke();
            return;
        }

        _dashDamagedTargets.Clear();

        if (!TryComp<BubblegumBossComponent>(uid, out var bossComp))
        {
            onComplete?.Invoke();
            return;
        }

        InitializeDash(uid, bossComp, mapUid.Value, startTile, direction);

        var stepCounter = new StepCounter { CompletedSteps = 0, TotalSteps = steps };

        for (int step = 1; step <= steps; step++)
        {
            ScheduleDashStep(uid, bossComp, mapUid.Value, startTile, direction, step, moveSpeed,
                damage, stepCounter, isLastDash, onComplete);
        }
    }

    private void InitializeDash(EntityUid uid, BubblegumBossComponent bossComp, EntityUid mapUid,
        Vector2 startTile, Vector2 direction)
    {
        SpawnBloodPool(uid);
        SpawnAttachedTo(bossComp.DashTrail, WorldCoordinates(mapUid, startTile),
            rotation: GetDirectionRotation(direction));
    }

    private void ScheduleDashStep(EntityUid uid, BubblegumBossComponent bossComp, EntityUid mapUid,
        Vector2 startTile, Vector2 direction, int step, float moveSpeed, DamageSpecifier damage,
        StepCounter stepCounter, bool isLastDash, Action? onComplete)
    {
        var currentStep = step;

        Timer.Spawn(TimeSpan.FromSeconds(currentStep * moveSpeed), () =>
        {
            if (!Exists(uid) || _mobState.IsDead(uid))
            {
                if (currentStep == stepCounter.TotalSteps)
                    onComplete?.Invoke();
                return;
            }

            var stepVector = direction * currentStep;
            var currentPos = startTile + stepVector;
            var tileCenter = GetTileCenter(mapUid, currentPos);
            var currentCoords = WorldCoordinates(mapUid, tileCenter);

            if (!IsValidSpawnPosition(currentCoords))
            {
                stepCounter.CompletedSteps++;
                if (stepCounter.CompletedSteps >= stepCounter.TotalSteps)
                    HandleDashCompletion(uid, isLastDash, onComplete);
                return;
            }

            _transform.SetCoordinates(uid, currentCoords);
            SpawnBloodPool(uid);

            SpawnAttachedTo(bossComp.DashTrail, currentCoords,
                rotation: GetDirectionRotation(direction));

            CheckDashDamage(uid, currentCoords, damage);
            _audio.PlayPvs(bossComp.DashSound, uid);

            stepCounter.CompletedSteps++;

            if (stepCounter.CompletedSteps >= stepCounter.TotalSteps)
                HandleDashCompletion(uid, isLastDash, onComplete);
        });
    }

    private void HandleDashCompletion(EntityUid uid, bool isLastDash, Action? onComplete)
    {
        Timer.Spawn(TimeSpan.FromSeconds(0.1f), () =>
        {
            onComplete?.Invoke();

            if (isLastDash)
            {
                Timer.Spawn(TimeSpan.FromSeconds(0.3f), () =>
                {
                    if (Exists(uid))
                        _npc.WakeNPC(uid);
                });
            }
        });
    }

    private void CheckDashDamage(EntityUid uid, EntityCoordinates coords, DamageSpecifier damage)
    {
        var entities = _lookup.GetEntitiesInRange<MobStateComponent>(coords, 1f, LookupFlags.Uncontained);
        foreach (var entity in entities)
        {
            if (entity.Owner == uid || HasComp<BubblegumBossComponent>(entity.Owner))
                continue;

            if (_dashDamagedTargets.Contains(entity.Owner))
                continue;

            if (_mobState.IsIncapacitated(entity.Owner))
            {
                _gibbing.Gib(entity.Owner);
                _dashDamagedTargets.Add(entity.Owner);
                continue;
            }

            if (_damage.TryChangeDamage(entity.Owner, damage))
                _dashDamagedTargets.Add(entity.Owner);
        }
    }

    #endregion

    #region Illusion System

    private void StartIllusionDash(EntityUid uid, EntityCoordinates target, DamageSpecifier damage,
        HashSet<EntityUid> damagedTargets)
    {
        if (!TryComp<BubblegumIllusionComponent>(uid, out var illusion))
            return;

        if (!IsValidSpawnPosition(target))
            return;

        illusion.TargetPosition = target;

        var startPos = _transform.GetWorldPosition(uid);
        var targetPos = target.Position;

        var map = _transform.GetMap(uid);
        if (map == null)
            return;

        var startTile = GetTileCenter(map.Value, startPos);
        var targetTile = GetTileCenter(map.Value, targetPos);

        var direction = NormalizeOrZero(targetTile - startTile);
        var distance = Vector2.Distance(startTile, targetTile);
        var steps = Math.Max(1, (int)Math.Ceiling(distance));

        illusion.TotalSteps = steps;

        var mapUid = _transform.GetMap(uid);
        if (mapUid == null)
            return;

        for (int step = 1; step <= steps; step++)
        {
            ScheduleIllusionDashStep(uid, illusion, mapUid.Value, startTile, direction, step, damage, damagedTargets);
        }
    }

    private void ScheduleIllusionDashStep(EntityUid uid, BubblegumIllusionComponent illusion, EntityUid mapUid,
        Vector2 startTile, Vector2 direction, int step, DamageSpecifier damage, HashSet<EntityUid> damagedTargets)
    {
        var currentStep = step;

        Timer.Spawn(TimeSpan.FromSeconds(currentStep * 0.1f), () =>
        {
            if (!Exists(uid))
                return;

            var stepVector = direction * currentStep;
            var currentPos = startTile + stepVector;
            var tileCenter = GetTileCenter(mapUid, currentPos);
            var currentCoords = WorldCoordinates(mapUid, tileCenter);

            if (!IsValidSpawnPosition(currentCoords))
                return;

            _transform.SetCoordinates(uid, currentCoords);

            if (TryComp<BubblegumBossComponent>(illusion.Master, out var bossComp))
                SpawnAttachedTo(bossComp.DashTrail, currentCoords,
                    rotation: GetDirectionRotation(direction));

            CheckIllusionDashDamage(uid, illusion.Master, currentCoords, damage, damagedTargets);

            illusion.CurrentStep = currentStep;
        });
    }

    private void CheckIllusionDashDamage(EntityUid uid, EntityUid? master, EntityCoordinates coords,
        DamageSpecifier damage, HashSet<EntityUid> damagedTargets)
    {
        var entities = _lookup.GetEntitiesInRange<MobStateComponent>(coords, 1f, LookupFlags.Uncontained);
        foreach (var entity in entities)
        {
            if (entity.Owner == uid || entity.Owner == master)
                continue;

            if (damagedTargets.Contains(entity.Owner))
                continue;

            if (_damage.TryChangeDamage(entity.Owner, damage, origin: master))
                damagedTargets.Add(entity.Owner);
        }
    }

    #endregion

    #region Passive Hand Attack

    private void UpdatePassiveHandAttack()
    {
        var query = EntityQueryEnumerator<BubblegumBossComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (_mobState.IsDead(uid))
                continue;

            if (_timing.CurTime < comp.NextPassiveHandTime)
                continue;

            comp.NextPassiveHandTime = _timing.CurTime + TimeSpan.FromSeconds(PassiveHandInterval);
            var playersOnBlood = FindPlayersOnBlood((uid, comp, xform));

            foreach (var player in playersOnBlood)
            {
                if (_random.Prob(PassiveHandChance))
                    SpawnBloodHand(player, comp);
            }
        }
    }

    private HashSet<EntityUid> FindPlayersOnBlood(Entity<BubblegumBossComponent, TransformComponent> boss)
    {
        var playersOnBlood = new HashSet<EntityUid>();
        PruneBloodPools(boss.Comp1);
        var bossMap = _transform.GetMap(boss.Owner);
        if (bossMap == null)
            return playersOnBlood;

        var bossPosition = _transform.GetWorldPosition(boss.Owner);
        foreach (var puddle in boss.Comp1.ActiveBloodPools)
        {
            if (_transform.GetMap(puddle) != bossMap ||
                !HasBloodPuddle(puddle) ||
                Vector2.DistanceSquared(bossPosition, _transform.GetWorldPosition(puddle)) >
                PassiveHandRadius * PassiveHandRadius)
                continue;

            var puddleCoords = Transform(puddle).Coordinates;
            var entitiesOnPuddle = _lookup.GetEntitiesInRange<ActorComponent>(puddleCoords, 0.5f, LookupFlags.Uncontained)
                .Where(a => HasComp<MobStateComponent>(a.Owner) && !HasComp<GhostComponent>(a.Owner));

            foreach (var entity in entitiesOnPuddle)
                playersOnBlood.Add(entity.Owner);
        }

        return playersOnBlood;
    }

    private void SpawnBloodHand(EntityUid target, BubblegumBossComponent comp)
    {
        var targetCoords = Transform(target).Coordinates;
        Spawn(_random.Prob(0.5f) ? comp.LeftHandEffect : comp.RightHandEffect, targetCoords);

        // Paradise resolves the hand four deciseconds after its warning. Relying on a very short-lived
        // collision fixture made the SS14 version miss stationary players depending on broadphase timing.
        Timer.Spawn(TimeSpan.FromSeconds(0.4f), () =>
        {
            if (!Exists(target) || _mobState.IsDead(target) || !IsStandingOnBlood(target))
                return;

            if (_mobState.IsIncapacitated(target))
            {
                _gibbing.Gib(target);
                return;
            }

            var damage = comp.CurrentPhase == BubblegumPhase.Enraged
                ? comp.EnragedBloodHandDamage
                : comp.BloodHandDamage;
            _damage.TryChangeDamage(target, damage);
        });
    }

    private void UpdatePassiveBloodWarp()
    {
        var query = EntityQueryEnumerator<BubblegumBossComponent, NPCUseActionsOnTargetComponent, HTNComponent>();
        while (query.MoveNext(out var uid, out var boss, out var actions, out var htn))
        {
            if (_mobState.IsDead(uid) ||
                _timing.CurTime < boss.NextBloodDiveAttemptTime ||
                _timing.CurTime < boss.NextBloodDiveTime ||
                _timing.CurTime < actions.ActionLockUntil)
            {
                continue;
            }

            boss.NextBloodDiveAttemptTime = _timing.CurTime + TimeSpan.FromSeconds(1);
            if (!htn.Blackboard.TryGetValue<EntityUid>(boss.TargetKey, out var target, EntityManager) ||
                !Exists(target) ||
                !_random.Prob(boss.CurrentPhase == BubblegumPhase.Enraged ? 0.45f : 0.25f))
            {
                continue;
            }

            var warp = new BubblegumBloodDiveActionEvent
            {
                Performer = uid,
                Target = target,
            };
            OnBloodDiveAction((uid, boss), ref warp);
        }
    }

    private bool IsStandingOnBlood(EntityUid target)
        => _lookup.GetEntitiesInRange<PuddleComponent>(Transform(target).Coordinates, 0.5f)
            .Any(puddle => HasBloodPuddle(puddle.Owner));

    private bool HasBloodPuddle(EntityUid uid)
    {
        if (!TryComp<PuddleComponent>(uid, out var puddle))
            return false;

        if (!TryComp(uid, out ContainerManagerComponent? containerManager))
            return false;

        if (!containerManager.Containers.TryGetValue("solution@puddle", out var container))
            return false;

        return container.ContainedEntities.Any(containedEntity =>
            TryComp(containedEntity, out SolutionComponent? solutionComponent) &&
            solutionComponent.Solution.Contents.Any(r =>
                r.Reagent.Prototype == "Blood" || r.Reagent.Prototype == "CopperBlood"));
    }

    #endregion

    #region Blood Pool

    private void SpawnBloodPool(EntityUid uid)
    {
        if (!TryComp<BubblegumBossComponent>(uid, out var comp))
            return;

        var mapUid = _transform.GetMap(uid);
        if (mapUid == null)
            return;

        var centerPos = _transform.GetWorldPosition(uid);
        var centerTile = GetTileCenter(mapUid.Value, centerPos);

        // One puddle per occupied tile is enough to communicate the dash trail. The previous 3x3 burst
        // multiplied every dash step into nine solution/container entities and caused increasingly large
        // lookup spikes during a fight.
        TrySpawnBloodPoolAt(comp, mapUid.Value, centerTile);
    }

    private void TrySpawnBloodPoolAt(BubblegumBossComponent comp, EntityUid mapUid, Vector2 centerTile)
    {
        var bloodCoords = WorldCoordinates(mapUid, centerTile);

        if (!IsValidMapPosition(mapUid, centerTile))
            return;

        var existingPuddles = _lookup.GetEntitiesInRange<PuddleComponent>(bloodCoords, 0.1f);
        var hasBlood = existingPuddles.Any(p => HasBloodPuddle(p.Owner));

        if (hasBlood)
            return;

        comp.ActiveBloodPools.Add(Spawn(comp.BloodEffect, bloodCoords));
        PruneBloodPools(comp);

        while (comp.ActiveBloodPools.Count > Math.Max(1, comp.MaximumBloodPools))
        {
            var oldest = comp.ActiveBloodPools[0];
            comp.ActiveBloodPools.RemoveAt(0);
            if (Exists(oldest))
                QueueDel(oldest);
        }
    }

    private void PruneBloodPools(BubblegumBossComponent comp)
        => comp.ActiveBloodPools.RemoveAll(puddle => !Exists(puddle));

    #endregion

    #region Utility Methods

    private List<EntityCoordinates> GetCircularPositions(EntityCoordinates center, EntityUid mapUid,
        int count, float radius)
    {
        var positions = new List<EntityCoordinates>();

        for (int i = 0; i < count; i++)
        {
            var angle = i * (MathF.PI * 2 / count);
            var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            var pos = center.Offset(offset);

            if (!CanSpawnAt(pos))
            {
                var safePos = FindValidPositionNear(pos, radius);
                if (safePos == null)
                    continue;
                pos = safePos.Value;
            }

            positions.Add(pos);
        }

        return positions;
    }

    private List<EntityUid> SpawnIllusionCircle(Entity<BubblegumBossComponent> ent, EntityUid target,
        EntityCoordinates targetCoords, List<EntityCoordinates> positions, int bossIndex,
        EntProtoId illusionPrototype, DamageSpecifier damage)
    {
        var illusions = new List<EntityUid>();

        for (int i = 0; i < positions.Count; i++)
        {
            if (i == bossIndex)
            {
                PlaceBossAtPosition(ent, target, positions[i]);
            }
            else
            {
                var illusion = SpawnIllusion(ent, target, targetCoords, positions[i], illusionPrototype, damage);
                if (illusion != null)
                    illusions.Add(illusion.Value);
            }
        }

        return illusions;
    }

    private void PlaceBossAtPosition(Entity<BubblegumBossComponent> ent, EntityUid target, EntityCoordinates position)
    {
        if (!IsValidSpawnPosition(position))
        {
            var safePos = FindSafePositionNear(ent, position);
            if (safePos == null)
                return;
            position = safePos.Value;
        }

        _transform.SetCoordinates(ent.Owner, position);

        if (TryComp<BubblegumBossComponent>(ent.Owner, out var bossComp))
        {
            var direction = NormalizeOrZero(_transform.GetWorldPosition(target) - _transform.GetWorldPosition(ent));
            SpawnAttachedTo(bossComp.DashTrail, position, rotation: GetDirectionRotation(direction));
            _audio.PlayPvs(bossComp.DashSound, ent.Owner);
        }
    }

    private EntityUid? SpawnIllusion(Entity<BubblegumBossComponent> ent, EntityUid target,
        EntityCoordinates targetCoords, EntityCoordinates position, EntProtoId prototype,
        DamageSpecifier damage)
    {
        if (!IsValidSpawnPosition(position))
            return null;

        var direction = NormalizeOrZero(targetCoords.Position - position.Position);
        var illusion = SpawnAttachedTo(prototype, position, rotation: GetDirectionRotation(direction));
        if (TryComp<BubblegumIllusionComponent>(illusion, out var illusionComp))
        {
            illusionComp.Master = ent.Owner;
            illusionComp.Target = target;
            illusionComp.TargetPosition = targetCoords;
            illusionComp.Damage = damage;
            return illusion;
        }

        return null;
    }

    private void StartIllusionDashForAll(List<EntityUid> illusions, EntityCoordinates targetCoords, DamageSpecifier damage)
    {
        var damagedTargets = new HashSet<EntityUid>();

        foreach (var illusion in illusions)
        {
            if (Exists(illusion))
                StartIllusionDash(illusion, targetCoords, damage, damagedTargets);
        }
    }

    private EntityCoordinates? FindValidPositionNear(EntityCoordinates center, float maxDistance)
    {
        for (int i = 0; i < 10; i++)
        {
            var angle = _random.NextFloat(0, MathF.PI * 2);
            var distance = _random.NextFloat(1f, maxDistance);
            var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
            var testCoords = center.Offset(offset);

            if (CanSpawnAt(testCoords))
                return testCoords;
        }
        return null;
    }

    private bool CanSpawnAt(EntityCoordinates coords)
    {
        var mapCoordinates = _transform.ToMapCoordinates(coords);
        if (!_map.TryFindGridAt(mapCoordinates, out var gridUid, out var grid))
            return false;

        var gridCoordinates = _transform.ToCoordinates(gridUid, mapCoordinates);
        var tilePos = _map.CoordinatesToTile(gridUid, grid, gridCoordinates);
        if (!_map.TryGetTileRef(gridUid, grid, tilePos, out var tileRef))
            return false;

        return !_turf.IsTileBlocked(tileRef, CollisionGroup.Impassable);
    }

    private Vector2 GetTileCenter(EntityUid mapUid, Vector2 position)
        => GetTileCenter(Transform(mapUid).MapID, position);

    private Vector2 GetTileCenter(MapId mapId, Vector2 position)
    {
        var mapCoordinates = new MapCoordinates(position, mapId);
        if (!_map.TryFindGridAt(mapCoordinates, out var gridUid, out var grid))
            return position;

        var gridCoordinates = _transform.ToCoordinates(gridUid, mapCoordinates);
        var tilePos = _map.CoordinatesToTile(gridUid, grid, gridCoordinates);
        return _map.GridTileToWorld(gridUid, grid, tilePos).Position;
    }

    private float GetHealthRatio(EntityUid uid)
    {
        var totalDamage = _damage.GetTotalDamage(uid);
        if (!_threshold.TryGetThresholdForState(uid, MobState.Dead, out var threshold))
            return 1f;

        return 1f - (float)(totalDamage / threshold.Value.Double());
    }

    private bool IsValidMapPosition(EntityUid mapUid, Vector2 position)
    {
        var mapCoordinates = new MapCoordinates(position, Transform(mapUid).MapID);
        if (!_map.TryFindGridAt(mapCoordinates, out var gridUid, out var grid))
            return false;

        var gridCoordinates = _transform.ToCoordinates(gridUid, mapCoordinates);
        var tilePos = _map.CoordinatesToTile(gridUid, grid, gridCoordinates);
        if (!_map.TryGetTileRef(gridUid, grid, tilePos, out var tileRef))
            return false;

        return !_turf.IsTileBlocked(tileRef, CollisionGroup.Impassable);
    }

    private bool IsValidSpawnPosition(EntityCoordinates coords)
    {
        var mapCoordinates = _transform.ToMapCoordinates(coords);
        if (!_map.TryFindGridAt(mapCoordinates, out var gridUid, out var grid))
            return false;

        var gridCoordinates = _transform.ToCoordinates(gridUid, mapCoordinates);
        var tilePos = _map.CoordinatesToTile(gridUid, grid, gridCoordinates);
        if (!_map.TryGetTileRef(gridUid, grid, tilePos, out var tileRef))
            return false;

        return !_turf.IsTileBlocked(tileRef, CollisionGroup.Impassable);
    }

    private EntityCoordinates? FindSafePositionNear(Entity<BubblegumBossComponent> ent, EntityCoordinates original)
    {
        var mapUid = _transform.GetMap(original);
        if (mapUid == null)
            return null;

        for (float radius = 0.5f; radius <= 10f; radius += 0.5f)
        {
            for (int angle = 0; angle < 360; angle += 45)
            {
                var rad = MathF.PI * angle / 180f;
                var offset = new Vector2(MathF.Cos(rad), MathF.Sin(rad)) * radius;
                var testPos = original.Position + offset;
                var testCoords = WorldCoordinates(mapUid.Value, testPos);

                if (IsValidSpawnPosition(testCoords))
                    return testCoords;
            }
        }

        return FindNearestGridPosition(ent, mapUid.Value);
    }

    private EntityCoordinates? FindNearestGridPosition(Entity<BubblegumBossComponent> ent, EntityUid mapUid)
    {
        var gridQuery = EntityQueryEnumerator<MapGridComponent>();
        while (gridQuery.MoveNext(out var gridUid, out var grid))
        {
            if (Transform(gridUid).ParentUid != mapUid)
                continue;

            var gridCenter = _transform.GetWorldPosition(gridUid);
            var coords = WorldCoordinates(mapUid, gridCenter);

            if (IsValidSpawnPosition(coords))
                return coords;
        }

        return null;
    }

    private Vector2 FindNearestValidPosition(EntityUid mapUid, Vector2 position)
    {
        var gridQuery = EntityQueryEnumerator<MapGridComponent>();
        while (gridQuery.MoveNext(out var gridUid, out _))
        {
            if (Transform(gridUid).ParentUid != mapUid)
                continue;

            var worldBounds = _transform.GetWorldPosition(gridUid);
            var gridRadius = 10f;

            for (float radius = 0.5f; radius <= gridRadius; radius += 0.5f)
            {
                for (int angle = 0; angle < 360; angle += 30)
                {
                    var rad = MathF.PI * angle / 180f;
                    var offset = new Vector2(MathF.Cos(rad), MathF.Sin(rad)) * radius;
                    var testPos = worldBounds + offset;

                    var testCoords = WorldCoordinates(mapUid, testPos);
                    if (CanSpawnAt(testCoords))
                        return testPos;
                }
            }
        }

        return position;
    }

    private void CleanupIllusions(EntityUid boss)
    {
        if (_activeIllusions.TryGetValue(boss, out var illusions))
        {
            foreach (var illusion in illusions)
            {
                if (Exists(illusion))
                    QueueDel(illusion);
            }
            _activeIllusions.Remove(boss);
        }
    }

    private Angle GetDirectionRotation(Vector2 direction)
    {
        return direction == Vector2.Zero ? Angle.Zero
            : Angle.FromWorldVec(direction);
    }

    private static Vector2 NormalizeOrZero(Vector2 vector)
    {
        return vector.LengthSquared() <= float.Epsilon
            ? Vector2.Zero
            : Vector2.Normalize(vector);
    }

    private EntityCoordinates WorldCoordinates(EntityUid mapUid, Vector2 worldPosition)
    {
        var mapCoordinates = new MapCoordinates(worldPosition, Transform(mapUid).MapID);

        // Map-relative coordinates are suitable for free-floating entities, but effects such as puddles
        // anchor themselves during initialization. Give every attack effect grid-relative coordinates when
        // there is a grid under the requested world position so anchoring, lookup and collision all agree.
        if (_map.TryFindGridAt(mapCoordinates, out var gridUid, out _))
            return _transform.ToCoordinates(gridUid, mapCoordinates);

        return _transform.ToCoordinates(mapCoordinates);
    }

    private static DamageSpecifier ScaleDamage(DamageSpecifier damage, int multiplier)
    {
        return new DamageSpecifier(damage)
        {
            DamageDict = damage.DamageDict.ToDictionary(
                entry => entry.Key,
                entry => entry.Value * multiplier)
        };
    }

    private void FinishAttackSequence(Entity<BubblegumBossComponent> boss, EntityUid target)
    {
        if (!Exists(boss.Owner) || _mobState.IsDead(boss.Owner))
            return;

        _npc.WakeNPC(boss.Owner);
        if (Exists(target))
            SetHTNTarget(boss, target);
    }

    private void SetHTNTarget(Entity<BubblegumBossComponent> boss, EntityUid target)
    {
        if (!TryComp<HTNComponent>(boss, out var htn))
            return;

        if (htn.Blackboard.TryGetValue<EntityUid>(boss.Comp.TargetKey, out var targetEnt, EntityManager) && Exists(targetEnt))
            return;

        htn.Blackboard.SetValue(boss.Comp.TargetKey, target);
    }

    #endregion

    private sealed record BubblegumArenaSession(
        EntityCoordinates ReturnAnchor,
        Dictionary<EntityUid, EntityCoordinates> Participants,
        EntityUid? ArenaMap,
        EntityUid? ArenaGrid);

    private sealed class StepCounter
    {
        public int CompletedSteps { get; set; }
        public int TotalSteps { get; set; }
    }
}
