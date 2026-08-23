// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

namespace Content.Shared._Starlight.NullSpace.Components;

/// <summary>
/// Will block and effects nullspace entities.
/// </summary>
[RegisterComponent]
public sealed partial class NullSpaceBlockerComponent : Component
{
    /// <summary>
    /// Should it BypassPVS?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool BypassPVS = false;

    /// <summary>
    /// Will force Unphase any ent that touches it.
    /// </summary>
    [DataField]
    public bool UnphaseOnCollide = true;
}
