// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Robust.Client.Graphics;

namespace Content.Client._Starlight.Overlay.Overlays;

public sealed class NullSpaceOverlay : BaseVisionOverlay
{
    public NullSpaceOverlay(ShaderPrototype shader) : base(shader)
        => ZIndex = (int?)OverlayZIndexes.NullSpace;
}
