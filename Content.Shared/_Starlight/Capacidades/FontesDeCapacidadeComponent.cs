// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT

namespace Content.Shared._Starlight.Capacidades;

/// <summary>
///     Registra QUEM concedeu cada capacidade a esta entidade.
///
///     Existe porque o padrão EnsureComp ao equipar e RemComp ao desequipar só
///     funciona enquanto houver exatamente uma fonte daquele estado. Dois
///     equipamentos que concedem a mesma coisa: tirar um apagava a capacidade
///     inteira, com o outro ainda vestido. E se a entidade já tinha aquilo por
///     espécie, antagonista ou outro sistema, o desequipar apagava o dela.
///
///     Aqui cada capacidade guarda o conjunto de fontes que a concederam. Ela só
///     é retirada quando o conjunto esvazia, e nunca é retirada se ninguém a
///     concedeu, ou seja se ela já existia antes.
/// </summary>
[RegisterComponent]
public sealed partial class FontesDeCapacidadeComponent : Component
{
    /// <summary> Nome do componente concedido para o conjunto de entidades que o concederam. </summary>
    [ViewVariables]
    public Dictionary<string, HashSet<EntityUid>> Fontes = new();
}
