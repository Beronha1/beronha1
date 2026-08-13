// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Shared.BossRewards;

/// <summary>
/// Radiation-fed capacitor recovered from the Spider of Mercury.
/// Charge is represented by the owning entity's <c>UseDelayComponent</c>, so
/// examine, activation and radiation-assisted recharge share one authoritative timer.
/// </summary>
[RegisterComponent]
public sealed partial class MercuryEtherDrinkerComponent : Component
{
    [DataField]
    public TimeSpan BaseRechargeTime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Seconds removed from the recharge timer per rad/s, per elapsed second.
    /// </summary>
    [DataField]
    public float RadiationRechargeMultiplier = 8f;

    /// <summary>
    /// Percent charge consumed to create one strike.
    /// </summary>
    [DataField]
    public float ChargePerStrike = 5f;

    [DataField]
    public int MaxStrikes = 15;

    [DataField]
    public float StrikeOffset = 4f;

    [DataField]
    public EntProtoId LightningPrototype = "LightningCrackleNeutral";

    [DataField]
    public SoundSpecifier DischargeSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/glitch.ogg");
}

/// <summary>
/// Saves a user's position and damage state, then restores both after a short,
/// server-authoritative interval. Inventory is deliberately never snapshotted,
/// preventing the device from duplicating items or reagents.
/// </summary>
[RegisterComponent]
public sealed partial class MercuryParadoxCancellerComponent : Component
{
    [DataField]
    public TimeSpan RewindTime = TimeSpan.FromSeconds(5);

    [DataField]
    public SoundSpecifier StartSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/communicating.ogg");

    [DataField]
    public SoundSpecifier RewindSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/glitch.ogg");

    [DataField]
    public EntProtoId MarkerPrototype = "MercuryParadoxAnchorEffect";

    public EntityUid? User;
    public EntityUid? Marker;
    public EntityCoordinates? SavedCoordinates;
    public DamageSpecifier? SavedDamage;
    public TimeSpan? RewindAt;
}
