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
}

[RegisterComponent]
public sealed partial class DragonBloodComponent : Component
{
    [DataField]
    public ProtoId<PolymorphPrototype> Skeleton = "WizardForcedSkeleton";

    [DataField]
    public EntProtoId LowerDrakeAction = "BecomeToDrakeAction";

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
