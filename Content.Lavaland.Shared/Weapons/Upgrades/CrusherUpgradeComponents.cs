// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Shared.Damage;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;

namespace Content.Lavaland.Shared.Weapons.Upgrades;

/// <summary>
/// Adds dedicated boss trophy slots and capacity to a portable kinetic weapon.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(GunUpgradeSystem))]
public sealed partial class WeaponTrophySlotComponent : Component
{
    [DataField]
    public string SlotPrefix = "trophy_slot_";

    [DataField]
    public int SlotCount = 8;

    [DataField]
    public int MaxTrophyCapacity = 100;

    [DataField]
    public ItemSlot Slot = new();

    [ViewVariables]
    public List<ItemSlot> RuntimeSlots = new();
}

/// <summary>
/// Marker for boss trophies. Trophy effects are still implemented as normal GunUpgrade relays.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CrusherTrophyComponent : Component
{
    /// <summary>
    /// Stable identifier used to reject duplicate trophies independently of prototype inheritance.
    /// </summary>
    [DataField(required: true)]
    public string TrophyId = string.Empty;

    [DataField]
    public int CapacityCost = 25;
}

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

    [DataField]
    public int SkullHitsRequired = 3;

    [DataField]
    public TimeSpan SkullCooldown = TimeSpan.FromSeconds(8);

    [ViewVariables]
    public int SkullHitProgress;

    [ViewVariables]
    public TimeSpan NextSkull;

    [ViewVariables]
    public EntityUid? ActiveSkull;

    [DataField]
    public EntProtoId SkullPrototype = "MobKineticExplosiveLegionSkull";
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

    [DataField]
    public DamageSpecifier RangedHeal = new();
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

    [DataField]
    public float ProjectileKnockback = 6f;

    [DataField]
    public TimeSpan ProjectileExplosionCooldown = TimeSpan.FromSeconds(3);

    [ViewVariables]
    public TimeSpan NextProjectileExplosion;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherIceBlockTalismanUpgradeComponent : Component
{
    [DataField]
    public TimeSpan FreezeDuration = TimeSpan.FromSeconds(4);

    [DataField]
    public EntProtoId EffectPrototype = "EffectCrusherIceBlock";

    [DataField]
    public int RangedHitsRequired = 3;

    [DataField]
    public TimeSpan RangedCooldown = TimeSpan.FromSeconds(8);

    [ViewVariables]
    public Dictionary<EntityUid, int> RangedHits = new();

    [ViewVariables]
    public Dictionary<EntityUid, TimeSpan> RangedCooldowns = new();
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherDemonClawsUpgradeComponent : Component
{
    [DataField]
    public float DamageMultiplier = 0.15f;

    [DataField]
    public DamageSpecifier MeleeHeal = new();

    [DataField]
    public int ProjectileCount = 3;

    /// <summary>
    /// Hard cap for multiplicative pellet platforms such as the kinetic shotgun
    /// and shockwave accelerator.
    /// </summary>
    [DataField]
    public int MaxProjectileCount = 24;

    [DataField]
    public Angle ProjectileSpread = Angle.FromDegrees(45);

    [DataField]
    public DamageSpecifier RangedHeal = new();
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

    [DataField]
    public float RangedRechargeMultiplier = 0.4f;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherMercuryAlloyUpgradeComponent : Component
{
    [DataField]
    public float RicochetRange = 5f;
}

[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class CrusherOniHornUpgradeComponent : Component
{
    [DataField]
    public float WaveRadius = 2.5f;

    [DataField]
    public float ThrowStrength = 7f;
}

/// <summary>
/// Aggregated projectile behaviour assembled from all installed boss trophies.
/// One component enforces a deterministic, non-recursive impact pipeline.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(CrusherUpgradeEffectsSystem))]
public sealed partial class KineticTrophyProjectileComponent : Component
{
    [DataField]
    public EntityUid? AshDrakeTrophy;

    [DataField]
    public EntityUid? ColossusTrophy;

    [DataField]
    public EntityUid? DemonClawsTrophy;

    [DataField]
    public EntityUid? LegionTrophy;

    [DataField]
    public EntityUid? BloodDrunkTrophy;

    [DataField]
    public EntityUid? IceTalismanTrophy;

    [DataField]
    public EntityUid? MercuryTrophy;

    [DataField]
    public EntityUid? OniTrophy;

    [DataField]
    public bool Ricocheted;
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
public sealed partial class GunUpgradeAreaDamageComponent : Component
{
    [DataField]
    public float DamageRadius = 1.5f;

    [DataField]
    public float DamageMultiplier = 0.2f;
}
