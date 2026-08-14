// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Lavaland.Shared.MobPhases;

/// <summary>
/// Configurable ambient and transition dialogue for a phased megafauna.
/// The server deliberately tracks phase changes locally instead of coupling
/// dialogue to the damage system's event ordering.
/// </summary>
[RegisterComponent]
public sealed partial class MegafaunaPhaseDialogueComponent : Component
{
    [DataField(required: true)]
    public Dictionary<int, MegafaunaPhaseDialogueEntry> Phases = new();

    [DataField]
    public float MinimumInterval = 12f;

    [DataField]
    public float MaximumInterval = 22f;

    [ViewVariables]
    public int LastPhase;

    [ViewVariables]
    public TimeSpan NextLineAt;
}

[DataDefinition]
public sealed partial class MegafaunaPhaseDialogueEntry
{
    [DataField]
    public List<LocId> Lines = new();

    [DataField]
    public LocId? TransitionLine;
}
