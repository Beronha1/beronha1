// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared._Starlight.NullSpace.Components;
using Content.Shared.Interaction.Events;

namespace Content.Shared._Starlight.NullSpace.Systems;

public abstract partial class SharedShowNullSpaceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShowNullSpaceComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        SubscribeLocalEvent<ShowNullSpaceComponent, AttackAttemptEvent>(OnAttackAttempt);

        // Whiskey: o culto cósmico é de outra assembly e entra pelo
        // CultoCosmicoNaEscuridaoSystem, em Content.Trauma.Shared.
    }

    private void OnAttackAttempt(EntityUid uid, ShowNullSpaceComponent component, AttackAttemptEvent args)
    {
        if (HasComp<NullSpaceComponent>(args.Target))
            args.Cancel();
    }

    private void OnInteractionAttempt(EntityUid uid, ShowNullSpaceComponent component, ref InteractionAttemptEvent args)
    {
        if (HasComp<NullSpaceComponent>(args.Target))
            args.Cancelled = true;
    }
}
