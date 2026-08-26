using Content.Server.Animals.Components;
using Content.Shared.EntityTable;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server.Animals.Systems;

/// <summary>
/// Handles producing configured entities in response to production attempts.
/// </summary>
public sealed partial class EntityProducerSystem : EntitySystem
{
    [Dependency] private EntityTableSystem _entityTable = default!;
    [Dependency] private TagSystem _tag = default!;

    [SubscribeLocalEvent]
    private void OnProduce(Entity<EntityProducerComponent> ent, ref ProductionAttemptEvent args)
    {
        if (ent.Comp.PopulationCap is { } populationCap &&
            ent.Comp.PopulationCapTag is { } populationTag &&
            CountPopulationOnMap(args.Owner, populationTag) >= populationCap)
        {
            return;
        }

        var produced = new List<EntityUid>();

        foreach (var spawn in _entityTable.GetSpawns(ent.Comp.Table))
        {
            produced.Add(SpawnNextToOrDrop(spawn, args.Owner));
        }

        if (produced.Count == 0)
            return;

        args.Produced = true;

        var ev = new EntitiesProducedEvent(args.Owner, produced);
        RaiseLocalEvent(ent.Owner, ref ev);
    }

    private int CountPopulationOnMap(EntityUid source, ProtoId<TagPrototype> tag)
    {
        var mapId = Transform(source).MapID;
        var count = 0;
        var query = EntityQueryEnumerator<TagComponent, TransformComponent>();

        while (query.MoveNext(out _, out var tags, out var transform))
        {
            if (transform.MapID == mapId && _tag.HasTag(tags, tag))
                count++;
        }

        return count;
    }
}
