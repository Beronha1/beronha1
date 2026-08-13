// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Lavaland.Shared.Megafauna.Utility;

/// <summary>
/// Gives extracted boss organs an explicit fresh/stabilized/deteriorated lifecycle.
/// Refrigerated containers pause the timer; chemical stabilization ends it permanently.
/// </summary>
[RegisterComponent]
public sealed partial class PerishableBossOrganComponent : Component
{
    [DataField]
    public TimeSpan FreshDuration = TimeSpan.FromMinutes(4);

    [DataField]
    public PerishableBossOrganState State = PerishableBossOrganState.Fresh;

    /// <summary>
    /// Optional container on this entity whose contents are destroyed when the carrier organ deteriorates.
    /// This is used by disposable organic implanters so an expired organ cannot still be implanted.
    /// </summary>
    [DataField]
    public string? DestroyContentsOf;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan RemainingFreshness;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? DecayAt;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? PreservedBy;
}

[Serializable, NetSerializable]
public enum PerishableBossOrganState : byte
{
    Fresh,
    Stabilized,
    Deteriorated,
}

/// <summary>
/// Chemical entity effect used by stabilizing serum on compatible megafauna organs.
/// </summary>
public sealed partial class StabilizeMegafaunaOrgan : EntityEffectBase<StabilizeMegafaunaOrgan>
{
    public override string? EntityEffectGuidebookText(
        IPrototypeManager prototype,
        IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-stabilize-megafauna-organ");
    }
}
