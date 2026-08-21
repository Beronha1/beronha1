// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

namespace Content.Server.WhiteDream.BloodCult.BloodBoilProjectile;

[RegisterComponent]
public sealed partial class BloodBoilProjectileComponent : Component
{
    public EntityUid Target;
}
