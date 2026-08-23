// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Robust.Shared.GameStates;

namespace Content.Shared._Starlight.Shadekin.Components;

/// <summary>
/// Protect the Ent or Wearer of the Ent from suffering from "The Dark" effect.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TheDarkImmuneComponent : Component
{
    [DataField]
    public bool Ranged;
}
