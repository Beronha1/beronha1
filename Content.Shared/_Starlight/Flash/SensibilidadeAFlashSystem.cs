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

        // Whiskey: aqui existia um segundo tratador que rodava depois do
        // SharedFlashSystem e fazia args.Cancelled = false, para "furar óculos".
        // Foi retirado, e a crítica que motivou isso estava certa: Cancelled não
        // guarda quem cancelou. Descancelar não significa "ignorar proteção de
        // olho", significa "apagar QUALQUER motivo de cancelamento". Hoje
        // coincidia com a proteção; amanhã furaria imunidade de antag, estado
        // especial ou regra nova, sem ninguém perceber.
        //
        // Fazer isso direito exige provenance, ou seja o próprio SharedFlashSystem
        // consultar uma marca antes de deixar a proteção cancelar. Isso é mudança
        // em arquivo base e precisa de decisão de quem mantém o fork.
    }

    private void AoCalcularDuracao(Entity<SensibilidadeAFlashComponent> ent, ref FlashDurationMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplicador;
    }
}
