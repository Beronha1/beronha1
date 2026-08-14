// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Lavaland.Server.Weapons;

/// <summary>
/// Marker component that used for weapons.
/// If weapon has this component, Megafauna can drop special loot.
/// </summary>
[RegisterComponent]
public sealed partial class MegafaunaWeaponLooterComponent : Component;

/// <summary>
/// Identifies a projectile fired by a qualifying portable kinetic weapon.
/// Kept server-side because it is only used to account megafauna trophy damage.
/// </summary>
[RegisterComponent]
public sealed partial class MegafaunaWeaponLooterProjectileComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid SourceWeapon;
}
