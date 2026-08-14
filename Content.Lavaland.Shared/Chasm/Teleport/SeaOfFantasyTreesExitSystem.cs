// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Physics.Events;

namespace Content.Lavaland.Shared.Chasm.Teleport;

/// <summary>
/// Handles the original Mercury arena escape rope.
/// Ported from Goobstation PR #6542.
/// </summary>
public sealed partial class SeaOfFantasyTreesExitSystem : EntitySystem
{
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SeaOfFantasyTreesExitComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, SeaOfFantasyTreesExitComponent comp, ref StartCollideEvent args)
    {
        var query = EntityQueryEnumerator<SeaOfFantasyTreesExitBeaconComponent, TransformComponent>();
        if (!query.MoveNext(out _, out _, out var beaconXform))
            return;

        _transform.SetCoordinates(args.OtherEntity, beaconXform.Coordinates);
    }
}
