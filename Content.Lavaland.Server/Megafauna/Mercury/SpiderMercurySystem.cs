// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Lavaland.Shared.Megafauna.Components;
using Content.Lavaland.Shared.Megafauna.Events;
using Content.Lavaland.Shared.Megafauna.Harvesting;
using Content.Lavaland.Shared.Megafauna.Mercury;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Weapons.Reflect;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.Mercury;

/// <summary>
/// Server-authoritative adaptation of the Spider of Mercury systems from
/// Goobstation PR #6542. The original project spread these attacks over many
/// generic systems; keeping them together here avoids coupling Whiskey to the
/// PR's unrelated fissure devices while preserving the encounter behavior.
/// </summary>
public sealed partial class SpiderMercurySystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private static readonly ProtoId<DamageGroupPrototype> GeneticDamageGroup = "Genetic";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AddOrRemoveComponentComponent, AddComponentActionEvent>(OnAddComponent);
        SubscribeLocalEvent<VicinitySpawnerComponent, MapInitEvent>(OnVicinityMapInit);
        SubscribeLocalEvent<EtherDrainComponent, EtherDrainEvent>(OnEtherDrain);
        SubscribeLocalEvent<CosmicRayCirculatorComponent, CosmicRayCirculatorActionEvent>(OnCosmicRays);
        SubscribeLocalEvent<EnvironmentalResonanceComponent, EnvironmentalResonanceActionEvent>(OnResonance);
        SubscribeLocalEvent<ORTSolarStormComponent, ORTSolarStormActionEvent>(OnSolarStorm);
        SubscribeLocalEvent<ParadigmInflationComponent, ParadigmInflationActionEvent>(OnParadigmInflation);
        SubscribeLocalEvent<PhaseConversionComponent, PhaseConversionActionEvent>(OnPhaseConversion);
        SubscribeLocalEvent<ReflectiveThreadsComponent, ReflectiveThreadsActionEvent>(OnReflectiveThreads);
        SubscribeLocalEvent<OrbitingRingComponent, OrbitingRingActionEvent>(OnOrbitingRing);
        SubscribeLocalEvent<ORTConvergenceComponent, ORTConvergenceActionEvent>(OnConvergence);
        SubscribeLocalEvent<ORTTransportMatterComponent, MapInitEvent>(OnTransportMapInit);
        SubscribeLocalEvent<ORTTransportMatterComponent, MobStateChangedEvent>(OnTransportMobStateChanged);
        SubscribeLocalEvent<ORTTransportMatterComponent, EntityTerminatingEvent>(OnTransportTerminating);
        SubscribeLocalEvent<SpiderMercuryStageComponent, MobStateChangedEvent>(OnStageDefeated);
    }

    private void OnStageDefeated(Entity<SpiderMercuryStageComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != Content.Shared.Mobs.MobState.Dead)
            return;

        var coordinates = Transform(ent).Coordinates;
        if (ent.Comp.TransitionEffect is { } effect)
            Spawn(effect, coordinates);
        if (ent.Comp.NextStage is { } next)
            Spawn(next, coordinates);
        if (!HasComp<MegafaunaHarvestableComponent>(ent))
            QueueDel(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateAddedComponents();
        UpdateVicinitySpawners();
        UpdateDirectionalMovement(frameTime);
        UpdateCosmicRays();
        UpdateSolarStorm();
        UpdateParadigmInflation();
        UpdatePhaseConversion();
        UpdateReflectiveThreads();
        UpdateOrbiting(frameTime);
        UpdateConvergence();
        UpdateSafeZones();
        UpdateDangerZones();
        UpdateTransport(frameTime);
    }

    private void OnAddComponent(Entity<AddOrRemoveComponentComponent> ent, ref AddComponentActionEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.TargetComponent = args.TargetComponent;
        ent.Comp.RemoveAfterTimer = args.RemoveAfterTimer;
        ent.Comp.TimeToRemoval = args.TimeToRemoval;
        EntityManager.AddComponents(ent.Owner, args.TargetComponent);

        if (args.RemoveAfterTimer)
            ent.Comp.RemovalTime = _timing.CurTime + args.TimeToRemoval;

        args.Handled = true;
    }

    private void UpdateAddedComponents()
    {
        var query = EntityQueryEnumerator<AddOrRemoveComponentComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.RemoveAfterTimer || _timing.CurTime < comp.RemovalTime || comp.TargetComponent == null)
                continue;

            EntityManager.RemoveComponents(uid, comp.TargetComponent);
            comp.RemoveAfterTimer = false;
            comp.RemovalTime = TimeSpan.Zero;
        }
    }

    private void OnVicinityMapInit(Entity<VicinitySpawnerComponent> ent, ref MapInitEvent args)
    {
        SpawnVicinity(ent);
        ent.Comp.NextSpawn = _timing.CurTime + ent.Comp.SpawnInterval;
    }

    private void UpdateVicinitySpawners()
    {
        var query = EntityQueryEnumerator<VicinitySpawnerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextSpawn)
                continue;

            comp.NextSpawn = _timing.CurTime + comp.SpawnInterval;
            SpawnVicinity((uid, comp));
        }
    }

    private void SpawnVicinity(Entity<VicinitySpawnerComponent> ent)
    {
        if (ent.Comp.Prototype.Count == 0)
            return;

        var xform = Transform(ent);
        for (var i = 0; i < ent.Comp.NumberToSpawn; i++)
        {
            var coordinates = xform.Coordinates;
            if (xform.GridUid is { } grid)
            {
                coordinates = new EntityCoordinates(
                    grid,
                    coordinates.X + _random.Next(-ent.Comp.OffsetForSpawn, ent.Comp.OffsetForSpawn + 1),
                    coordinates.Y + _random.Next(-ent.Comp.OffsetForSpawn, ent.Comp.OffsetForSpawn + 1));
            }

            Spawn(_random.Pick(ent.Comp.Prototype), coordinates);
        }
    }

    private void OnEtherDrain(Entity<EtherDrainComponent> ent, ref EtherDrainEvent args)
    {
        if (args.Handled)
            return;

        var targets = new HashSet<Entity<ActorComponent>>();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.Range, targets);
        foreach (var target in targets)
        {
            if (target.Owner == ent.Owner)
                continue;

            _stamina.TakeOvertimeStaminaDamage(target, ent.Comp.StaminaDrain);
            _popup.PopupEntity(Loc.GetString("ort-ether-drain"), target, target, PopupType.MediumCaution);
            Spawn(ent.Comp.Prototype, Transform(target).Coordinates);
        }

        args.Handled = true;
    }

    private void OnCosmicRays(Entity<CosmicRayCirculatorComponent> ent, ref CosmicRayCirculatorActionEvent args)
    {
        if (args.Handled || ent.Comp.Active)
            return;

        if (TryComp<MegafaunaAnchorComponent>(ent, out var anchor))
            anchor.Anchored = true;

        ent.Comp.Active = true;
        ent.Comp.CurrentWave = 0;
        ent.Comp.NextWaveTime = _timing.CurTime + ent.Comp.Delay;
        args.Handled = true;
    }

    private void UpdateCosmicRays()
    {
        var query = EntityQueryEnumerator<CosmicRayCirculatorComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Active || _timing.CurTime < comp.NextWaveTime)
                continue;

            var radius = comp.Radius + comp.RadiusIncrease * comp.CurrentWave;
            SpawnRingWarnings(Transform(uid).Coordinates, radius, comp.Count, comp.WarningPrototype);
            comp.CurrentWave++;

            if (comp.CurrentWave >= comp.WaveCount)
            {
                comp.Active = false;
                comp.CurrentWave = 0;
                if (TryComp<MegafaunaAnchorComponent>(uid, out var anchor))
                    anchor.Anchored = false;
            }
            else
            {
                comp.NextWaveTime = _timing.CurTime + comp.WaveDelay;
            }
        }
    }

    private void OnResonance(Entity<EnvironmentalResonanceComponent> ent, ref EnvironmentalResonanceActionEvent args)
    {
        if (args.Handled)
            return;

        var coordinates = Transform(ent).Coordinates;
        for (var i = 0; i < ent.Comp.RowNumber; i++)
        {
            if (args.Vertical)
            {
                var x = -ent.Comp.HorizontalOffset + i * ent.Comp.TileSkip;
                Spawn(ent.Comp.DownPrototype, coordinates.Offset(new Vector2(x, ent.Comp.VerticalOffset)));
            }
            else
            {
                var y = ent.Comp.VerticalOffset - i * ent.Comp.TileSkip;
                Spawn(ent.Comp.RightPrototype, coordinates.Offset(new Vector2(-ent.Comp.HorizontalOffset, y)));
            }
        }

        args.Handled = true;
    }

    private void UpdateDirectionalMovement(float frameTime)
    {
        var query = EntityQueryEnumerator<DirectionalMovementComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Direction.LengthSquared() <= float.Epsilon)
                continue;

            comp.CurrentSpeed = comp.Acceleration > 0f
                ? MathF.Min(comp.CurrentSpeed + comp.Acceleration * frameTime, comp.Speed)
                : comp.Speed;
            var direction = Vector2.Normalize(comp.Direction);
            _transform.SetLocalPosition(uid, Transform(uid).LocalPosition + direction * comp.CurrentSpeed * frameTime);
        }
    }

    private void OnSolarStorm(Entity<ORTSolarStormComponent> ent, ref ORTSolarStormActionEvent args)
    {
        if (args.Handled || ent.Comp.IsCharging || ent.Comp.StormSoon || ent.Comp.IsActive)
            return;

        ent.Comp.CurrentParticleSpawnRate = ent.Comp.ParticleSpawnRate;
        ent.Comp.ChargeEndTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.ChargeTime);
        ent.Comp.NextParticleSpawn = _timing.CurTime;
        ent.Comp.WarningEntity = Spawn(ent.Comp.WarningPrototype, Transform(ent).Coordinates);
        _transform.SetParent(ent.Comp.WarningEntity.Value, ent.Owner);
        _audio.PlayPvs(ent.Comp.ChargeSound, ent);
        ent.Comp.IsCharging = true;
        args.Handled = true;
    }

    private void UpdateSolarStorm()
    {
        var query = EntityQueryEnumerator<ORTSolarStormComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.IsCharging)
            {
                if (_timing.CurTime >= comp.NextParticleSpawn)
                {
                    var position = Transform(uid).Coordinates.Offset(_random.NextAngle().ToVec() * comp.ParticleSpawnRadius);
                    Spawn(comp.ParticlePrototype, position);
                    comp.CurrentParticleSpawnRate = MathF.Max(0.05f, comp.CurrentParticleSpawnRate - comp.ParticleIncreaseBy);
                    comp.NextParticleSpawn = _timing.CurTime + TimeSpan.FromSeconds(comp.CurrentParticleSpawnRate);
                }

                if (_timing.CurTime >= comp.ChargeEndTime)
                {
                    comp.IsCharging = false;
                    if (comp.WarningEntity is { } warning && Exists(warning))
                        QueueDel(warning);
                    comp.WarningEntity = null;
                    comp.StormSoon = true;
                    comp.StormStartTime = _timing.CurTime + TimeSpan.FromSeconds(comp.WaitForIt);
                }
            }

            if (comp.StormSoon && _timing.CurTime >= comp.StormStartTime)
            {
                comp.StormSoon = false;
                comp.StormEntity = Spawn(comp.StormPrototype, Transform(uid).Coordinates);
                _transform.SetParent(comp.StormEntity.Value, uid);
                _audio.PlayPvs(comp.FireSound, uid);
                comp.IsActive = true;
                comp.NextDamageTick = _timing.CurTime;
                comp.StormEndTime = _timing.CurTime + TimeSpan.FromSeconds(comp.StormDuration);
            }

            if (!comp.IsActive)
                continue;

            if (_timing.CurTime >= comp.NextDamageTick)
            {
                comp.NextDamageTick = _timing.CurTime + TimeSpan.FromSeconds(0.5f);
                foreach (var target in _lookup.GetEntitiesInRange(Transform(uid).Coordinates, comp.StormRadius))
                {
                    if (target != uid && !_mobState.IsDead(target))
                        _damage.TryChangeDamage(target, comp.StormDamage, origin: uid);
                }
            }

            if (_timing.CurTime < comp.StormEndTime)
                continue;

            comp.IsActive = false;
            if (comp.StormEntity is { } storm && Exists(storm))
                QueueDel(storm);
            comp.StormEntity = null;
        }
    }

    private void OnParadigmInflation(Entity<ParadigmInflationComponent> ent, ref ParadigmInflationActionEvent args)
    {
        if (args.Handled || ent.Comp.IsAnalyzing || _mobState.IsDead(args.Target))
            return;

        ent.Comp.IsAnalyzing = true;
        ent.Comp.Target = args.Target;
        ent.Comp.AnalyzeEndTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.AnalyzeTime);
        ent.Comp.WarningEntity = Spawn(ent.Comp.WarningPrototype, Transform(args.Target).Coordinates);
        _transform.SetParent(ent.Comp.WarningEntity.Value, args.Target);
        _audio.PlayPvs(ent.Comp.AnalyzeSound, ent);
        args.Handled = true;
    }

    private void UpdateParadigmInflation()
    {
        var query = EntityQueryEnumerator<ParadigmInflationComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsAnalyzing || comp.Target is not { } target || _timing.CurTime < comp.AnalyzeEndTime)
                continue;

            comp.IsAnalyzing = false;
            comp.Target = null;
            if (comp.WarningEntity is { } warning && Exists(warning))
                QueueDel(warning);
            comp.WarningEntity = null;

            if (_mobState.IsDead(target) || !TryComp<DamageableComponent>(target, out var damageable))
                continue;

            string? highestType = null;
            var highestDamage = FixedPoint2.Zero;
            foreach (var (type, amount) in _damage.GetAllDamage((target, damageable)).DamageDict)
            {
                if (amount <= highestDamage)
                    continue;
                highestType = type;
                highestDamage = amount;
            }

            if (highestType == null)
                continue;

            var genetic = _prototypes.Index(GeneticDamageGroup);
            if (genetic.DamageTypes.Contains(highestType))
                continue;

            var heal = new Content.Shared.Damage.DamageSpecifier();
            heal.DamageDict.Add(highestType, -highestDamage);
            _damage.TryChangeDamage(target, heal, origin: uid);
            _damage.TryChangeDamage(target, new Content.Shared.Damage.DamageSpecifier(genetic, highestDamage / comp.DivideDamage), origin: uid);
            _audio.PlayPvs(comp.ParadigmSound, uid);
        }
    }

    private void OnPhaseConversion(Entity<PhaseConversionComponent> ent, ref PhaseConversionActionEvent args)
    {
        if (args.Handled || ent.Comp.SwitchSoon)
            return;

        _audio.PlayPvs(ent.Comp.SwitchSound, ent);
        ent.Comp.EffectEntity = Spawn(ent.Comp.EffectPrototype, Transform(ent).Coordinates);
        _transform.SetParent(ent.Comp.EffectEntity.Value, ent.Owner);
        ent.Comp.SwitchSoon = true;
        ent.Comp.SwitchTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.SwitchDelay);
        args.Handled = true;
    }

    private void UpdatePhaseConversion()
    {
        var query = EntityQueryEnumerator<PhaseConversionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.SwitchSoon || _timing.CurTime < comp.SwitchTime)
                continue;

            comp.SwitchSoon = false;
            comp.IsRanged = !comp.IsRanged;
            if (TryComp<MegafaunaAiComponent>(uid, out var ai))
            {
                var selector = comp.IsRanged ? comp.RangedSelector : comp.MeleeSelector;
                ai.Selector = _prototypes.Index(selector).Selector;
            }
            _appearance.SetData(uid, PhaseConversionVisuals.IsRanged, comp.IsRanged);
        }
    }

    private void OnReflectiveThreads(Entity<ReflectiveThreadsComponent> ent, ref ReflectiveThreadsActionEvent args)
    {
        if (args.Handled || ent.Comp.Reflecting)
            return;

        _audio.PlayPvs(ent.Comp.ReflectSound, ent);
        ent.Comp.EffectEntity = Spawn(ent.Comp.EffectPrototype, Transform(ent).Coordinates);
        _transform.SetParent(ent.Comp.EffectEntity.Value, ent.Owner);
        var reflect = EnsureComp<ReflectComponent>(ent);
        reflect.ReflectProb = 1f;
        reflect.Reflects = ReflectType.Energy | ReflectType.NonEnergy | ReflectType.Magic;
        ent.Comp.Reflecting = true;
        ent.Comp.ReflectEndTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.ReflectDuration);
        args.Handled = true;
    }

    private void UpdateReflectiveThreads()
    {
        var query = EntityQueryEnumerator<ReflectiveThreadsComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Reflecting || _timing.CurTime < comp.ReflectEndTime)
                continue;

            comp.Reflecting = false;
            RemCompDeferred<ReflectComponent>(uid);
            if (comp.EffectEntity is { } effect && Exists(effect))
                QueueDel(effect);
            comp.EffectEntity = null;
        }
    }

    private void OnOrbitingRing(Entity<OrbitingRingComponent> ent, ref OrbitingRingActionEvent args)
    {
        if (args.Handled)
            return;

        foreach (var old in ent.Comp.Entities)
        {
            if (Exists(old))
                QueueDel(old);
        }
        ent.Comp.Entities.Clear();

        for (var i = 0; i < ent.Comp.Count; i++)
        {
            var spawned = Spawn(ent.Comp.Prototype, Transform(ent).Coordinates);
            _transform.SetParent(spawned, ent.Owner);
            var orbit = EnsureComp<OrbitingComponent>(spawned);
            orbit.Angle = MathF.Tau * i / ent.Comp.Count;
            orbit.Radius = 0f;
            orbit.MaxRadius = ent.Comp.RingDistance;
            orbit.GrowSpeed = ent.Comp.GrowSpeed;
            ent.Comp.Entities.Add(spawned);
        }

        if (ent.Comp.Sound != null)
            _audio.PlayPvs(ent.Comp.Sound, ent);
        args.Handled = true;
    }

    private void UpdateOrbiting(float frameTime)
    {
        var query = EntityQueryEnumerator<OrbitingComponent>();
        while (query.MoveNext(out var uid, out var orbit))
        {
            var parent = Transform(uid).ParentUid;
            if (!Exists(parent))
            {
                QueueDel(uid);
                continue;
            }

            orbit.Radius = MathF.Min(orbit.Radius + orbit.GrowSpeed * frameTime, orbit.MaxRadius);
            orbit.Angle += MathF.Tau * frameTime;
            _transform.SetLocalPosition(uid, new Vector2(MathF.Cos(orbit.Angle), MathF.Sin(orbit.Angle)) * orbit.Radius);
        }
    }

    private void OnConvergence(Entity<ORTConvergenceComponent> ent, ref ORTConvergenceActionEvent args)
    {
        if (args.Handled || ent.Comp.Active)
            return;

        var angle = _random.NextAngle().ToVec();
        var distance = _random.NextFloat(ent.Comp.MinDistance, ent.Comp.MaxDistance);
        var center = _transform.GetWorldPosition(ent) + angle * distance;
        ent.Comp.SafeZoneEntity = Spawn(ent.Comp.SafeZonePrototype, new MapCoordinates(center, Transform(ent).MapID));
        ent.Comp.Active = true;
        ent.Comp.CurrentWave = 0;
        ent.Comp.NextWaveTime = _timing.CurTime + ent.Comp.InitialDelay;
        args.Handled = true;
    }

    private void UpdateConvergence()
    {
        var query = EntityQueryEnumerator<ORTConvergenceComponent>();
        while (query.MoveNext(out _, out var comp))
        {
            if (!comp.Active || _timing.CurTime < comp.NextWaveTime)
                continue;

            if (comp.SafeZoneEntity is not { } safe || !Exists(safe))
            {
                FinishConvergence(comp);
                continue;
            }

            var progress = (float) comp.CurrentWave / comp.WaveCount;
            var radius = MathHelper.Lerp(comp.StartRadius, comp.SafeZoneRadius, progress);
            var count = Math.Max(comp.MinCount, (int) Math.Round(comp.Count * radius / comp.StartRadius));
            SpawnRingWarnings(Transform(safe).Coordinates, radius, count, comp.WarningPrototype);
            comp.CurrentWave++;

            if (comp.CurrentWave > comp.WaveCount)
                FinishConvergence(comp);
            else
                comp.NextWaveTime = _timing.CurTime + comp.WaveDelay;
        }
    }

    private void FinishConvergence(ORTConvergenceComponent comp)
    {
        comp.Active = false;
        comp.CurrentWave = 0;
        if (comp.SafeZoneEntity is { } safe && Exists(safe))
            QueueDel(safe);
        comp.SafeZoneEntity = null;
    }

    private void SpawnRingWarnings(EntityCoordinates center, float radius, int count, EntProtoId prototype)
    {
        for (var i = 0; i < count; i++)
        {
            var offset = new Angle(MathF.Tau * i / count).ToVec() * radius;
            Spawn(prototype, center.Offset(offset));
        }
    }

    private void UpdateSafeZones()
    {
        var query = EntityQueryEnumerator<SafeZoneComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextLookupTime)
                continue;
            comp.NextLookupTime = _timing.CurTime + comp.LookupInterval;

            var blacklist = new HashSet<string>();
            foreach (var id in comp.Blacklist)
                blacklist.Add(id.Id);
            foreach (var nearby in _lookup.GetEntitiesInRange(Transform(uid).Coordinates, comp.SafeRadius))
            {
                if (nearby == uid || MetaData(nearby).EntityPrototype is not { } prototype)
                    continue;
                if (blacklist.Contains(prototype.ID))
                    QueueDel(nearby);
            }
        }
    }

    private void UpdateDangerZones()
    {
        var query = EntityQueryEnumerator<DangerZoneComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Popup.Count == 0 || _timing.CurTime < comp.NextPopup)
                continue;
            comp.NextPopup = _timing.CurTime + comp.Interval;
            var message = Loc.GetString(_random.Pick(comp.Popup));
            foreach (var actor in _lookup.GetEntitiesInRange<ActorComponent>(Transform(uid).Coordinates, comp.PopUpRange))
            {
                if (actor.Owner != uid)
                    _popup.PopupEntity(message, actor, actor, PopupType.MediumCaution);
            }
        }
    }

    private void OnTransportMapInit(Entity<ORTTransportMatterComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.AnchorEntity = Spawn(ent.Comp.AnchorPrototype, Transform(ent).Coordinates);
        ent.Comp.NextTransport = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.TeleportDelay);
    }

    private void OnTransportTerminating(Entity<ORTTransportMatterComponent> ent, ref EntityTerminatingEvent args)
    {
        foreach (var spawned in new[] { ent.Comp.AnchorEntity, ent.Comp.DashWarningEntity, ent.Comp.PlayerTargetEntity })
        {
            if (spawned is { } uid && Exists(uid))
                QueueDel(uid);
        }
    }

    private void OnTransportMobStateChanged(Entity<ORTTransportMatterComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        StopTransport(ent);
    }

    private void UpdateTransport(float frameTime)
    {
        var query = EntityQueryEnumerator<ORTTransportMatterComponent, PhaseConversionComponent>();
        while (query.MoveNext(out var uid, out var transport, out var phase))
        {
            // The final Mercury body intentionally remains for harvesting, so its
            // timed transport component also remains. Never let a corpse restart
            // the dash loop or keep emitting dash damage after death.
            if (_mobState.IsDead(uid))
            {
                StopTransport((uid, transport));
                continue;
            }

            if (transport.Dashing)
            {
                if (transport.MoveTarget is { } target)
                {
                    var delta = target - _transform.GetWorldPosition(uid);
                    if (delta.LengthSquared() > 0.09f)
                        _physics.SetLinearVelocity(uid, Vector2.Normalize(delta) * transport.MoveSpeed);
                }

                if (!phase.IsRanged && _timing.CurTime >= transport.NextDashDamage)
                {
                    Spawn(transport.DashDamagePrototype, Transform(uid).Coordinates);
                    transport.NextDashDamage = _timing.CurTime + TimeSpan.FromSeconds(transport.DashDamageInterval);
                }

                if (_timing.CurTime < transport.DashEndTime)
                    continue;

                _physics.SetLinearVelocity(uid, Vector2.Zero);
                if (transport.MoveTarget is { } end)
                    _transform.SetWorldPosition(uid, end);
                if (!phase.IsRanged)
                    Spawn(transport.DashLandPrototype, Transform(uid).Coordinates);
                CleanupTransportIndicators(transport);
                transport.Dashing = false;
                transport.MoveTarget = null;
                transport.NextTransport = _timing.CurTime + TimeSpan.FromSeconds(transport.TeleportDelay * (phase.IsRanged ? 1f : transport.TeleportDelayMultiplier));
                continue;
            }

            if (_timing.CurTime < transport.NextTransport || IsSolarStormActive(uid))
                continue;

            StartTransport(uid, transport, phase);
        }
    }

    private void StartTransport(EntityUid uid, ORTTransportMatterComponent transport, PhaseConversionComponent phase)
    {
        var target = GetRandomTransportTarget(transport);
        if (!phase.IsRanged && FindNearestPlayer(uid) is { } player)
        {
            var playerPosition = _transform.GetWorldPosition(player);
            var delta = playerPosition - _transform.GetWorldPosition(uid);
            var direction = delta.LengthSquared() > float.Epsilon ? Vector2.Normalize(delta) : Vector2.UnitX;
            target = playerPosition + direction * transport.DashOvershootDistance;
            transport.PlayerTargetEntity = Spawn(transport.PlayerTargetPrototype, Transform(player).Coordinates);
            _transform.SetParent(transport.PlayerTargetEntity.Value, player);
        }

        if (transport.ShouldPlaySound)
            _audio.PlayPvs(transport.TeleportSound, uid);
        transport.DashWarningEntity = Spawn(transport.DashWarningPrototype, new MapCoordinates(target, Transform(uid).MapID));
        transport.MoveTarget = target;
        transport.Dashing = true;
        transport.DashEndTime = _timing.CurTime + TimeSpan.FromSeconds(transport.FadeOutTime);
        transport.NextDashDamage = _timing.CurTime;
    }

    private Vector2 GetRandomTransportTarget(ORTTransportMatterComponent transport)
    {
        var center = transport.AnchorEntity is { } anchor && Exists(anchor)
            ? _transform.GetWorldPosition(anchor)
            : Vector2.Zero;
        return center + new Vector2(
            _random.NextFloat(-transport.TeleportDistance, transport.TeleportDistance),
            _random.NextFloat(-transport.TeleportDistance, transport.TeleportDistance));
    }

    private EntityUid? FindNearestPlayer(EntityUid uid)
    {
        EntityUid? nearest = null;
        var nearestDistance = float.MaxValue;
        var origin = _transform.GetWorldPosition(uid);
        foreach (var actor in _lookup.GetEntitiesInRange<ActorComponent>(Transform(uid).Coordinates, 30f))
        {
            if (_mobState.IsDead(actor))
                continue;
            var distance = Vector2.DistanceSquared(origin, _transform.GetWorldPosition(actor));
            if (distance >= nearestDistance)
                continue;
            nearestDistance = distance;
            nearest = actor;
        }
        return nearest;
    }

    private bool IsSolarStormActive(EntityUid uid)
        => TryComp<ORTSolarStormComponent>(uid, out var storm) && (storm.IsActive || storm.IsCharging || storm.StormSoon);

    private void CleanupTransportIndicators(ORTTransportMatterComponent transport)
    {
        foreach (var spawned in new[] { transport.DashWarningEntity, transport.PlayerTargetEntity })
        {
            if (spawned is { } uid && Exists(uid))
                QueueDel(uid);
        }
        transport.DashWarningEntity = null;
        transport.PlayerTargetEntity = null;
    }

    private void StopTransport(Entity<ORTTransportMatterComponent> ent)
    {
        if (ent.Comp.Dashing)
            _physics.SetLinearVelocity(ent, Vector2.Zero);

        CleanupTransportIndicators(ent.Comp);
        ent.Comp.Dashing = false;
        ent.Comp.MoveTarget = null;
    }
}
