using Content.Shared._ES.Voting.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._ES.StationEvents.Scheduler.Components;

[RegisterComponent]
[Access(typeof(ESEventVoteSchedulerSystem))]
public sealed partial class ESEventVoteComponent : Component
{
    [DataField]
    public List<EntProtoId> EventIds = new();
}
