// SPDX-License-Identifier: MIT
// Shadekin: portado de ss14Starlight/space-station-14 (MIT).

using Robust.Shared.GameStates;

namespace Content.Shared._Starlight.Immunity;

/// <summary>
///     Ignora dano de pressão enquanto estiver presente. No Starlight isto vinha
///     do Atmos base; aqui vive junto do porte para não mexer em namespace da
///     Whiskey. Quem faz valer é o ImunidadePressaoSystem, no servidor.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PressureImmunityComponent : Component;

/// <summary>
///     Ignora troca de calor com o ambiente enquanto estiver presente. O Trauma
///     já tem o par SpecialLowTempImmunity e SpecialHighTempImmunity, mas os dois
///     moram em Content.Trauma.Shared, que o Content.Server não enxerga. Este aqui
///     usa o mesmo evento e vale para frio e calor de uma vez.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TemperatureImmunityComponent : Component;
