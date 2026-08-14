// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Lavaland.Shared.Research;

/// <summary>
/// Data awarded when this entity is consumed by a research destructor.
/// </summary>
[RegisterComponent]
public sealed partial class ResearchArtifactComponent : Component
{
    [DataField]
    public int Points;

    [DataField]
    public List<ProtoId<TechnologyPrototype>> Technologies = [];
}

[RegisterComponent]
public sealed partial class ResearchDestructorComponent : Component
{
    [DataField]
    public TimeSpan AnalyzeTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The operator must present the same irreplaceable artifact twice within
    /// this window before destructive analysis starts.
    /// </summary>
    [DataField]
    public TimeSpan ConfirmationWindow = TimeSpan.FromSeconds(8);

    [ViewVariables(VVAccess.ReadOnly)]
    public bool Busy;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? PendingArtifact;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan PendingUntil;
}

[Serializable, NetSerializable]
public sealed partial class ResearchDestructorDoAfterEvent : SimpleDoAfterEvent;
