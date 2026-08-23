// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared.NPC.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.NullSpace.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class NullSpaceComponent : Component
{
    public List<ProtoId<NpcFactionPrototype>> SuppressedFactions = new();
}
