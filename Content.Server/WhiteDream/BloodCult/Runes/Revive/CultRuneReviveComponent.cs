// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Runes.Revive;

[RegisterComponent]
public sealed partial class CultRuneReviveComponent : Component
{
    [DataField]
    public float ReviveRange = 0.5f;

    [DataField]
    public DamageSpecifier Healing = new()
    {
        // Trauma - DamageDict is keyed by ProtoId now, and only damage types are valid keys,
        // so the old Brute/Burn groups are spelled out.
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            ["Blunt"] = -100,
            ["Slash"] = -100,
            ["Piercing"] = -100,
            ["Ballistic"] = -100,
            ["Heat"] = -100,
            ["Shock"] = -100,
            ["Cold"] = -100,
            ["Caustic"] = -100,
            ["Asphyxiation"] = -100,
            ["Bloodloss"] = -100,
            ["Poison"] = -50,
            ["Cellular"] = -50
        }
    };
}
