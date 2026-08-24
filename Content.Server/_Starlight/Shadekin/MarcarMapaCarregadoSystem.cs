// SPDX-FileCopyrightText: 2024-2026 Starlight
// SPDX-FileCopyrightText: 2026 Whiskey Station Contributors
// SPDX-License-Identifier: MIT
//
// Portado de https://github.com/ss14Starlight/space-station-14

using Content.Server.GameTicking.Rules;
using Content.Shared.Tag;
using Robust.Server.GameObjects;

namespace Content.Server._Starlight.Shadekin;

/// <summary> Aplica a marca assim que a regra termina de carregar o mapa. </summary>
public sealed partial class MarcarMapaCarregadoSystem : EntitySystem
{
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private MapSystem _map = default!;

    [SubscribeLocalEvent]
    private void AoCarregarMapa(Entity<MarcarMapaCarregadoComponent> ent, ref RuleLoadedGridsEvent args)
    {
        if (!_map.TryGetMap(args.Map, out var mapa))
            return;

        _tag.AddTag(mapa.Value, ent.Comp.Marca);
    }
}
