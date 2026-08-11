// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Common.Chasm;
using Content.Lavaland.Shared.Chasm.Teleport;
using Content.Shared.Chasm;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using System.Diagnostics.CodeAnalysis;

namespace Content.Lavaland.Server.Chasm.Teleport;

/// <summary>
/// Loads the original Mercury arena and moves entities there after they fall
/// into its Lavaland fissure. Ported from Goobstation PR #6542.
/// </summary>
public sealed partial class ChasmTeleportSystem : EntitySystem
{
    [Dependency] private MapLoaderSystem _mapLoader = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChasmFallingComponent, BeforeChasmFallingEvent>(OnBeforeFalling);
    }

    private void OnBeforeFalling(EntityUid uid, ChasmFallingComponent falling, ref BeforeChasmFallingEvent args)
    {
        if (args.Cancelled || !TryComp<ChasmTeleportComponent>(falling.FallingInto, out var comp))
            return;

        if (!TryGetOrLoadMap(comp, out var beaconCoords) || beaconCoords is null)
            return;

        args.Cancelled = true;
        _transform.SetCoordinates(args.Entity, beaconCoords.Value);
    }

    private bool TryGetOrLoadMap(ChasmTeleportComponent comp, out EntityCoordinates? beaconCoords)
    {
        beaconCoords = null;

        if (comp.LoadedMap is not null && !TerminatingOrDeleted(comp.LoadedMap.Value))
        {
            _map.SetPaused(Comp<MapComponent>(comp.LoadedMap.Value).MapId, false);
            return TryGetBeaconCoords(comp.LoadedMap.Value, out beaconCoords);
        }

        if (!_mapLoader.TryLoadMap(comp.MapPath,
                out var map,
                out _,
                options: new DeserializationOptions { InitializeMaps = true }))
        {
            Log.Error($"ChasmTeleportSystem failed to load {comp.MapPath}");
            return false;
        }

        comp.LoadedMap = map;
        // The original serialized arena is saved paused. InitializeMaps does not
        // override that state, so explicitly start its animations and boss timers.
        _map.SetPaused(map.Value.Comp.MapId, false);
        return TryGetBeaconCoords(map!.Value, out beaconCoords);
    }

    private bool TryGetBeaconCoords(EntityUid mapUid, [NotNullWhen(true)] out EntityCoordinates? coords)
    {
        var query = EntityQueryEnumerator<ChasmTeleportBeaconComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (Transform(uid).MapUid != mapUid)
                continue;

            coords = xform.Coordinates;
            return true;
        }

        coords = null;
        return false;
    }
}
