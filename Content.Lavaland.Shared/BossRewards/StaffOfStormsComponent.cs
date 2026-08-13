// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Lavaland.Shared.BossRewards;

/// <summary>
/// A rechargeable Legion reward that calls down delayed lightning at a ranged target.
/// Active weather makes the strike cover a wider area and deal additional damage.
/// </summary>
[RegisterComponent]
public sealed partial class StaffOfStormsComponent : Component
{
    [DataField]
    public int MaxCharges = 3;

    [ViewVariables(VVAccess.ReadOnly)]
    public int Charges;

    [DataField]
    public TimeSpan RechargeTime = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan StrikeDelay = TimeSpan.FromSeconds(1.5);

    [DataField]
    public TimeSpan DispelTime = TimeSpan.FromSeconds(3);

    [DataField]
    public float MaxRange = 12f;

    [DataField]
    public float StrikeRadius = 0.65f;

    [DataField]
    public float WeatherStrikeRadius = 1.5f;

    [DataField]
    public float WeatherDamageMultiplier = 2f;

    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public EntProtoId TelegraphPrototype = "StormStaffTelegraph";

    [DataField]
    public EntProtoId StrikePrototype = "StormStaffLightning";

    [DataField]
    public SoundSpecifier RechargeSound = new SoundPathSpecifier("/Audio/Effects/Lightning/lightningshock.ogg");

    [ViewVariables(VVAccess.ReadOnly)]
    public List<TimeSpan> RechargeAt = [];

    [ViewVariables(VVAccess.ReadOnly)]
    public List<StormStaffPendingStrike> PendingStrikes = [];
}

public readonly record struct StormStaffPendingStrike(
    MapCoordinates Coordinates,
    TimeSpan StrikeAt,
    EntityUid User,
    bool WeatherBoosted);

[Serializable, NetSerializable]
public sealed partial class StormStaffDispelDoAfterEvent : SimpleDoAfterEvent;
