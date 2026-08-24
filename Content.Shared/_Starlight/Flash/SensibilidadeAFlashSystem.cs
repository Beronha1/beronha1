// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared.Flash;

namespace Content.Shared._Starlight.Flash;

/// <summary> Aplica o multiplicador de duração e, se pedido, fura a proteção. </summary>
public sealed partial class SensibilidadeAFlashSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SensibilidadeAFlashComponent, FlashDurationMultiplierEvent>(AoCalcularDuracao);

        // roda depois do SharedFlashSystem de propósito: a proteção cancela lá, e
        // aqui a gente descancela. Sem essa ordem, o cancelamento viria por último
        // e o efeito não valeria.
        SubscribeLocalEvent<SensibilidadeAFlashComponent, FlashAttemptEvent>(AoTentarFlash,
            after: new[] { typeof(SharedFlashSystem) });
    }

    private void AoCalcularDuracao(Entity<SensibilidadeAFlashComponent> ent, ref FlashDurationMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplicador;
    }

    private void AoTentarFlash(Entity<SensibilidadeAFlashComponent> ent, ref FlashAttemptEvent args)
    {
        if (ent.Comp.AtravessaProtecao)
            args.Cancelled = false;
    }
}
