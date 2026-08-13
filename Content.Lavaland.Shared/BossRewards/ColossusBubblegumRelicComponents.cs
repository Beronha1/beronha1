// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Lavaland.Shared.Artifacts;

[RegisterComponent]
public sealed partial class MayhemBottleComponent : Component
{
    [DataField]
    public float Radius = 7f;

    [DataField]
    public TimeSpan FrenzyDuration = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan ConfirmWindow = TimeSpan.FromSeconds(5);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan ArmedUntil;
}

[RegisterComponent]
public sealed partial class MayhemFrenzyComponent : Component
{
    [DataField]
    public float DamageMultiplier = 1.35f;

    [DataField]
    public float MovementMultiplier = 1.15f;

    [DataField]
    public TimeSpan ViolenceGrace = TimeSpan.FromSeconds(4);

    [DataField]
    public DamageSpecifier AgonyDamage = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan EndTime;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan LastViolence;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextAgony;
}

[RegisterComponent]
public sealed partial class CainAbelComponent : Component
{
    [DataField]
    public int MaxCombo = 6;

    [DataField]
    public float DamageMultiplierPerCombo = 1.15f;

    [DataField]
    public TimeSpan ComboTimeout = TimeSpan.FromSeconds(5);

    [DataField]
    public EntProtoId WispProjectile = "ProjectileCainAbelWisp";

    [DataField]
    public float ProjectileSpeed = 18f;

    [ViewVariables(VVAccess.ReadOnly)]
    public int Combo;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan ComboExpires;
}

[RegisterComponent]
public sealed partial class SoulScytheComponent : Component
{
    [DataField]
    public float MaxBlood = 100f;

    [DataField]
    public float StartingBlood = 25f;

    [DataField]
    public float BloodPerHit = 10f;

    [DataField]
    public float BloodRegenPerSecond = 1f;

    [DataField]
    public float EmpoweredHitCost = 5f;

    [DataField]
    public DamageSpecifier EmpoweredHitDamage = new();

    [DataField]
    public float WaveCost = 15f;

    [DataField]
    public EntProtoId WaveProjectile = "ProjectileSoulScytheWave";

    [DataField]
    public float ProjectileSpeed = 16f;

    [ViewVariables(VVAccess.ReadOnly)]
    public float Blood;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan LastUpdate;
}

/// <summary>
/// Applies Bubblegum's auditory curse while the H.E.C.K. suit is worn.
/// The carrier component records ownership so pre-existing hallucinations are never removed by the suit.
/// </summary>
[RegisterComponent]
public sealed partial class HeckSuitComponent : Component
{
    [DataField]
    public SoundSpecifier CurseSounds = new SoundCollectionSpecifier("bloodCrawl");

    [DataField]
    public float MinTimeBetweenIncidents = 30f;

    [DataField]
    public float MaxTimeBetweenIncidents = 60f;

    [DataField]
    public float MaxSoundDistance = 8f;
}

[RegisterComponent]
public sealed partial class HeckCurseCarrierComponent : Component
{
    [DataField]
    public EntityUid Source;

    [DataField]
    public bool AddedParacusia;
}

/// <summary>
/// Lets a worn H.E.C.K. helmet consume an ordinary biological corpse for proportional healing.
/// Megafauna corpses remain reserved for the harvesting pipeline.
/// </summary>
[RegisterComponent]
public sealed partial class HeckHelmetComponent : Component
{
    [DataField]
    public TimeSpan ConsumeDuration = TimeSpan.FromSeconds(1.5);

    [DataField]
    public float HealFraction = 0.1f;
}

[RegisterComponent]
public sealed partial class HeckCorpseReservationComponent : Component
{
    [DataField]
    public EntityUid User;

    [DataField]
    public EntityUid Helmet;
}

[Serializable, NetSerializable]
public sealed partial class HeckConsumeCorpseDoAfterEvent : SimpleDoAfterEvent;

/// <summary>
/// A bounded, auditable SS14 adaptation of Paradise's Blood Contract reward.
/// </summary>
[RegisterComponent]
public sealed partial class BloodContractComponent : Component
{
    [DataField]
    public TimeSpan MarkDuration = TimeSpan.FromMinutes(2);

    [DataField]
    public float IncomingDamageMultiplier = 1.2f;

    [DataField]
    public TimeSpan PulseInterval = TimeSpan.FromSeconds(10);

    [DataField]
    public DamageSpecifier PulseDamage = new();

    [DataField]
    public EntProtoId RewardPrototype = "ChemistryBottleDemonicBlood";
}

[RegisterComponent]
public sealed partial class BloodContractMarkComponent : Component
{
    [DataField]
    public EntityUid Source;

    [DataField]
    public float IncomingDamageMultiplier = 1.2f;

    [DataField]
    public DamageSpecifier PulseDamage = new();

    [DataField]
    public TimeSpan PulseInterval = TimeSpan.FromSeconds(10);

    [DataField]
    public EntProtoId RewardPrototype = "ChemistryBottleDemonicBlood";

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan ExpiresAt;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextPulse;
}

public sealed partial class CainAbelWispActionEvent : WorldTargetActionEvent;

public sealed partial class SoulScytheWaveActionEvent : WorldTargetActionEvent;

public sealed partial class BloodContractActionEvent : EntityTargetActionEvent;
