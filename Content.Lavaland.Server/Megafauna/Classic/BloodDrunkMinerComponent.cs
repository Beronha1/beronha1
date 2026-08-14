// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

namespace Content.Lavaland.Server.Megafauna.Classic;

[RegisterComponent, Access(typeof(BloodDrunkMinerSystem))]
public sealed partial class BloodDrunkMinerComponent : Component
{
    [DataField]
    public Content.Shared.Damage.DamageSpecifier MeleeHeal = new();

    [DataField]
    public float MaximumDashRange = 10f;

    [DataField] public float DashDistanceThreshold = 4f;
    [DataField] public float MinimumRangedDistance = 1.25f;
    [DataField] public TimeSpan DashCooldown = TimeSpan.FromSeconds(1.5);
    [DataField] public TimeSpan TransformCooldownMin = TimeSpan.FromSeconds(5);
    [DataField] public TimeSpan TransformCooldownMax = TimeSpan.FromSeconds(10);
    [DataField] public float TransformChance = 0.5f;
    [DataField] public float ClosedAttackRate = 2.5f;
    [DataField] public float OpenAttackRate = 0.8f;
    [DataField] public Content.Shared.Damage.DamageSpecifier ClosedDamage = new();
    [DataField] public Content.Shared.Damage.DamageSpecifier OpenDamage = new();

    [ViewVariables] public bool SawOpen;
    [ViewVariables] public TimeSpan NextDash;
    [ViewVariables] public TimeSpan NextTransform;
}
