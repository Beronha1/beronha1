// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Server.NPC.Systems;
using Content.Shared.Actions.Components;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Server.NPC;

/// <summary>
/// This is used for an NPC that constantly tries to use an actions on a given target.
/// </summary>
[RegisterComponent, Access(typeof(NPCUseActionsOnTargetSystem))]
public sealed partial class NPCUseActionsOnTargetComponent : Component
{
    /// <summary>
    /// HTN blackboard key for the target entity
    /// </summary>
    [DataField]
    public string TargetKey = "Target";

    /// <summary>
    /// Actions that's going to attempt to be used.
    /// </summary>
    [DataField(required: true)]
    public List<EntProtoId<TargetActionComponent>> ActionIds = new();

    [DataField]
    public Dictionary<EntProtoId<TargetActionComponent>, EntityUid?> ActionEnts = new();

    [DataField]
    public Dictionary<EntProtoId<TargetActionComponent>, float> ActionChances = new();

    [DataField] public float DefaultChance = 1f;

    /// <summary>
    /// Determines when the NPC can use the skill next time using UseDelay.
    /// Values below 1 make action use faster; values above 1 make it slower.
    /// </summary>
    [DataField] public float DelayModifier = 1f;
    [DataField] public TimeSpan NextUseTime = TimeSpan.Zero;

    /// <summary>
    /// Prevents a new action from starting while a multi-step attack is still running.
    /// Boss systems extend this lock when they schedule a Paradise-style sequence.
    /// </summary>
    public TimeSpan ActionLockUntil = TimeSpan.Zero;

    /// <summary>
    /// How many successfully used actions are temporarily excluded from the preferred selection pool.
    /// This prevents a boss from repeatedly choosing only one part of its moveset while still allowing
    /// an older action as a fallback when every alternative is unavailable or rejected.
    /// </summary>
    [DataField]
    public int RecentActionMemory = 1;

    /// <summary>
    /// Delay before trying another action when every currently valid action rejects execution.
    /// This is intentionally short: a rejected state-specific attack must not consume a full attack cycle.
    /// </summary>
    [DataField]
    public TimeSpan FailedActionRetryDelay = TimeSpan.FromSeconds(0.25);

    /// <summary>
    /// Runtime history of successfully executed action prototypes, oldest first.
    /// </summary>
    public List<EntProtoId<TargetActionComponent>> RecentActions = new();
}
