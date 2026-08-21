// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

namespace Content.Server.WhiteDream.BloodCult.Objectives;

[RegisterComponent, Access(typeof(KillTargetCultSystem))]
public sealed partial class KillTargetCultComponent : Component
{
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string Title = string.Empty;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Target;
}
