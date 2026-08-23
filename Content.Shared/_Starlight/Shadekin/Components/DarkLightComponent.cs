// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Robust.Shared.GameStates;

namespace Content.Shared._Starlight.Shadekin.Components;

/// <summary>
/// DarkLight Ents will be ingored by the "Light Sensetivity Check"
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DarkLightComponent : Component;
