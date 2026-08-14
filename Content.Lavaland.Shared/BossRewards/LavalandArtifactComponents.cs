// All modifications and original work in ss14-wega under the Corvax-Wega tag
// and _Wega directories are licensed under GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Polymorph;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Lavaland.Shared.Artifacts;

[RegisterComponent]
public sealed partial class LavaStaffComponent : Component
{
    [DataField]
    public EntProtoId LavaEntity = "FloorLavaEntity";

    [DataField]
    public ProtoId<ContentTileDefinition> BasaltTile = "FloorBasaltLavaland";

    [DataField]
    public SoundSpecifier? UseSound;

    [DataField]
    public float MaxRange = 8f;

    [DataField]
    public TimeSpan TerraformTime = TimeSpan.FromSeconds(1.5);

    [DataField]
    public EntProtoId TargetPrototype = "LavaStaffTerraformTarget";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActiveTarget;
}

[RegisterComponent]
public sealed partial class DragonBloodComponent : Component
{
    [DataField]
    public ProtoId<PolymorphPrototype> Skeleton = "WizardForcedSkeleton";

    [DataField]
    public EntProtoId LowerDrakeAction = "BecomeToDrakeAction";

    /// <summary>
    /// Permanent fire-breath ability used for the fourth canonical dragon-blood outcome.
    /// </summary>
    [DataField]
    public EntProtoId FireBreathAction = "ActionFireBreath";

    [DataField]
    public TimeSpan UseTime = TimeSpan.FromSeconds(5);

    [DataField]
    public SoundSpecifier UseSound = new SoundPathSpecifier("/Audio/Items/drink.ogg");
}

[RegisterComponent]
public sealed partial class SoulStorageComponent : Component
{
    [DataField]
    public float BonusDamagePerSoul = 4f;

    [DataField]
    public float MaxBonusDamage = 76f;

    [DataField]
    public int MaxOrbitingGhosts = 12;

    [DataField]
    public SoundSpecifier CallSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/ghost2.ogg");

    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> StolenSouls = [];
}

[RegisterComponent]
public sealed partial class DivineVocalCordsImplantComponent : Component
{
    [DataField]
    public float Radius = 5f;

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(30);

    [ViewVariables]
    public TimeSpan NextUse;
}

[RegisterComponent]
public sealed partial class DivineVoiceCarrierComponent : Component
{
    [DataField]
    public EntityUid Implant;
}

[RegisterComponent]
public sealed partial class StabilizedLegionCoreImplantComponent : Component
{
    [DataField]
    public int MaxActivations = 1;

    [ViewVariables(VVAccess.ReadOnly)]
    public int ActivationsRemaining;
}

[Serializable, NetSerializable]
public sealed partial class LavaStaffTerraformDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class DragonBloodDoAfterEvent : SimpleDoAfterEvent;

public sealed partial class BecomeToDrakeActionEvent : InstantActionEvent
{
    [DataField]
    public ProtoId<PolymorphPrototype> LowerDrake = "LowerAshDrakePolymorph";

    [DataField]
    public EntProtoId ReturnBack = "DrakeReturnBackAction";
}

public sealed partial class DrakeReturnBackActionEvent : InstantActionEvent;

public sealed partial class ColossusRoarActionEvent : InstantActionEvent;

/// <summary>
/// Reimplementation of the Ash Drake's Sacred Flame reward. The spellbook grants this action without
/// requiring the reader to retain an item-side component.
/// </summary>
public sealed partial class SacredFlameActionEvent : InstantActionEvent
{
    [DataField]
    public float Radius = 4f;

    [DataField]
    public float Severity = 0.45f;
}
