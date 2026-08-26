using Content.Server.Animals.Systems;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server.Animals.Components;

/// <summary>
/// Spawns configured entities when production is requested.
/// </summary>
[RegisterComponent, Access(typeof(EntityProducerSystem))]
public sealed partial class EntityProducerComponent : Component
{
    /// <summary>
    /// Selects the entities spawned for each production attempt.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector Table = default!;

    /// <summary>
    /// Optional map-wide cap for entities carrying <see cref="PopulationCapTag"/>.
    /// Prevents self-replicating mobs from growing without bound.
    /// </summary>
    [DataField]
    public int? PopulationCap;

    /// <summary>
    /// Tag counted when enforcing <see cref="PopulationCap"/>.
    /// </summary>
    [DataField]
    public ProtoId<TagPrototype>? PopulationCapTag;
}

/// <summary>
/// Raised after entity production succeeds with the entities that were spawned.
/// </summary>
/// <param name="Owner">Entity on whose behalf the entities were produced.</param>
/// <param name="Entities">Entities spawned by the successful production attempt.</param>
[ByRefEvent]
public readonly record struct EntitiesProducedEvent(EntityUid Owner, IReadOnlyList<EntityUid> Entities);
