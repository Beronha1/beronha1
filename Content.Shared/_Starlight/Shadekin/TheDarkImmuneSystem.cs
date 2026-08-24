using Content.Shared._Starlight.Capacidades;
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
    [Dependency] private FontesDeCapacidadeSystem _capacidades = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TheDarkImmuneComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<TheDarkImmuneComponent, GotUnequippedEvent>((uid, _, ref args) => _capacidades.Devolver<TheDarkImmuneComponent>(args.EquipTarget, uid));
    }

    private void OnEquipped(EntityUid uid, TheDarkImmuneComponent component, GotEquippedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (!TryComp<ClothingComponent>(uid, out var clothing)
            || !clothing.Slots.HasFlag(args.SlotFlags))
            return;

        _capacidades.Conceder<TheDarkImmuneComponent>(args.EquipTarget, uid);
    }
}
