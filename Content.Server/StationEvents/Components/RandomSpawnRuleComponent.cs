using Content.Server.StationEvents.Events;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.StationEvents.Components;

/// <summary>
/// Spawns a single entity at a random tile on a station using TryGetRandomTile.
/// </summary>
[RegisterComponent, Access(typeof(RandomSpawnRule))]
public sealed partial class RandomSpawnRuleComponent : Component
{
    /// <summary>
    /// The entity to be spawned.
    /// </summary>
    [DataField("prototype", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Prototype = string.Empty;

    /// <summary>
    /// Optional radio message sent by the spawned entity, with its nearest navigation location.
    /// </summary>
    [DataField]
    public RandomSpawnRuleRadioMessage? RadioMessage;
}

/// <param name="Channel">Radio channel used for the message.</param>
/// <param name="Message">Localized message with a <c>location</c> argument.</param>
[DataRecord]
public sealed partial record RandomSpawnRuleRadioMessage(
    [field: DataField(required: true)] ProtoId<RadioChannelPrototype> Channel,
    [field: DataField(required: true)] LocId Message);
