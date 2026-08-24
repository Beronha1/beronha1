// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Starlight.Shadekin;

/// <summary>
///     Enquanto estiver no escuro, a criatura fica parcialmente invisível.
///
///     Quem faz o resto do trabalho é o StealthOnMove, que já existe na fork: ele
///     apaga mais quem fica parado e revela quem anda. Por isso o manto não é
///     capa de invisibilidade, é recompensa para quem se esconde e espera.
///
///     Some no instante em que a luz sobe, então não serve em corredor aceso.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MantoDeSombraComponent : Component
{
    /// <summary> Visibilidade ao entrar no escuro. 0 é invisível, 1 é normal. </summary>
    [DataField]
    public float VisibilidadeNoEscuro = 0.4f;

    /// <summary> Se o manto está valendo agora. Controlado pelo sistema. </summary>
    [ViewVariables]
    public bool Ativo;

    /// <summary>
    ///     Se foi o manto que pôs a furtividade nesta entidade.
    ///
    ///     Existe porque o NullSpace também concede Stealth, com outro valor. Sem
    ///     esta marca, o manto apagaria a furtividade do NullSpace ao clarear, e
    ///     vice-versa. Quem não concedeu não tira.
    /// </summary>
    [ViewVariables]
    public bool ConcedeuFurtividade;
}
