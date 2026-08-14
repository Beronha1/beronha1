using Content.Shared.EntityTable;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._ES.EntityTable.EntitySelectors;

/// <summary>
/// EphemeralSpace shorthand equivalent of <see cref="NestedSelector"/>.
/// </summary>
public sealed partial class ESNestedSelector : EntityTableSelector
{
    public const string DataFieldTag = "tableId";

    [DataField(DataFieldTag, required: true)]
    public ProtoId<EntityTablePrototype> TableId;

    protected override IEnumerable<EntProtoId> GetSpawnsImplementation(
        IRobustRandom rand,
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        return proto.Index(TableId).Table.GetSpawns(rand, entMan, proto, ctx);
    }

    protected override IEnumerable<(EntProtoId spawn, double)> ListSpawnsImplementation(
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        return proto.Index(TableId).Table.ListSpawns(entMan, proto, ctx);
    }

    protected override IEnumerable<(EntProtoId spawn, double)> AverageSpawnsImplementation(
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        return proto.Index(TableId).Table.AverageSpawns(entMan, proto, ctx);
    }
}
