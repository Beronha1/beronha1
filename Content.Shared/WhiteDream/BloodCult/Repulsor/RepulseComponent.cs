// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

namespace Content.Shared.WhiteDream.BloodCult.Repulsor;

[RegisterComponent]
public sealed partial class RepulseComponent : Component
{
    [DataField]
    public float ForceMultiplier = 13000;

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(3);

    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(3);
}

public sealed class BeforeRepulseEvent(EntityUid target) : CancellableEntityEventArgs
{
    public EntityUid Target = target;
}
