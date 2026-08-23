// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared._Starlight.Shadekin.Components;
using Content.Shared.Alert.Components;

namespace Content.Client._Starlight.Shadekin;

public sealed partial class ShadekinSystem : EntitySystem
{
    public void InitializeBrighteye()
        => SubscribeLocalEvent<BrighteyeComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);

    private void OnGetCounterAmount(Entity<BrighteyeComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.BrighteyeAlert != args.Alert)
            return;

        args.Amount = ent.Comp.Energy;
    }
}
