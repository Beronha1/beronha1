// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared.Actions;

namespace Content.Shared._Starlight.NullSpace.Components;

[RegisterComponent]
public sealed partial class NullPhaseComponent : Component
{
    [DataField]
    public EntityUid? PhaseAction;

    [DataField] public bool PreventLightFlicker;
    public bool OriginalFlickerFlagState;
}

public sealed partial class NullPhaseActionEvent : InstantActionEvent { }
