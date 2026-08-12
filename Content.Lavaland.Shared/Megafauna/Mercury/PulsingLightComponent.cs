// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Lavaland.Shared.Megafauna.Mercury;

/// <summary>
/// Gives an entity the original Mercury pulsing-light presentation.
/// Ported from Goobstation PR #6542.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PulsingLightComponent : Component
{
    [DataField, AutoNetworkedField]
    public float GlowIntensity = 5;

    public float CurrentGlow;

    [DataField, AutoNetworkedField]
    public float IncreaseBy = 0.1f;

    public bool ReduceGlow;

    [DataField, AutoNetworkedField]
    public bool ShouldPlaySound;

    public bool SoundPlayed;

    [DataField]
    public SoundSpecifier BootUpSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/ominous_pulse.ogg");

    public TimeSpan NextUpdate;

    [DataField]
    public TimeSpan Interval;

    [DataField, AutoNetworkedField]
    public Color LightColor = Color.Cyan;
}
