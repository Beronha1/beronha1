// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Robust.Shared.Prototypes;
using Content.Shared.Damage;

namespace Content.Lavaland.Server.Megafauna.Classic;

[RegisterComponent, Access(typeof(LegionSystem))]
public sealed partial class LegionBossComponent : Component
{
    [ViewVariables]
    public LegionState CurrentState = LegionState.Summoning;
    [ViewVariables] public TimeSpan NextStateSwitchTime;
    [ViewVariables] public TimeSpan NextSummonTime;
    [ViewVariables] public TimeSpan NextChargeTime;

    [DataField]
    public float StateSwitchInterval = 30f;

    [DataField]
    public float SummonInterval = 6f;

    [DataField]
    public float ChargeInterval = 1.5f;

    [DataField]
    public int SummonCount = 2;

    /// <summary>
    /// Maximum number of boss-spawned creatures shared by every fragment in
    /// the same encounter. Projectiles fired by those creatures are excluded.
    /// </summary>
    [DataField]
    public int MaximumActiveSummons = 8;

    /// <summary>
    /// Greater legions continuously launch skulls, so they receive a stricter
    /// encounter-wide cap than ordinary summoned skulls.
    /// </summary>
    [DataField]
    public int MaximumActiveLargeSummons = 2;

    [DataField]
    public float ReactiveSummonCooldown = 2.5f;

    [DataField]
    public EntProtoId MinionPrototype = "MobLegionSkull";

    [DataField]
    public EntProtoId LargeMinionPrototype = "MobLegionLarge";

    [DataField]
    public EntProtoId LaserMarkerPrototype = "EffectMegaFaunaMarker";

    [DataField]
    public float LaserRange = 14f;

    [DataField]
    public DamageSpecifier LaserDamage = new();

    [ViewVariables]
    public LegionRangedPattern? LastRangedPattern;

    [ViewVariables]
    public TimeSpan NextReactiveSummon;

    [DataField]
    public List<EntProtoId> SplitPrototypes = new()
    {
        "MobMegaLegionSplitLeft",
        "MobMegaLegionSplitRight",
        "MobMegaLegionSplitEye"
    };

}

public enum LegionRangedPattern : byte
{
    Laser,
    LargeSummon,
    SkullSummon,
}

[RegisterComponent, Access(typeof(LegionSystem))]
public sealed partial class LegionSplitComponent : Component
{
    [DataField("nextSplit")]
    public EntProtoId? NextSplitPrototype;

    /// <summary>
    /// Identifies every fragment produced by the same original Legion. The
    /// final reward is granted only after the last living fragment in this
    /// group dies.
    /// </summary>
    [ViewVariables]
    public Guid SplitGroup;

    /// <summary>
    /// The original Legion carcass that owns this fragment chain.
    /// </summary>
    [ViewVariables]
    public EntityUid? RootCarcass;
}

/// <summary>
/// Runtime state for the single encounter represented by the root Legion and
/// every fragment produced after its apparent death.
/// </summary>
[RegisterComponent, Access(typeof(LegionSystem))]
public sealed partial class LegionEncounterComponent : Component
{
    [ViewVariables]
    public Guid SplitGroup;

    /// <summary>
    /// Set before rewards are emitted so simultaneous final deaths cannot pay twice.
    /// </summary>
    [ViewVariables]
    public bool Completed;

    /// <summary>
    /// Summons owned by the whole encounter. Stale entities are pruned before
    /// every spawn check and all remaining summons are removed on completion.
    /// </summary>
    [ViewVariables]
    public HashSet<EntityUid> ActiveSummons = new();

    [ViewVariables]
    public HashSet<EntityUid> ActiveLargeSummons = new();
}
