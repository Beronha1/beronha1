// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.Shadekin;

/// <summary>
///     Deixa a criatura apagar uma lâmpada por perto, estendendo a mão.
///
///     É a única habilidade do Shadekin comum que FABRICA a vantagem dele em vez
///     de só reagir ao ambiente: apagar a luz do corredor transforma um lugar
///     ruim em lugar bom. O preço é que lâmpada quebrada é sabotagem visível, a
///     tripulação percebe e o eletricista vai trocar.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ApagarLuzComponent : Component
{
    /// <summary> Alcance em tiles. </summary>
    [DataField]
    public float Alcance = 4f;

    /// <summary> A ação concedida ao nascer. </summary>
    [DataField]
    public EntProtoId Acao = "ShadekinApagarLuzAction";

    [DataField]
    public EntityUid? AcaoEntidade;
}

/// <summary> Disparado quando a criatura usa a habilidade. </summary>
public sealed partial class ApagarLuzActionEvent : InstantActionEvent;
