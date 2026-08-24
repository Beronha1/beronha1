// SPDX-License-Identifier: MIT
// Shadekin: ported from ss14Starlight/space-station-14 (MIT).

using Content.Shared.GameTicking.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.Shadekin;

/// <summary>
///     Pedido para garantir que o mapa da escuridão existe.
///
///     No Starlight o ShadekinSystem chamava o GameTicker direto, porque o
///     SharedGameTicker deles expõe StartGameRule. O da Whiskey não expõe: iniciar
///     regra de jogo mora no GameTicker do servidor. Então o lado compartilhado
///     pede, e o servidor faz.
/// </summary>
[ByRefEvent]
public record struct GarantirEscuridaoEvent(EntProtoId<GameRuleComponent> Regra);
