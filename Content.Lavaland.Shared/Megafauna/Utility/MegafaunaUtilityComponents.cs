// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Lavaland.Shared.Megafauna.Utility;

[RegisterComponent, NetworkedComponent]
public sealed partial class DensityCoreComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? AppliedTo;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class DensityCoreReceiverComponent : Component
{
    [DataField]
    public string SlotId = "density_core";

    [DataField]
    public int CapacityBonus = 30;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class DragonArmorComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Wearer;

    [ViewVariables(VVAccess.ReadOnly)]
    public int ProtectionGeneration;
}

/// <summary>
/// Tracks every megafauna item currently granting heat and lava protection to an entity.
/// Generation numbers let temporary effects refresh safely without an older timer removing a newer grant.
/// </summary>
[RegisterComponent]
public sealed partial class MegafaunaHeatProtectionSourcesComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<EntityUid, int> Sources = [];

    [ViewVariables(VVAccess.ReadOnly)]
    public bool PreserveFireImmunity;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool PreserveLavaWalking;

    [ViewVariables(VVAccess.ReadOnly)]
    public int NextGeneration;
}
