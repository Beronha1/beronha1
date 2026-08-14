// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.EntityTable;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Lavaland.Shared.Artifacts;

/// <summary>
/// Native ECS implementation of the two-mode cleaving saw reward.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CleavingSawComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Open;

    [DataField]
    public float ClosedAttackRate = 2.5f;

    [DataField]
    public float OpenAttackRate = 0.8f;

    [DataField]
    public Angle ClosedAngle = Angle.FromDegrees(30);

    [DataField]
    public Angle OpenAngle = Angle.FromDegrees(120);

    [DataField]
    public DamageSpecifier ClosedDamage = new();

    [DataField]
    public DamageSpecifier OpenDamage = new();

    [DataField]
    public DamageSpecifier ClosedBleed = new();

    [DataField]
    public float MegafaunaDamageMultiplier = 1.5f;
}

[Serializable, NetSerializable]
public enum CleavingSawVisuals : byte
{
    Open,
}

/// <summary>
/// Marks the Wildhunter knife. Its Tool component remains the authority for harvest speed and qualities.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WildhunterKnifeComponent : Component;

/// <summary>
/// Defines the finite material return obtainable by cutting a trophy with a Wildhunter knife.
/// </summary>
[RegisterComponent]
public sealed partial class TrophyRecyclableComponent : Component
{
    [DataField(required: true)]
    public EntityTableSelector Loot = default!;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(4);

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActiveKnife;
}

[Serializable, NetSerializable]
public sealed partial class RecycleTrophyDoAfterEvent : SimpleDoAfterEvent;

[RegisterComponent]
public sealed partial class DemonicJackhammerComponent : Component
{
    [DataField]
    public DamageSpecifier MeleeHeal = new();

    [DataField]
    public float ThrowStrength = 4f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ResurrectionCrystalComponent : Component
{
    [DataField]
    public TimeSpan ReviveTime = TimeSpan.FromSeconds(8);

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActiveTarget;
}

[RegisterComponent]
public sealed partial class ResurrectionInProgressComponent : Component
{
    [DataField]
    public EntityUid Crystal;
}

[Serializable, NetSerializable]
public sealed partial class ResurrectionCrystalDoAfterEvent : SimpleDoAfterEvent;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CursedIceBootsComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled;

    [DataField]
    public EntProtoId TrailPrototype = "DemonicIceTrail";

    [DataField]
    public TimeSpan TrailInterval = TimeSpan.FromSeconds(0.3);
}

[RegisterComponent]
public sealed partial class CursedIceTrailCarrierComponent : Component
{
    [DataField]
    public EntityUid Source;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextTrail;
}

public sealed partial class ToggleCursedIceBootsActionEvent : InstantActionEvent;

[RegisterComponent]
public sealed partial class GodslayerArmorComponent : Component
{
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Time the armour needs to rebuild its wearer after they enter critical condition or die.
    /// If the wearer dies while restoration is already charging, this delay starts again from death.
    /// </summary>
    [DataField]
    public TimeSpan RevivalDelay = TimeSpan.FromSeconds(4);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextRevival;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Wearer;
}

[RegisterComponent]
public sealed partial class GodslayerCarrierComponent : Component
{
    [DataField]
    public EntityUid Armor;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool RevivalPending;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan RevivalAt;
}
