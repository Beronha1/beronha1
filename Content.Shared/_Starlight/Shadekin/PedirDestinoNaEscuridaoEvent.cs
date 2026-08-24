// SPDX-License-Identifier: MIT
// Shadekin: ported from ss14Starlight/space-station-14 (MIT).

using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Shared.Tag;

namespace Content.Shared._Starlight.Shadekin;

/// <summary>
///     Pede um lugar para onde mandar o Shadekin quando ele deveria morrer.
///
///     No Starlight o próprio sistema compartilhado varria os pontos de
///     nascimento procurando um em mapa marcado como a escuridão. Aqui o
///     SpawnPointComponent só existe no servidor, então o compartilhado pede e o
///     servidor responde. Quem responde é o DestinoNaEscuridaoSystem.
///
///     Destino nulo significa que não há para onde ir, e aí o Shadekin morre de
///     verdade, que é o mesmo desfecho do original.
/// </summary>
[ByRefEvent]
public record struct PedirDestinoNaEscuridaoEvent(ProtoId<TagPrototype> Marca)
{
    public EntityCoordinates? Destino = null;
}
