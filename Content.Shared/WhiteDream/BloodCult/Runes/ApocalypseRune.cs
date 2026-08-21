// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.WhiteDream.BloodCult.Runes;

[Serializable, NetSerializable]
public sealed partial class ApocalypseRuneDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public enum ApocalypseRuneVisuals
{
    Used,
    Layer
}
