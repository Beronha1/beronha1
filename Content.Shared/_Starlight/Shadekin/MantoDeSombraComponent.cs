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
}
