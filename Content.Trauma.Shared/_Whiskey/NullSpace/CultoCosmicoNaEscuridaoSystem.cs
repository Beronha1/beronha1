// SPDX-License-Identifier: MIT
// Shadekin: portado de ss14Starlight/space-station-14 (MIT).

using Content.Shared._Starlight.NullSpace.Components;
using Content.Shared._Starlight.Shadekin;
using Content.Shared.Interaction.Events;
using Content.Trauma.Shared.CosmicCult.Components;

namespace Content.Trauma.Shared._Whiskey.NullSpace;

/// <summary>
///     Liga o culto cósmico da Whiskey nas regras da escuridão do Shadekin.
///
///     No Starlight isto morava no SharedShowNullSpaceSystem, porque lá o culto
///     e o Shadekin são da mesma assembly. Aqui o CosmicCultComponent é do Trauma
///     e o NullSpaceComponent é do Content.Shared, e só esta assembly vê os dois.
///
///     São duas regras, as mesmas do original: o cultista enxerga quem está na
///     escuridão mas não pode tocar nem bater, e um Shadekin que também seja
///     cultista não recupera a ação de portal quando o portal fecha.
/// </summary>
public sealed partial class CultoCosmicoNaEscuridaoSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void AoTentarBater(Entity<CosmicCultComponent> ent, ref AttackAttemptEvent args)
    {
        if (HasComp<NullSpaceComponent>(args.Target))
            args.Cancel();
    }

    [SubscribeLocalEvent]
    private void AoTentarInteragir(Entity<CosmicCultComponent> ent, ref InteractionAttemptEvent args)
    {
        if (HasComp<NullSpaceComponent>(args.Target))
            args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void AoPerguntarSeECultista(Entity<CosmicCultComponent> ent, ref EhCultistaCosmicoEvent args)
    {
        args.EhCultista = true;
    }
}
