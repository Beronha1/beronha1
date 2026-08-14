using System.Linq;
using Content.Shared.EntityTable;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._ES.EntityTable.EntitySelectors;

/// <summary>
/// EphemeralSpace shorthand equivalent of <see cref="GroupSelector"/>.
/// </summary>
public sealed partial class ESGroupSelector : EntityTableSelector
{
    public const string DataFieldTag = "group";

    [DataField(DataFieldTag, required: true)]
    public List<EntityTableSelector> Children = new();

    protected override IEnumerable<EntProtoId> GetSpawnsImplementation(
        IRobustRandom rand,
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        var children = new Dictionary<EntityTableSelector, float>(Children.Count);
        foreach (var child in Children)
        {
            if (!child.CheckConditions(entMan, proto, ctx))
                continue;

            children.Add(child, child.Weight);
        }

        if (children.Count == 0)
            return Array.Empty<EntProtoId>();

        var pick = SharedRandomExtensions.Pick(children, rand);
        return pick.GetSpawns(rand, entMan, proto, ctx);
    }

    protected override IEnumerable<(EntProtoId spawn, double)> ListSpawnsImplementation(
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        var totalWeight = Children.Sum(x => x.Weight);
        foreach (var child in Children)
        {
            foreach (var spawn in child.ListSpawns(entMan, proto, ctx, child.Weight / totalWeight))
                yield return spawn;
        }
    }

    protected override IEnumerable<(EntProtoId spawn, double)> AverageSpawnsImplementation(
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        var totalWeight = Children.Sum(x => x.Weight);
        foreach (var child in Children)
        {
            foreach (var spawn in child.AverageSpawns(entMan, proto, ctx, child.Weight / totalWeight))
                yield return spawn;
        }
    }
}
