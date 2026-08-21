// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

namespace Content.Server.WhiteDream.BloodCult.Runes.Revive;

[RegisterComponent]
public sealed partial class ReviveRuneChargesProviderComponent : Component
{
    [DataField]
    public int Charges = 3;
}
