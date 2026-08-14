using System.Linq;
using Content.Shared.EntityTable;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.EntityTable.ValueSelector;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._ES.EntityTable.EntitySelectors;

/// <summary>
/// Picks a specified number of spawns from all child-selector results.
/// </summary>
public sealed partial class ESPickSelector : EntityTableSelector
{
    public const string DataFieldTag = "pick";

    [DataField(DataFieldTag, required: true)]
    public EntityTableSelector Child;

    [DataField]
    public NumberSelector Amount = new ConstantNumberSelector(1);

    protected override IEnumerable<EntProtoId> GetSpawnsImplementation(
        IRobustRandom rand,
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        var pool = Child.GetSpawns(rand, entMan, proto, ctx).ToArray();
        return rand.GetItems(pool, Amount.Get(rand), allowDuplicates: false);
    }

    protected override IEnumerable<(EntProtoId spawn, double)> ListSpawnsImplementation(
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        return Child.ListSpawns(entMan, proto, ctx, Amount.Odds());
    }

    protected override IEnumerable<(EntProtoId spawn, double)> AverageSpawnsImplementation(
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        return Child.AverageSpawns(entMan, proto, ctx, Amount.Average());
    }
}
