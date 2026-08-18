// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

namespace Content.Server.WhiteDream.BloodCult.Items.ShuttleCurse;

[RegisterComponent]
public sealed partial class ShuttleCurseProviderComponent : Component
{
    [DataField]
    public int MaxUses = 3;

    [ViewVariables(VVAccess.ReadOnly)]
    public int CurrentUses = 0;
}
