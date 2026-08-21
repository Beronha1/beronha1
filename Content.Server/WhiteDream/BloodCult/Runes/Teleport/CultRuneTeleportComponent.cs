// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Robust.Shared.Audio;

namespace Content.Server.WhiteDream.BloodCult.Runes.Teleport;

[RegisterComponent]
public sealed partial class CultRuneTeleportComponent : Component
{
    [DataField]
    public float TeleportGatherRange = 0.65f;

    [DataField]
    public string Name = "";

    [DataField]
    public SoundPathSpecifier TeleportInSound = new("/Audio/WhiteDream/BloodCult/veilin.ogg");

    [DataField]
    public SoundPathSpecifier TeleportOutSound = new("/Audio/WhiteDream/BloodCult/veilout.ogg");
}
