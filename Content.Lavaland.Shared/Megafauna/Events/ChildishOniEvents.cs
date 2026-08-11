// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Lavaland.Shared.Megafauna.Events;

[Serializable, NetSerializable]
public enum ChildishOniVisuals : byte
{
    Phase,
}

[Serializable, NetSerializable]
public enum ChildishOniPhaseVisual : byte
{
    Phase0,
    Phase1,
    Phase2,
    Phase3,
}

public sealed partial class ChildishOniRampageEvent : WorldTargetActionEvent;
public sealed partial class ChildishOniFlurryEvent : InstantActionEvent;

public sealed partial class ChildishOniRingEvent : InstantActionEvent
{
    [DataField]
    public float Radius = 2f;

    [DataField]
    public string RingId = string.Empty;
}

public sealed partial class ChildishOniHandEvent : WorldTargetActionEvent
{
    [DataField]
    public float Offset = 6f;

    [DataField]
    public int Count = 1;

    [DataField]
    public float Interval = 0.8f;
}
