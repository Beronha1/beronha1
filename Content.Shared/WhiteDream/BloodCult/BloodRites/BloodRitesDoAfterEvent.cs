// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.WhiteDream.BloodCult.BloodRites;

/// <summary>
///     Raised when a cultist finishes draining blood out of a restrained victim.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class BloodRitesDrainDoAfterEvent : SimpleDoAfterEvent;
