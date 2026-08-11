// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Utility;

namespace Content.Lavaland.Shared.Chasm.Teleport;

/// <summary>
/// Loads a destination map when an entity finishes falling into this chasm.
/// Ported from Goobstation PR #6542.
/// </summary>
[RegisterComponent]
public sealed partial class ChasmTeleportComponent : Component
{
    [DataField]
    public ResPath MapPath;

    public EntityUid? LoadedMap;
}
