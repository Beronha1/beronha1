// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Robust.Shared.Serialization;

namespace Content.Shared.WhiteDream.BloodCult.Construction;

[Serializable, NetSerializable]
public sealed class BloodCultConstructionSelectedMessage(string selectedItem) : BoundUserInterfaceMessage
{
    public string SelectedItem = selectedItem;
}

[Serializable, NetSerializable]
public enum BloodCultConstructionUiKey : byte
{
    Key
}
