// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Weapons.Ranged.Events;

/// <summary>
/// Raised on a gun while one generated projectile is being expanded into its
/// final pellet fan, before any projectile receives velocity.
/// </summary>
/// <remarks>
/// This is predicted. Handlers must be deterministic and must not spawn or
/// mutate entities. <see cref="Prototype"/> is the non-spreading prototype
/// used for the additional pellets.
/// </remarks>
[ByRefEvent]
public record struct GunGetProjectileSpreadEvent(
    EntityUid Projectile,
    EntProtoId? Prototype,
    int Count,
    Angle Spread,
    EntityUid? User);
