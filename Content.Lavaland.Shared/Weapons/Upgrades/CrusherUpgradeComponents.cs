// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Shared.Damage;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;

namespace Content.Lavaland.Shared.Weapons.Upgrades;

/// <summary>
/// Declares the dedicated trophy slot on a kinetic crusher.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WeaponTrophySlotComponent : Component
{
    [DataField]
    public string SlotId = "trophy_slot";
}

/// <summary>
/// Marker for boss trophies. Trophy effects are still implemented as normal GunUpgrade relays.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CrusherTrophyComponent : Component;

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherLegionSkullUpgradeComponent : Component
{
    [DataField]
    public float FireRateCoefficient = 1.3f;

    [DataField]
    public TimeSpan RaiseCooldown = TimeSpan.FromSeconds(60);

    [DataField]
    public EntProtoId RaiseEffect = "LightningCrackleNeutral";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActiveAlly;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextRaise;
}

/// <summary>
/// Prevents fauna raised by one Legion trophy from being recycled through
/// additional crushers. The master is retained for administration/debugging.
/// </summary>
[RegisterComponent]
public sealed partial class LegionTrophyRaisedAllyComponent : Component
{
    [DataField]
    public EntityUid? Master;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherGoliathTentacleUpgradeComponent : Component
{
    [DataField]
    public float MaxCoefficient = 1f;

    [DataField]
    public MobState TargetState = MobState.Critical;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherAncientGoliathTentacleUpgradeComponent : Component
{
    [DataField]
    public float Coefficient = 0.5f;

    [DataField]
    public float HealthThreshold = 0.9f;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherWatcherWingUpgradeComponent : Component
{
    [DataField]
    public float CooldownIncrease = 1f;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherMagmaWingUpgradeComponent : Component
{
    [ViewVariables]
    public bool Active;

    [DataField(required: true)]
    public DamageSpecifier Damage;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherPoisonFangUpgradeComponent : Component
{
    [DataField]
    public float DamageModifier = 0.1f;

    [DataField]
    public float Duration = 2f;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherFrostGlandUpgradeComponent : Component
{
    [DataField]
    public float DamageModifier = 0.9f;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherEyeBloodDrunkMinerUpgradeComponent : Component
{
    [DataField]
    public float ImmunityDuration = 1f;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherAshDrakeSpikeUpgradeComponent : Component
{
    [DataField]
    public float DamageRadius = 3f;

    [DataField]
    public float DamageMultiplier = 0.4f;

    [DataField]
    public float HeatImmunityDuration = 4f;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherIceBlockTalismanUpgradeComponent : Component
{
    [DataField]
    public TimeSpan FreezeDuration = TimeSpan.FromSeconds(4);

    [DataField]
    public EntProtoId EffectPrototype = "EffectCrusherIceBlock";
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherDemonClawsUpgradeComponent : Component
{
    [DataField]
    public float DamageMultiplier = 0.15f;

    [DataField]
    public DamageSpecifier MeleeHeal = new();
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherBlasterTubesUpgradeComponent : Component
{
    [ViewVariables]
    public bool Active;

    [DataField(required: true)]
    public DamageSpecifier Damage;

    [DataField]
    public float ProjectileSpeedCoefficient = 1.25f;

    [DataField]
    public float ShockwaveRadius = 3f;

    [DataField]
    public float ShockwaveDamageMultiplier = 0.5f;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class IncreasedDamageComponent : Component
{
    [DataField]
    public float DamageModifier = 0.1f;

    [ViewVariables]
    public TimeSpan EndTime;
}

[RegisterComponent]
public sealed partial class ProjectileTimerResetUpgradeComponent : Component
{
    [DataField]
    public float CooldownIncrease = 1f;
}

[RegisterComponent]
public sealed partial class ProjectileAreaDamageComponent : Component
{
    [DataField]
    public float DamageRadius = 3f;

    [DataField]
    public float DamageMultiplier = 0.5f;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class GunUpgradeAreaDamageComponent : Component;
