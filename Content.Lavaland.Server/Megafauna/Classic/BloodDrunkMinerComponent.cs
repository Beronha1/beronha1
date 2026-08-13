// SPDX-FileCopyrightText: 2026 AdventureTime SS14 contributors
// SPDX-FileCopyrightText: 2026 Whiskey Station contributors
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Server.Megafauna.Classic;

/// <summary>
/// Combat state for the ADT-derived blood-drunk miner implementation.
/// </summary>
[RegisterComponent, Access(
    typeof(BloodDrunkMinerSystem),
    typeof(BloodDrunkMinerDashSystem),
    typeof(BloodDrunkMinerCombatSystem))]
public sealed partial class BloodDrunkMinerComponent : Component
{
    [DataField]
    public bool SawOpen;

    [DataField]
    public DamageSpecifier ClosedDamage = new();

    [DataField]
    public DamageSpecifier OpenDamage = new();

    [DataField]
    public float ClosedAttackRate = 2.5f;

    [DataField]
    public float OpenAttackRate = 1.5f;

    [DataField]
    public float CleaveArc = 67.5f;

    [DataField]
    public float TransformAfterAttackChance = 0.5f;

    [DataField]
    public TimeSpan TransformCooldownMin = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan TransformCooldownMax = TimeSpan.FromSeconds(10);

    [ViewVariables]
    public TimeSpan NextTransformAt;

    [DataField]
    public SoundSpecifier TransformSound =
        new SoundPathSpecifier("/Audio/Weapons/chainsaw_rev.ogg");

    [DataField]
    public float DashRange = 4f;

    [DataField]
    public TimeSpan DashCooldown = TimeSpan.FromSeconds(1.5);

    [ViewVariables]
    public TimeSpan NextDashAt;

    [DataField]
    public float DashSpeed = 12f;

    [DataField]
    public EntProtoId? DashSmokeProto = "EffectBloodDrunkMinerDashSmoke";

    [DataField]
    public SoundSpecifier DashSound = new SoundPathSpecifier("/Audio/Weapons/punchmiss.ogg");

    [DataField]
    public EntProtoId GunProto = "WeaponBloodDrunkMinerKineticAccelerator";

    [DataField]
    public TimeSpan RangedCooldown = TimeSpan.FromSeconds(1.6);

    [ViewVariables]
    public TimeSpan NextShotAt;

    [DataField]
    public float ButcherHealFraction = 0.5f;

    [DataField]
    public float DamageInterruptCoefficient = 0.01f;

    [DataField]
    public TimeSpan DecisionInterval = TimeSpan.FromSeconds(0.5);

    [ViewVariables]
    public TimeSpan NextDecisionAt;
}
