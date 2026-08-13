// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.EntityShapes.Shapes;
using Content.Lavaland.Shared.Megafauna.Systems;
using Robust.Shared.Map;

namespace Content.Lavaland.Shared.Megafauna.Components;

/// <summary>
/// Generates a square field  around the megafauna then it starts attacking.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MegafaunaFieldGeneratorComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool Enabled;

    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> Walls = new();

    [DataField(required: true)]
    public EntityShape WallShape;

    [DataField, AutoNetworkedField]
    public EntProtoId WallId;

    /// <summary>
    /// Coordinates where the current field was created. The boss can move far
    /// away during an encounter, so cleanup cannot safely use its death position.
    /// </summary>
    [ViewVariables]
    public EntityCoordinates? FieldOrigin;
}

/// <summary>
/// Runtime ownership for a wall created by a megafauna field. This lets field
/// cleanup recover even if the generator's replicated wall list is stale or a
/// predicted wall was not reconciled into that list.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(MegafaunaFieldSystem))]
public sealed partial class MegafaunaFieldWallComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid Generator;
}
