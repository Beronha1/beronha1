// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server._Starlight.Shadekin;

/// <summary>
///     Põe uma tag no mapa que a regra acabou de carregar.
///
///     No Starlight isto era o campo mapTag do LoadMapRule. O LoadMapRule daqui
///     não tem esse campo, e o Shadekin depende dele: o DestinoNaEscuridaoSystem
///     acha para onde teleportar procurando ponto de spawn em mapa marcado.
/// </summary>
[RegisterComponent]
public sealed partial class MarcarMapaCarregadoComponent : Component
{
    /// <summary> A tag posta no mapa. </summary>
    [DataField(required: true)]
    public ProtoId<TagPrototype> Marca;
}
