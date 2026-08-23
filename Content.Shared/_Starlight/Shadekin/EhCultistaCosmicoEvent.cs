// SPDX-License-Identifier: MIT
// Shadekin: portado de ss14Starlight/space-station-14 (MIT).

namespace Content.Shared._Starlight.Shadekin;

/// <summary>
///     Pergunta se a entidade é cultista cósmico. O componente do culto vive em
///     Content.Trauma.Shared, que o Content.Shared não enxerga, então quem responde
///     é o CultoCosmicoNaEscuridaoSystem, do lado do Trauma. Ninguém respondendo
///     significa "não é cultista", que é o padrão correto.
/// </summary>
[ByRefEvent]
public record struct EhCultistaCosmicoEvent
{
    public bool EhCultista = false;

    public EhCultistaCosmicoEvent() { }
}
