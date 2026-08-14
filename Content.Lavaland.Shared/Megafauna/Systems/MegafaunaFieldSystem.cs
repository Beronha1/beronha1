// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.EntityShapes;
using Content.Lavaland.Shared.Megafauna.Components;
using Content.Lavaland.Shared.Megafauna.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using Robust.Shared.Threading;

// ReSharper disable EnforceForeachStatementBraces
namespace Content.Lavaland.Shared.Megafauna.Systems;

public sealed partial class MegafaunaFieldSystem : EntitySystem
{
    [Dependency] private EntityShapeSystem _entityShape = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private IParallelManager _parallel = default!;

    private MegafaunaSpawnFieldJob _job;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, MegafaunaStartupEvent>(OnStartup);
        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, MegafaunaShutdownEvent>(OnShutdown);
        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, MegafaunaKilledEvent>(OnDefeated);
        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, EntityTerminatingEvent>(OnTerminating);

        _job = new MegafaunaSpawnFieldJob { System = this };
    }

    private void OnStartup(Entity<MegafaunaFieldGeneratorComponent> ent, ref MegafaunaStartupEvent args)
    {
        // Aggressor and AI events may arrive during the same damage transaction
        // that killed the boss. Never let a late startup recreate a dead boss's
        // arena after its death cleanup already ran.
        if (_mobState.IsDead(ent.Owner))
        {
            DeactivateField(ent);
            return;
        }

        ActivateField(ent);
    }

    private void OnShutdown(Entity<MegafaunaFieldGeneratorComponent> ent, ref MegafaunaShutdownEvent args)
        => DeactivateField(ent);

    private void OnDefeated(Entity<MegafaunaFieldGeneratorComponent> ent, ref MegafaunaKilledEvent args)
        => DeactivateField(ent);

    private void OnMobStateChanged(Entity<MegafaunaFieldGeneratorComponent> ent, ref MobStateChangedEvent args)
    {
        // A harvestable boss remains as a corpse. If it dies while its AI is
        // inactive, MegafaunaKilledEvent is not raised and EntityTerminatingEvent
        // may never happen, so the arena must also follow the actual mob state.
        if (args.NewMobState == MobState.Dead)
            DeactivateField(ent);
    }

    private void OnTerminating(Entity<MegafaunaFieldGeneratorComponent> ent, ref EntityTerminatingEvent args)
        // During map/grid teardown the broadphase can already be gone. Tracked
        // and explicitly-owned walls are still safe to delete, but a spatial
        // recovery query would log an error while the lookup tree is shutting down.
        => DeactivateField(ent, recoverNearby: false);

    public void ActivateField(Entity<MegafaunaFieldGeneratorComponent> ent)
    {
        if (TerminatingOrDeleted(ent.Owner) || _mobState.IsDead(ent.Owner))
        {
            DeactivateField(ent);
            return;
        }

        if (ent.Comp.Enabled)
            return;

        _job.Entity = ent;
        _parallel.ProcessNow(_job);
        ent.Comp.Enabled = true;
        Dirty(ent);
    }

    private void SpawnField(Entity<MegafaunaFieldGeneratorComponent> ent)
    {
        var comp = ent.Comp;
        var origin = Transform(ent).Coordinates.AlignWithClosestGridTile(1.5f, EntityManager);
        comp.FieldOrigin = origin;
        _entityShape.SpawnEntityShape(comp.WallShape, origin, comp.WallId, out comp.Walls);
        foreach (var wall in comp.Walls)
        {
            var owned = EnsureComp<MegafaunaFieldWallComponent>(wall);
            owned.Generator = ent.Owner;
            Dirty(wall, owned);
        }
    }

    public void DeactivateField(Entity<MegafaunaFieldGeneratorComponent> ent, bool recoverNearby = true)
    {
        if (!ent.Comp.Enabled && ent.Comp.Walls.Count == 0 && ent.Comp.FieldOrigin == null)
            return;

        // Start from the replicated list, then recover any predicted/orphaned
        // walls through their explicit owner. HashSet also prevents a duplicate
        // UID from being queued twice.
        var walls = new HashSet<EntityUid>(ent.Comp.Walls);
        var query = EntityQueryEnumerator<MegafaunaFieldWallComponent>();
        while (query.MoveNext(out var wall, out var owned))
        {
            if (owned.Generator == ent.Owner)
                walls.Add(wall);
        }

        // Last-resort recovery for walls created before ownership finished
        // replicating, or for a stale Walls list. The field is centered on its
        // activation point rather than the boss's death position because mobile
        // bosses can cross most of the arena during combat.
        if (recoverNearby && ent.Comp.FieldOrigin is { } origin && origin.IsValid(EntityManager))
        {
            var size = ent.Comp.WallShape.DefaultSize ?? ent.Comp.WallShape.Size;
            var offset = ent.Comp.WallShape.DefaultOffset ?? ent.Comp.WallShape.Offset;
            var cleanupRadius = MathF.Max(2f, size * 1.5f + offset.Length() + 2f);
            foreach (var nearby in _lookup.GetEntitiesInRange(origin, cleanupRadius))
            {
                if (MetaData(nearby).EntityPrototype?.ID == ent.Comp.WallId.Id)
                    walls.Add(nearby);
            }
        }

        foreach (var wall in walls)
        {
            if (!TerminatingOrDeleted(wall))
                PredictedQueueDel(wall);
        }

        ent.Comp.Walls.Clear();
        ent.Comp.FieldOrigin = null;
        ent.Comp.Enabled = false;
        Dirty(ent);
    }

    private record struct MegafaunaSpawnFieldJob : IRobustJob
    {
        public required MegafaunaFieldSystem System;
        public Entity<MegafaunaFieldGeneratorComponent> Entity;

        public void Execute()
        {
            System.SpawnField(Entity);
        }
    }
}
