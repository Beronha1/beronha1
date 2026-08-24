// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Robust.Shared.GameStates;

namespace Content.Shared._Starlight.NullSpace.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShowNullSpaceComponent : Component
{
    /// <summary>
    /// Should its show the shader of nullspace?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ShowShader = false;
}
