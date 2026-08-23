// SPDX-License-Identifier: MIT
// Shadekin: portado de ss14Starlight/space-station-14 (MIT).

using Content.Shared.Temperature;

namespace Content.Shared._Starlight.Immunity;

/// <summary>
///     Cancela a troca de calor de quem tem imunidade. Mesmo gancho que o
///     TemperatureImmunitySystem do Trauma usa, só que sem separar frio de calor.
/// </summary>
public sealed partial class ImunidadeTemperaturaSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void AoTrocarCalor(Entity<TemperatureImmunityComponent> ent, ref BeforeHeatExchangeEvent args)
    {
        args.Cancelled = true;
    }
}
