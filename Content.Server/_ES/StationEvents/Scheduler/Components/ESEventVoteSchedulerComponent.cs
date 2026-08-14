using Content.Shared._ES.Voting.Components;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._ES.StationEvents.Scheduler.Components;

[RegisterComponent, AutoGenerateComponentPause]
[Access(typeof(ESEventVoteSchedulerSystem))]
public sealed partial class ESEventVoteSchedulerComponent : Component
{
    [DataField(required: true)]
    public EntityTableSelector EventTable = new NoneSelector();

    [DataField(required: true)]
    public EntProtoId<ESVoteComponent> VotePrototype;

    [DataField]
    public int VoteOptions = 4;

    [DataField]
    public TimeSpan BaseFirstEventDelay = TimeSpan.FromMinutes(2.5f);

    [DataField]
    public TimeSpan MinEventDelay = TimeSpan.FromMinutes(3f);

    [DataField]
    public TimeSpan MaxEventDelay = TimeSpan.FromMinutes(10f);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextEventTime;
}
