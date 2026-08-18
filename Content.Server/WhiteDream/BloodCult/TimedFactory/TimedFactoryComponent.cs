// SPDX-License-Identifier: AGPL-3.0-or-later
// Blood Cult: ported from WWhiteDreamProject/wwdpublic. See Content.Shared/WhiteDream/BloodCult/ATTRIBUTION.md

using Content.Trauma.Common.RadialSelector;
using Robust.Shared.Audio;

namespace Content.Server.WhiteDream.BloodCult.TimedFactory;

[RegisterComponent]
public sealed partial class TimedFactoryComponent : Component
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = new();

    [DataField]
    public float Cooldown = 240;

    /// <summary>
    ///     WhiteDream - played when the structure spits something out.
    /// </summary>
    [DataField]
    public SoundSpecifier? ProductionSound;

    [ViewVariables(VVAccess.ReadOnly)]
    public float CooldownRemaining = 0;
}
