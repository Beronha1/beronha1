// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Lavaland.Shared.Artifacts;

/// <summary>
/// Configures one reusable Colossus crystal effect. Every variant remains valid research-destructor input.
/// </summary>
[RegisterComponent]
public sealed partial class AnomalousCrystalComponent : Component
{
    [DataField]
    public AnomalousCrystalMode Mode = AnomalousCrystalMode.Ward;

    [DataField]
    public float Radius = 4f;

    [DataField]
    public TimeSpan EffectDuration = TimeSpan.FromSeconds(8);

    [DataField]
    public DamageSpecifier Healing = new();
}

public enum AnomalousCrystalMode : byte
{
    Ward,
    HealingPulse,
    Repulsion,
    Stasis,
}

/// <summary>
/// Short-lived damage reduction produced by a ward crystal.
/// </summary>
[RegisterComponent]
public sealed partial class AnomalousCrystalWardComponent : Component
{
    [DataField]
    public float DamageCoefficient = 0.45f;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan EndTime;
}
