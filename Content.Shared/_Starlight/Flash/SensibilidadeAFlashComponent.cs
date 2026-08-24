// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Robust.Shared.GameStates;

namespace Content.Shared._Starlight.Flash;

/// <summary>
///     Multiplica a duração do flash em quem tem este componente, e opcionalmente
///     faz o flash atravessar proteção de olho.
///
///     O olho do Shadekin enxerga no escuro e paga por isso: leva flash em dobro,
///     e por decisão da administração leva mesmo usando óculos ou máscara.
///
///     No Starlight isto era o FlashModifierComponent. Aqui o nome é outro para
///     não colidir, e o mecanismo é o FlashDurationMultiplierEvent, que este
///     fork já levanta.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SensibilidadeAFlashComponent : Component
{
    /// <summary> Quanto a duração do flash é multiplicada. </summary>
    [DataField]
    public float Multiplicador = 1f;

    /// <summary>
    ///     Se verdadeiro, proteção de olho não segura o flash. O olho dele é
    ///     sensível demais para óculos resolverem.
    /// </summary>
    [DataField]
    public bool AtravessaProtecao;
}
