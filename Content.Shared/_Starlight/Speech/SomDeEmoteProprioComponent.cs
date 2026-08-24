// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT

using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.Speech;

/// <summary>
///     Dá à criatura um conjunto de som de emote próprio, independente da voz que
///     o jogador escolheu na criação de personagem.
///
///     Existe porque nesta base o som do emote não fica no emote: ele vem do
///     EmoteSoundsPrototype que o VocalComponent aponta, e esse aponta para a voz
///     do perfil. Sem isto, dar som ao "marr" do Shadekin exigiria mexer na lista
///     de vozes da espécie, o que muda o seletor da criação de personagem.
///
///     O gancho é o GetEmoteSoundsEvent, que o VocalSystem levanta antes de cair
///     na voz do perfil. Mesmo caminho que o manto de sombra do herege usa.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SomDeEmoteProprioComponent : Component
{
    /// <summary> Conjunto usado por personagem masculino. </summary>
    [DataField(required: true)]
    public ProtoId<EmoteSoundsPrototype> Masculino;

    /// <summary> Conjunto usado por personagem feminino. </summary>
    [DataField(required: true)]
    public ProtoId<EmoteSoundsPrototype> Feminino;

    /// <summary> Conjunto usado por quem não tem sexo definido. Cai no masculino se vazio. </summary>
    [DataField]
    public ProtoId<EmoteSoundsPrototype>? SemSexo;
}
