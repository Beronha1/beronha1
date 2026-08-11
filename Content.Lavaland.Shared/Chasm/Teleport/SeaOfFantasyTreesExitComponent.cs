// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Lavaland.Shared.Chasm.Teleport;

/// <summary>
/// Returns entities from the Mercury arena to its Lavaland fissure.
/// Ported from Goobstation PR #6542.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SeaOfFantasyTreesExitComponent : Component;
