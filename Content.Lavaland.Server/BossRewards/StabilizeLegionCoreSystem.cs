// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Artifacts;
using Content.Shared.EntityEffects;

namespace Content.Lavaland.Server.Artifacts;

/// <summary>
/// Converts a raw Legion core into the permanent, surgically transplantable organ.
/// </summary>
public sealed partial class StabilizeLegionCoreSystem
    : EntityEffectSystem<LegionCoreComponent, StabilizeLegionCore>
{
    [Dependency] private SharedTransformSystem _transform = default!;

    protected override void Effect(
        Entity<LegionCoreComponent> entity,
        ref EntityEffectEvent<StabilizeLegionCore> args)
    {
        Spawn("OrganStabilizedLegionCore", _transform.GetMapCoordinates(entity));
        QueueDel(entity);
    }
}
