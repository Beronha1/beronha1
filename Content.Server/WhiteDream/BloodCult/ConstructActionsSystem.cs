using Content.Goobstation.Shared.PhaseShift;
using Content.Shared.StatusEffect;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.WhiteDream.BloodCult;

public sealed partial class ConstructActionsSystem : EntitySystem
{
    [Dependency] private ITileDefinitionManager _tileDef = default!;

    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private MapSystem _mapSystem = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    private const string CultTileSpawnEffect = "CultTileSpawnEffect";

    public override void Initialize()
    {
        SubscribeLocalEvent<PlaceTileEntityEvent>(OnPlaceTileEntityEvent);
        SubscribeLocalEvent<PhaseShiftEvent>(OnPhaseShift);
    }

    private void OnPlaceTileEntityEvent(PlaceTileEntityEvent args)
    {
        if (args.Handled)
            return;

        if (args.EntityProto is { } entProtoId)
            Spawn(entProtoId, args.Target);

        if (args.TileId is { } tileId)
        {
            if (_transform.GetGrid(args.Target) is not { } grid || !TryComp(grid, out MapGridComponent? mapGrid))
                return;

            var tileDef = _tileDef[tileId];
            var tile = new Tile(tileDef.TileId);

            _mapSystem.SetTile(grid, mapGrid, args.Target, tile);
            Spawn(CultTileSpawnEffect, args.Target);
        }

        if (args.Audio is { } audio)
            _audio.PlayPvs(audio, args.Target);

        args.Handled = true;
    }

    private void OnPhaseShift(PhaseShiftEvent args)
    {
        if (args.Handled)
            return;

        if (_statusEffects.TryAddStatusEffect<PhaseShiftedComponent>(
            args.Performer,
            args.StatusEffectId,
            args.Duration,
            false))
            args.Handled = true;
    }
}
