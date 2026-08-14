// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Megafauna.Events;
using Content.Lavaland.Shared.MobPhases;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Throwing;
using Content.Lavaland.Shared.Megafauna.Components;
using Content.Lavaland.Shared.Megafauna.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.ChildishOni;

/// <summary>
/// Ports Goob PR #6734's Childish Oni attacks to Whiskey's modular Lavaland projects.
/// </summary>
public sealed partial class ChildishOniSystem : EntitySystem
{
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private GunSystem _gun = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MegafaunaFieldSystem _field = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IGameTiming _timing = default!;

    private readonly List<VolleyState> _volleys = new();
    private readonly List<HandBarrageState> _handBarrages = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChildishOniComponent, ChildishOniRampageEvent>(OnRampage);
        SubscribeLocalEvent<ChildishOniComponent, ChildishOniRingEvent>(OnRing);
        SubscribeLocalEvent<ChildishOniComponent, ChildishOniFlurryEvent>(OnFlurry);
        SubscribeLocalEvent<ChildishOniComponent, ChildishOniHandEvent>(OnHand);
        SubscribeLocalEvent<ChildishOniComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<ChildishOniComponent, StopThrowEvent>(OnStopThrow);
        SubscribeLocalEvent<ChildishOniComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ChildishOniComponent, EntityTerminatingEvent>(OnTerminating);
        SubscribeLocalEvent<ChildishOniComponent, MapInitEvent>(OnMapInit, after: [typeof(MobPhasesSystem)]);
        SubscribeLocalEvent<ChildishOniComponent, DamageChangedEvent>(OnDamageChanged, after: [typeof(MobPhasesSystem)]);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateVolleys();
        UpdateHandBarrages();
        UpdateDirectionalMovement(frameTime);
        UpdateSpirals(frameTime);
        UpdateOrbits(frameTime);
    }

    private void OnMapInit(Entity<ChildishOniComponent> ent, ref MapInitEvent args)
        => ApplyPhaseVisual(ent);

    private void OnDamageChanged(Entity<ChildishOniComponent> ent, ref DamageChangedEvent args)
        => ApplyPhaseVisual(ent);

    private void ApplyPhaseVisual(Entity<ChildishOniComponent> ent)
    {
        if (!TryComp<MobPhasesComponent>(ent, out var phases) ||
            !TryComp<AppearanceComponent>(ent, out var appearance) ||
            ent.Comp.LastVisualPhase == phases.CurrentPhase)
        {
            return;
        }

        ent.Comp.LastVisualPhase = phases.CurrentPhase;
        var visual = (ChildishOniPhaseVisual) Math.Clamp(phases.CurrentPhase - 1, 0, 3);
        _appearance.SetData(ent, ChildishOniVisuals.Phase, visual, appearance);
    }

    private void UpdateVolleys()
    {
        for (var i = _volleys.Count - 1; i >= 0; i--)
        {
            var volley = _volleys[i];
            if (!Exists(volley.Owner) || _mobState.IsDead(volley.Owner) || volley.Remaining <= 0)
            {
                _volleys.RemoveAt(i);
                continue;
            }

            if (_timing.CurTime < volley.NextShot)
                continue;

            var angle = volley.RandomDirection ? _random.NextAngle() : volley.Angle;
            FireProjectile(volley.Owner, volley.Prototype, angle.ToVec(), volley.Speed);
            volley.Remaining--;
            volley.NextShot = _timing.CurTime + volley.Delay;
        }
    }

    private void UpdateHandBarrages()
    {
        for (var i = _handBarrages.Count - 1; i >= 0; i--)
        {
            var barrage = _handBarrages[i];
            if (!TryComp<ChildishOniComponent>(barrage.Owner, out var oni) ||
                _mobState.IsDead(barrage.Owner) ||
                barrage.Remaining <= 0)
            {
                _handBarrages.RemoveAt(i);
                continue;
            }

            if (_timing.CurTime < barrage.NextSpawn)
                continue;

            SpawnSideHand(oni, barrage.Target, barrage.Offset);
            barrage.Remaining--;
            barrage.NextSpawn = _timing.CurTime + barrage.Interval;
        }
    }

    private void UpdateDirectionalMovement(float frameTime)
    {
        var query = EntityQueryEnumerator<ChildishOniDirectionalMovementComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var movement, out var transform))
        {
            var direction = movement.MoveEast ? Vector2.UnitX : movement.MoveWest ? -Vector2.UnitX : Vector2.Zero;
            if (direction == Vector2.Zero)
            {
                movement.CurrentSpeed = 0f;
                continue;
            }

            if (movement.Acceleration > 0f)
            {
                movement.CurrentSpeed = MathF.Min(
                    movement.CurrentSpeed + movement.Acceleration * frameTime,
                    movement.Speed);
            }
            else
            {
                movement.CurrentSpeed = movement.Speed;
            }

            _transform.SetLocalPosition(uid, transform.LocalPosition + direction * movement.CurrentSpeed * frameTime);
        }
    }

    private void UpdateSpirals(float frameTime)
    {
        var query = EntityQueryEnumerator<ChildishOniSpiralingComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var spiral, out var transform))
        {
            if (!spiral.MovementInitialized)
            {
                spiral.Origin = transform.LocalPosition;
                spiral.CurrentSpeed = spiral.SpiralSpeed;
                spiral.MovementInitialized = true;
            }

            spiral.CurrentSpeed = MathF.Min(
                spiral.CurrentSpeed + spiral.SpiralAcceleration * frameTime,
                spiral.SpiralMaxSpeed);
            spiral.Angle += spiral.CurrentSpeed * frameTime;
            spiral.Radius += spiral.CurrentSpeed * frameTime;

            var offset = new Vector2(MathF.Cos(spiral.Angle), MathF.Sin(spiral.Angle)) * spiral.Radius;
            _transform.SetLocalPosition(uid, spiral.Origin + offset);

            if (spiral.Radius < spiral.SpiralDistance)
                continue;

            if (spiral.DeleteOnEnd)
                QueueDel(uid);
            else
                RemCompDeferred<ChildishOniSpiralingComponent>(uid);
        }
    }

    private void UpdateOrbits(float frameTime)
    {
        var query = EntityQueryEnumerator<ChildishOniOrbitingComponent>();
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
            _transform.SetLocalPosition(uid,
                new Vector2(MathF.Cos(orbit.Angle), MathF.Sin(orbit.Angle)) * orbit.Radius);
        }
    }

    private void OnRampage(Entity<ChildishOniComponent> ent, ref ChildishOniRampageEvent args)
    {
        if (args.Handled || ent.Comp.IsLeaping || _mobState.IsDead(ent.Owner))
            return;

        var delta = args.Target.Position - Transform(ent).Coordinates.Position;
        if (delta.LengthSquared() <= float.Epsilon)
            return;

        ent.Comp.IsLeaping = true;
        var destination = Transform(ent).Coordinates.Offset(Vector2.Normalize(delta) * ent.Comp.JumpDistance);
        _throwing.TryThrow(ent, destination, ent.Comp.JumpSpeed);
        args.Handled = true;
    }

    private void OnLand(Entity<ChildishOniComponent> ent, ref LandEvent args)
    {
        if (!ent.Comp.IsLeaping || _mobState.IsDead(ent.Owner))
            return;

        ent.Comp.IsLeaping = false;
        var targets = new HashSet<EntityUid>();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.LandingRadius, targets);
        foreach (var target in targets)
        {
            if (target != ent.Owner && HasComp<DamageableComponent>(target))
                _damage.TryChangeDamage(target, ent.Comp.LandingDamage, true, origin: ent.Owner);
        }

        SpawnLandingRing(ent);
    }

    private void SpawnLandingRing(Entity<ChildishOniComponent> ent)
    {
        var xform = Transform(ent);
        if (xform.GridUid is not { } gridUid
            || !_transform.TryGetGridTilePosition(ent.Owner, out var tilePos)
            || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var range = (int) MathF.Ceiling(ent.Comp.LandingRadius);
        var gridEnt = (gridUid, grid);
        var center = _map.TileCenterToVector(gridEnt, tilePos);
        var outer = new Box2(center, center).Enlarged(range);
        var inner = new Box2(center, center).Enlarged(range - 1);
        var innerTiles = range > 1
            ? new HashSet<TileRef>(_map.GetLocalTilesIntersecting(ent.Owner, grid, inner))
            : new HashSet<TileRef>();

        foreach (var tile in _map.GetLocalTilesIntersecting(ent.Owner, grid, outer))
        {
            if (!innerTiles.Contains(tile))
                Spawn(ent.Comp.LandingRingPrototype, _map.GridTileToWorld(gridUid, grid, tile.GridIndices));
        }
    }

    private void OnStopThrow(Entity<ChildishOniComponent> ent, ref StopThrowEvent args)
        => ent.Comp.IsLeaping = false;

    private void OnRing(Entity<ChildishOniComponent> ent, ref ChildishOniRingEvent args)
    {
        if (args.Handled || _mobState.IsDead(ent.Owner))
            return;

        if (ent.Comp.Rings.TryGetValue(args.RingId, out var existing))
        {
            foreach (var skull in existing)
            {
                if (Exists(skull))
                    QueueDel(skull);
            }

            existing.Clear();
        }
        else
        {
            existing = new List<EntityUid>();
            ent.Comp.Rings[args.RingId] = existing;
        }

        const int count = 7;
        var coords = Transform(ent).Coordinates;
        for (var i = 0; i < count; i++)
        {
            var skull = SpawnAttachedTo(ent.Comp.LandingRingPrototype, coords);
            _transform.SetParent(skull, ent.Owner);
            var orbit = EnsureComp<ChildishOniOrbitingComponent>(skull);
            orbit.Angle = MathF.Tau * i / count;
            orbit.Radius = 0f;
            orbit.MaxRadius = args.Radius;
            orbit.GrowSpeed = 1f;
            existing.Add(skull);
        }

        args.Handled = true;
    }

    private void OnFlurry(Entity<ChildishOniComponent> ent, ref ChildishOniFlurryEvent args)
    {
        if (args.Handled || _mobState.IsDead(ent.Owner))
            return;

        _volleys.Add(new VolleyState(ent.Owner, ent.Comp.SlashProjectile, 20, 10f,
            TimeSpan.FromSeconds(0.1), _timing.CurTime, Angle.Zero, true));
        args.Handled = true;
    }

    private void OnHand(Entity<ChildishOniComponent> ent, ref ChildishOniHandEvent args)
    {
        if (args.Handled || _mobState.IsDead(ent.Owner))
            return;

        if (args.Count <= 1)
        {
            SpawnSideHand(ent.Comp, args.Target, args.Offset);
        }
        else
        {
            _handBarrages.Add(new HandBarrageState(ent.Owner, args.Target, args.Offset, args.Count,
                TimeSpan.FromSeconds(args.Interval), _timing.CurTime));
        }

        args.Handled = true;
    }

    private void SpawnSideHand(ChildishOniComponent comp, EntityCoordinates target, float offset)
    {
        var fromRight = _random.Prob(0.5f);
        var direction = fromRight ? Vector2.UnitX : -Vector2.UnitX;
        var prototype = fromRight ? comp.HandFromRightPrototype : comp.HandFromLeftPrototype;
        Spawn(prototype, target.Offset(direction * offset));
    }

    private void FireProjectile(EntityUid owner, EntProtoId prototype, Vector2 direction, float speed)
    {
        if (direction.LengthSquared() <= float.Epsilon)
            return;

        direction = Vector2.Normalize(direction);
        var ownerCoords = Transform(owner).Coordinates;
        var projectile = Spawn(prototype, _transform.ToMapCoordinates(ownerCoords));
        var velocity = _physics.GetMapLinearVelocity(ownerCoords);
        _gun.ShootProjectile(projectile, direction, velocity, owner, owner, speed);
    }

    private void OnTerminating(Entity<ChildishOniComponent> ent, ref EntityTerminatingEvent args)
        => CleanupQueuedAttacks(ent);

    private void OnMobStateChanged(Entity<ChildishOniComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        ent.Comp.IsLeaping = false;
        if (TryComp<MegafaunaFieldGeneratorComponent>(ent, out var field))
            _field.DeactivateField((ent.Owner, field));
        CleanupQueuedAttacks(ent);
    }

    private void CleanupQueuedAttacks(Entity<ChildishOniComponent> ent)
    {
        _volleys.RemoveAll(volley => volley.Owner == ent.Owner);
        _handBarrages.RemoveAll(barrage => barrage.Owner == ent.Owner);

        foreach (var ring in ent.Comp.Rings.Values)
        {
            foreach (var skull in ring)
            {
                if (Exists(skull))
                    QueueDel(skull);
            }
        }
        ent.Comp.Rings.Clear();
    }

    private sealed class VolleyState(
        EntityUid owner,
        EntProtoId prototype,
        int remaining,
        float speed,
        TimeSpan delay,
        TimeSpan nextShot,
        Angle angle,
        bool randomDirection)
    {
        public EntityUid Owner = owner;
        public EntProtoId Prototype = prototype;
        public int Remaining = remaining;
        public float Speed = speed;
        public TimeSpan Delay = delay;
        public TimeSpan NextShot = nextShot;
        public Angle Angle = angle;
        public bool RandomDirection = randomDirection;
    }

    private sealed class HandBarrageState(
        EntityUid owner,
        EntityCoordinates target,
        float offset,
        int remaining,
        TimeSpan interval,
        TimeSpan nextSpawn)
    {
        public EntityUid Owner = owner;
        public EntityCoordinates Target = target;
        public float Offset = offset;
        public int Remaining = remaining;
        public TimeSpan Interval = interval;
        public TimeSpan NextSpawn = nextSpawn;
    }
}
