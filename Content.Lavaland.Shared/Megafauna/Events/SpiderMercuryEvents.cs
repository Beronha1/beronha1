// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Shared.Megafauna.Events;

public sealed partial class EtherDrainEvent : InstantActionEvent;

public sealed partial class AddComponentActionEvent : InstantActionEvent
{
    [DataField(required: true)]
    public ComponentRegistry TargetComponent = new();

    [DataField]
    public bool RemoveAfterTimer;

    [DataField]
    public TimeSpan TimeToRemoval = TimeSpan.FromSeconds(5);
}

public sealed partial class CosmicRayCirculatorActionEvent : InstantActionEvent;

public sealed partial class EnvironmentalResonanceActionEvent : InstantActionEvent
{
    [DataField]
    public bool Vertical;
}

public sealed partial class ORTSolarStormActionEvent : InstantActionEvent;
public sealed partial class ParadigmInflationActionEvent : EntityTargetActionEvent;
public sealed partial class PhaseConversionActionEvent : InstantActionEvent;
public sealed partial class ReflectiveThreadsActionEvent : InstantActionEvent;
public sealed partial class OrbitingRingActionEvent : InstantActionEvent;
public sealed partial class ORTConvergenceActionEvent : InstantActionEvent;
