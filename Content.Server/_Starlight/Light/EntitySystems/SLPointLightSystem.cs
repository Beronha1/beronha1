// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14
//
// Este sistema tinha ficado de fora do porte, e a falta dele matava a mecânica
// inteira de luz do Shadekin em silêncio. O SLPointLightComponent é um marcador
// que existe porque a luz de verdade é dividida entre cliente e servidor, e
// código compartilhado não consegue buscar por ela. Sem alguém pondo o marcador,
// a busca voltava vazia, a exposição à luz dava sempre zero e o Shadekin ficava
// eternamente no estado Escuro: curando sempre, nunca tomando dano de luz e
// nunca perdendo a corrida.

using Content.Shared._Starlight.Light;
using Robust.Server.GameObjects;

namespace Content.Server._Starlight.Light.EntitySystems;

/// <summary> Marca toda fonte de luz, para o código compartilhado poder achá-la. </summary>
public sealed partial class SLPointLightSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void AoNascer(Entity<PointLightComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<SLPointLightComponent>(ent);
    }

    [SubscribeLocalEvent]
    private void AoSair(Entity<PointLightComponent> ent, ref ComponentRemove args)
    {
        if (!TerminatingOrDeleted(ent))
            RemCompDeferred<SLPointLightComponent>(ent);
    }
}
