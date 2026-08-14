// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Shared.Weapons.Upgrades;

/// <summary>
/// Paradise's railgun PASSMOB behaviour: damage living targets and continue,
/// but stop normally on structures and terrain.
/// </summary>
[RegisterComponent]
public sealed partial class KineticMobPenetrationProjectileComponent : Component;

[RegisterComponent, NetworkedComponent]
public sealed partial class KineticMiningAreaProjectileComponent : Component
{
    [DataField]
    public float Radius = 1.5f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class KineticHumanPassthroughProjectileComponent : Component;

[RegisterComponent, NetworkedComponent]
public sealed partial class KineticDronePassthroughProjectileComponent : Component;

[RegisterComponent, NetworkedComponent, Access(typeof(KineticModkitSystem))]
public sealed partial class KineticRapidRepeaterUpgradeComponent : Component
{
    [DataField]
    public float MissCooldownMultiplier = 6f;

    [DataField]
    public float HitCooldownFraction = 0.25f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class KineticRapidRepeaterProjectileComponent : Component
{
    [DataField]
    public EntityUid SourceUpgrade;
}

[RegisterComponent, NetworkedComponent, Access(typeof(KineticModkitSystem))]
public sealed partial class KineticResonatorUpgradeComponent : Component
{
    [DataField]
    public EntProtoId FieldPrototype = "EffectKineticResonanceField";

    [DataField]
    public float TriggerRadius = 1f;

    [DataField]
    public float DamageRadius = 1.5f;

    [DataField]
    public DamageSpecifier BurstDamage = new();

    [DataField]
    public EntProtoId SlowdownEffect = "KineticResonanceSlowdownStatusEffect";

    [DataField]
    public TimeSpan SlowdownDuration = TimeSpan.FromSeconds(10);

    [DataField]
    public float SlowdownModifier = 0.75f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class KineticResonatorProjectileComponent : Component
{
    [DataField]
    public EntityUid SourceUpgrade;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class KineticResonanceFieldComponent : Component
{
    [DataField]
    public EntityUid SourceUpgrade;

    [DataField]
    public EntityUid Shooter;
}

[RegisterComponent, Access(typeof(KineticModkitSystem))]
public sealed partial class KineticDeathSyphonUpgradeComponent : Component
{
    [DataField]
    public float NormalBounty = 1.25f;

    [DataField]
    public float MegafaunaBounty = 5f;

    [DataField]
    public float MaximumBounty = 25f;

    [ViewVariables]
    public Dictionary<EntProtoId, float> Bounties = new();
}

[RegisterComponent]
public sealed partial class KineticDeathSyphonProjectileComponent : Component
{
    [DataField]
    public EntityUid SourceUpgrade;
}

[RegisterComponent]
public sealed partial class KineticDeathSyphonMarkComponent : Component
{
    [ViewVariables]
    public HashSet<EntityUid> Sources = new();
}
