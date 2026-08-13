// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Lavaland.Shared.Megafauna.Utility;

/// <summary>
/// A processed batch of legion skulls that can grow a bounded, user-linked servitor.
/// </summary>
[RegisterComponent]
public sealed partial class LegionServitorCultureComponent : Component
{
    [DataField]
    public EntProtoId ServitorPrototype = "MobLegionServitor";

    [DataField]
    public int MaxActiveServitors = 2;
}

/// <summary>
/// Tracks living servitors created by one user so cultures cannot create unbounded NPCs.
/// </summary>
[RegisterComponent]
public sealed partial class LegionServitorControllerComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> Servitors = [];
}

/// <summary>
/// Links a cultured servitor to its creator for cleanup and follow behaviour.
/// </summary>
[RegisterComponent]
public sealed partial class CulturedLegionServitorComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Creator;
}
