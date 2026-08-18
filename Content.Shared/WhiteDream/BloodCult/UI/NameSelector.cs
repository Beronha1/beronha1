// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Robust.Shared.Serialization;

namespace Content.Shared.WhiteDream.BloodCult.UI;

[Serializable, NetSerializable]
public enum NameSelectorUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class NameSelectedMessage(string name)
    : BoundUserInterfaceMessage
{
    public string Name { get; private set; } = name;
}
