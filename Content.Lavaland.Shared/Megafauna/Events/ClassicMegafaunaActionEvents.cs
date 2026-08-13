// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Shared.Megafauna.Events;

public sealed partial class MegaLegionAction : EntityTargetActionEvent;

public sealed partial class ColossusFractionActionEvent : EntityTargetActionEvent
{
    [DataField] public float FractionSpread = 0.3f;
    [DataField] public int FractionCount = 5;
}

public sealed partial class ColossusCrossActionEvent : EntityTargetActionEvent
{
    [DataField] public float CrossLength = 10f;
    [DataField] public float CrossDelay = 1f;
}

public sealed partial class ColossusSpiralActionEvent : EntityTargetActionEvent
{
    [DataField] public int JudgementProjectileCount = 16;
    [DataField] public float JudgementProjectileDelay = 0.08f;
    [DataField] public float DieHealthModifier = 0.33f;
    [DataField] public int DieProjectileCount = 20;
    [DataField] public float DieProjectileDelay = 0.06f;
}

public sealed partial class ColossusTripleFractionActionEvent : EntityTargetActionEvent
{
    [DataField] public float FractionSpread = 0.3f;
    [DataField] public int FractionCount = 5;
    [DataField] public float TripleFractionDelay = 0.5f;
}

public sealed partial class AshDrakeConeFireActionEvent : EntityTargetActionEvent;

public sealed partial class AshDrakeBreathingFireActionEvent : EntityTargetActionEvent
{
    [DataField] public float HealthModifier = 0.5f;
}

public sealed partial class AshDrakeLavaActionEvent : EntityTargetActionEvent
{
    [DataField] public float HealthModifier = 0.5f;
    [DataField] public EntProtoId Lava = "EffectAshDrakeFloorLavaTemp";
    [DataField] public EntProtoId LavaLess = "EffectAshDrakeFloorLavaLessTemp";
    [DataField] public EntProtoId Wall = "EffectAshDrakeFireWall";
    [DataField] public EntProtoId Marker = "EffectMegaFaunaMarker";
    [DataField] public EntProtoId SafeMarker = "EffectAshDrakeSafeMarker";
    [DataField(required: true)] public DamageSpecifier LandingDamage;
    [DataField(required: true)] public DamageSpecifier HealingSpec;
}

public sealed partial class BubblegumRageActionEvent : EntityTargetActionEvent;

public sealed partial class BubblegumBloodDiveActionEvent : EntityTargetActionEvent
{
    [DataField] public float DiveRange = 5f;
    [DataField] public float PreDiveDelay = 0.8f;
}

public sealed partial class BubblegumTripleDashActionEvent : EntityTargetActionEvent
{
    [DataField] public List<float> DashDelays;
    [DataField] public float DashDistance = 6f;
    [DataField] public float MoveSpeed = 0.1f;
    [DataField(required: true)] public DamageSpecifier DashDamage = new();
    [DataField] public bool UseSineWaveForLast = true;

    public BubblegumTripleDashActionEvent()
    {
        // Explicit Add calls keep the generated IL compatible with the Robust
        // client sandbox. .NET 10 lowers collection initializers to
        // CollectionsMarshal.SetCount, which content assemblies cannot call.
        DashDelays = new List<float>(3);
        DashDelays.Add(0.9f);
        DashDelays.Add(0.6f);
        DashDelays.Add(0.3f);
    }
}

public sealed partial class BubblegumIllusionDashActionEvent : EntityTargetActionEvent
{
    [DataField] public int IllusionCount = 3;
    [DataField] public float PlacementRadius = 4f;
    [DataField] public float PreDashDelay = 1f;
    [DataField(required: true)] public DamageSpecifier IllusionDamage;
    [DataField] public EntProtoId IllusionPrototype = "MobBubblegumIllusion";
}

public sealed partial class BubblegumPentagramDashActionEvent : EntityTargetActionEvent
{
    [DataField] public float PlacementRadius = 5f;
    [DataField] public float PreDashDelay = 1f;
    [DataField(required: true)] public DamageSpecifier IllusionDamage;
    [DataField] public EntProtoId IllusionPrototype = "MobBubblegumIllusion";
}

public sealed partial class BubblegumChaoticIllusionDashActionEvent : EntityTargetActionEvent
{
    [DataField] public int IllusionCount = 2;
    [DataField] public float PlacementRadius = 6f;
    [DataField] public float PreDashDelay = 0.8f;
    [DataField(required: true)] public DamageSpecifier IllusionDamage;
    [DataField] public EntProtoId IllusionPrototype = "MobBubblegumIllusion";
}
