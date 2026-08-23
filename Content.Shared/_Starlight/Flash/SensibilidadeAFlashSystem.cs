// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared.Flash;

namespace Content.Shared._Starlight.Flash;

/// <summary> Aplica o multiplicador de duração de flash. </summary>
public sealed partial class SensibilidadeAFlashSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void AoCalcularDuracao(Entity<SensibilidadeAFlashComponent> ent, ref FlashDurationMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplicador;
    }
}
