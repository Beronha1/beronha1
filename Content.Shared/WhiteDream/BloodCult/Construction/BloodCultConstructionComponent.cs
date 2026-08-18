// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Content.Trauma.Common.RadialSelector;
using Robust.Shared.GameStates;

namespace Content.Shared.WhiteDream.BloodCult.Construction;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultConstructionComponent : Component
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = new();
}
