// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared.Light.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Starlight.Light;

/// <summary>
/// Added to a <see cref="PoweredLightComponent"/> entity when an alert level affects its brightness.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AlertLevelDimmedLightComponent : Component
{
    [DataField] public float LightEnergyMultiplier = 1.0f;
}
