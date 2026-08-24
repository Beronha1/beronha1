// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.Shadekin.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DarkBreacherComponent : Component
{
    [DataField]
    public EntProtoId Portal = "PortalDarkBreacher";

    [DataField]
    public float SpawnDistance = 500f;
}
