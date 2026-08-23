// SPDX-License-Identifier: MIT
// Shadekin: ported from ss14Starlight/space-station-14 (MIT).

using System.Linq;
using Content.Server.Spawners.Components;
using Content.Shared._Starlight.Shadekin;
using Content.Shared.Tag;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Map;

namespace Content.Server._Starlight.Shadekin;

/// <summary>
///     Responde ao pedido do lado compartilhado, procurando um ponto de
///     nascimento em mapa marcado como a escuridão. Ver
///     PedirDestinoNaEscuridaoEvent para o motivo desta ponte existir.
/// </summary>
public sealed partial class DestinoNaEscuridaoSystem : EntitySystem
{
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PedirDestinoNaEscuridaoEvent>(OnPedido);
    }

    private void OnPedido(ref PedirDestinoNaEscuridaoEvent args)
    {
        var candidatos = new List<EntityCoordinates>();

        var query = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out var xform))
        {
            if (!_map.TryGetMap(xform.MapID, out var mapa))
                continue;

            if (!_tag.HasTag(mapa.Value, args.Marca))
                continue;

            candidatos.Add(xform.Coordinates);
        }

        if (candidatos.Count == 0)
            return;

        args.Destino = _random.Pick(candidatos);
    }
}
