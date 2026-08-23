// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

namespace Content.Client._Starlight.Shadekin;

public sealed partial class ShadekinSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        InitializeBrighteye();
    }
}
