using Content.Server.Pinpointer;
using Content.Shared._ES.Auditions.Components;
using Content.Shared._ES.Masks.Traitor;
using Content.Shared._ES.Masks.Traitor.Components;
using Content.Shared._ES.SpawnRegion;
using Robust.Shared.Utility;

namespace Content.Server._ES.Masks.Traitor;

public sealed class ESMaskCacheSystem : ESSharedMaskCacheSystem
{
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly ESSharedSpawnRegionSystem _spawnRegion = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESMaskCacheSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<ESMaskCacheSpawnerComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<ESCharacterComponent>(ent, out var character))
            return;

        if (!_spawnRegion.TryGetRandomCoordsInRegion(
                ent.Comp.Region,
                character.Station,
                out var coords,
                checkPlayerLOS: false,
                minPlayerDistance: 0f))
        {
            Log.Debug("Failed to find spawn region!");
            return;
        }

        var spawner = SpawnAtPosition(CeilingCachePrototype, coords.Value);
        var comp = EnsureComp<ESCeilingCacheComponent>(spawner);
        comp.MindId = ent;
        comp.CacheLoot = ent.Comp.CacheProto;

        var mapCoords = TransformSystem.ToMapCoordinates(coords.Value);
        ent.Comp.LocationString = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString(mapCoords));
        ent.Comp.Location = (Vector2i) mapCoords.Position.Rounded();
        Dirty(ent);

        Dirty(spawner, comp);
    }
}
