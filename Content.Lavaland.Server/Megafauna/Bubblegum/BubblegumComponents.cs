// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Shared.Actions.Components;
using Content.Shared.Damage;
using Content.Shared.Maps;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Server.Megafauna.Bubblegum;

[RegisterComponent, Access(typeof(BubblegumSystem))]
public sealed partial class BubblegumBossComponent : Component
{
    [ViewVariables]
    public BubblegumPhase CurrentPhase = BubblegumPhase.Normal;

    [ViewVariables]
    public TimeSpan RageEndTime;

    [ViewVariables]
    public bool IsRaging;

    [DataField]
    public float RageDelayModifier = 0.5f;

    [DataField]
    public float RageDurationMin = 3.5f;

    [DataField]
    public float RageDurationMax = 7f;

    [ViewVariables]
    public TimeSpan NextBloodDiveTime;

    [ViewVariables]
    public TimeSpan NextBloodDiveAttemptTime;

    [ViewVariables]
    public TimeSpan NextPassiveHandTime;

    /// <summary>
    /// Paradise-inspired second encounter. The first body is a transition only; rewards belong to the second life.
    /// </summary>
    [DataField]
    public bool EnableSecondLife = true;

    [DataField]
    public bool SecondLife;

    [DataField]
    public EntProtoId SecondLifePrototype = "LavalandBossBubblegumSecondLife";

    [DataField]
    public float SecondLifeCaptureRadius = 18f;

    [DataField]
    public int ArenaRadius = 12;

    [DataField]
    public ProtoId<ContentTileDefinition> ArenaFloor = "FloorBasaltLavaland";

    [DataField]
    public EntProtoId ArenaWall = "WallNecropolisIndestructible";

    [ViewVariables(VVAccess.ReadOnly)]
    public bool TransitionStarted;

    [DataField]
    public float BloodDiveCooldown = 25f;

    [DataField("rewards")]
    public List<EntProtoId> RewardsProto = new();

    [DataField]
    public List<EntProtoId<TargetActionComponent>> Phase1Actions = new();

    [DataField]
    public List<EntProtoId<TargetActionComponent>> Phase2Actions = new();

    [DataField]
    public Dictionary<EntProtoId<TargetActionComponent>, float> Phase1Chances = new();

    [DataField]
    public Dictionary<EntProtoId<TargetActionComponent>, float> Phase2Chances = new();

    [DataField]
    public EntProtoId BloodEffect = "PuddleBlood";

    [DataField]
    public EntProtoId DashMarker = "EffectMegaFaunaMarker";

    [ViewVariables]
    public EntityUid? LastDashMarker;

    [ViewVariables]
    public string LastDashStatus = "idle";

    [DataField]
    public EntProtoId DashTrail = "EffectBubblegumDashTrail";

    [DataField]
    public EntProtoId LeftHandEffect = "EffectBubblegumHandLeft";

    [DataField]
    public EntProtoId RightHandEffect = "EffectBubblegumHandRight";

    [DataField(required: true)]
    public DamageSpecifier BloodHandDamage = new();

    [DataField(required: true)]
    public DamageSpecifier EnragedBloodHandDamage = new();

    [DataField]
    public SoundSpecifier DashSound = new SoundCollectionSpecifier("FootstepThud");

    /// <summary>
    /// Hard cap for blood decals created by this boss. Dash trails otherwise create an ever-growing
    /// number of puddles which makes the passive hand scan progressively more expensive.
    /// </summary>
    [DataField]
    public int MaximumBloodPools = 48;

    [ViewVariables]
    public List<EntityUid> ActiveBloodPools = new();

    /// <summary>
    /// HTN blackboard key for the target entity
    /// </summary>
    public string TargetKey = "Target";
}

[RegisterComponent]
public sealed partial class BubblegumIllusionComponent : Component
{
    [ViewVariables]
    public EntityUid? Master;

    [ViewVariables]
    public EntityUid? Target;

    [ViewVariables]
    public EntityCoordinates? TargetPosition;

    [ViewVariables]
    public int CurrentStep;

    [ViewVariables]
    public int TotalSteps;

    [DataField]
    public DamageSpecifier? Damage;
}
