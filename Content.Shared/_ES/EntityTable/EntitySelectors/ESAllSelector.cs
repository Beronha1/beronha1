using Content.Shared.EntityTable;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._ES.EntityTable.EntitySelectors;

/// <summary>
/// EphemeralSpace shorthand equivalent of <see cref="AllSelector"/>.
/// </summary>
public sealed partial class ESAllSelector : EntityTableSelector
{
    public const string DataFieldTag = "all";

    [DataField(DataFieldTag, required: true)]
    public List<EntityTableSelector> Children;

    protected override IEnumerable<EntProtoId> GetSpawnsImplementation(
        IRobustRandom rand,
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        foreach (var child in Children)
        {
            foreach (var spawn in child.GetSpawns(rand, entMan, proto, ctx))
                yield return spawn;
        }
    }

    protected override IEnumerable<(EntProtoId spawn, double)> ListSpawnsImplementation(
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        foreach (var child in Children)
        {
            foreach (var spawn in child.ListSpawns(entMan, proto, ctx))
                yield return spawn;
        }
    }

    protected override IEnumerable<(EntProtoId spawn, double)> AverageSpawnsImplementation(
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        foreach (var child in Children)
        {
            foreach (var spawn in child.AverageSpawns(entMan, proto, ctx))
                yield return spawn;
        }
    }
}
