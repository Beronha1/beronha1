// SPDX-License-Identifier: MIT
// Shadekin: portado de ss14Starlight/space-station-14 (MIT).

using Content.Server.Atmos.EntitySystems;
using Content.Shared._Starlight.Immunity;
using Content.Shared.Atmos;

namespace Content.Server._Starlight.Immunity;

/// <summary>
///     Faz o PressureImmunityComponent valer de verdade. Copia o desenho do
///     PressureImmunityStatusEffectSystem: responde ao evento de atualização
///     dizendo que é imune, e pede recálculo quando o componente entra ou sai,
///     porque o BarotraumaComponent guarda o resultado em cache.
/// </summary>
public sealed partial class ImunidadePressaoSystem : EntitySystem
{
    [Dependency] private BarotraumaSystem _barotrauma = default!;

    [SubscribeLocalEvent]
    private void AoGanhar(Entity<PressureImmunityComponent> ent, ref ComponentStartup args)
    {
        _barotrauma.RefreshPressureImmunity(ent);
    }

    [SubscribeLocalEvent]
    private void AoPerder(Entity<PressureImmunityComponent> ent, ref ComponentShutdown args)
    {
        _barotrauma.RefreshPressureImmunity(ent);
    }

    [SubscribeLocalEvent]
    private void AoAtualizar(Entity<PressureImmunityComponent> ent, ref RefreshPressureImmunityEvent args)
    {
        args.IsImmune = true;
    }
}
