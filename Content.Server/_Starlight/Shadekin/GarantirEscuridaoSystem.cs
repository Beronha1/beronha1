// SPDX-License-Identifier: MIT
// Shadekin: ported from ss14Starlight/space-station-14 (MIT).

using Content.Server.GameTicking;
using Content.Shared._Starlight.Shadekin;

namespace Content.Server._Starlight.Shadekin;

/// <summary>
///     Atende o pedido do lado compartilhado e inicia a regra do mapa da
///     escuridão, que é onde o Shadekin se refugia. Ver GarantirEscuridaoEvent
///     para o motivo de existir esta ponte.
/// </summary>
public sealed partial class GarantirEscuridaoSystem : EntitySystem
{
    [Dependency] private GameTicker _gameTicker = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GarantirEscuridaoEvent>(OnGarantir);
    }

    private void OnGarantir(ref GarantirEscuridaoEvent args)
        => _gameTicker.StartGameRule(args.Regra);
}
