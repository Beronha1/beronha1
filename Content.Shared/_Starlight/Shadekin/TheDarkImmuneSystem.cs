// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared._Starlight.Shadekin.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Robust.Shared.Timing;

namespace Content.Shared._Starlight.Shadekin;

public sealed partial class TheDarkImmuneSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TheDarkImmuneComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<TheDarkImmuneComponent, GotUnequippedEvent>((_, ref args) => RemComp<TheDarkImmuneComponent>(args.EquipTarget));
    }

    private void OnEquipped(EntityUid uid, TheDarkImmuneComponent component, GotEquippedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (!TryComp<ClothingComponent>(uid, out var clothing)
            || !clothing.Slots.HasFlag(args.SlotFlags))
            return;

        EnsureComp<TheDarkImmuneComponent>(args.EquipTarget);
    }
}
