// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared.NPC.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.NullSpace.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class NullSpaceComponent : Component
{
    public List<ProtoId<NpcFactionPrototype>> SuppressedFactions = new();

    /// <summary>
    ///     Whiskey: guarda quais componentes o NullSpace realmente acrescentou nesta
    ///     entidade, para tirar só o que ele mesmo pôs.
    ///
    ///     Sem isto o sistema fazia EnsureComp ao entrar e RemComp ao sair, cego.
    ///     Isso só funciona enquanto existir exatamente uma fonte daquele estado: se
    ///     a pessoa já tinha imunidade a pressão por traje, por espécie ou por outro
    ///     sistema, sair da escuridão apagava a dela também.
    ///
    ///     É o mesmo padrão que o FunctionalOrgan do Starlight usa, que guarda o que
    ///     instalou para a extração remover só aquilo.
    /// </summary>
    [ViewVariables]
    public HashSet<Type> Instalados = new();
}
