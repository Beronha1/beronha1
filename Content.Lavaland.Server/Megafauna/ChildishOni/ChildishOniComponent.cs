// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Server.Megafauna.ChildishOni;

[RegisterComponent, Access(typeof(ChildishOniSystem))]
public sealed partial class ChildishOniComponent : Component
{
    [DataField]
    public float JumpDistance = 6f;

    [DataField]
    public float JumpSpeed = 8f;

    [DataField]
    public float LandingRadius = 1f;

    [DataField]
    public DamageSpecifier LandingDamage = new();

    [DataField]
    public EntProtoId LandingRingPrototype = "ChildishOniSkullTemporary";

    [DataField]
    public EntProtoId HandFromRightPrototype = "ChildishOniHandLeft";

    [DataField]
    public EntProtoId HandFromLeftPrototype = "ChildishOniHandRight";

    [DataField]
    public EntProtoId SlashProjectile = "ChildishOniSlashProjectile";

    [ViewVariables]
    public bool IsLeaping;

    [ViewVariables]
    public int LastVisualPhase = 1;

    public Dictionary<string, List<EntityUid>> Rings = new();
}

[RegisterComponent, Access(typeof(ChildishOniSystem))]
public sealed partial class ChildishOniDirectionalMovementComponent : Component
{
    [DataField]
    public bool MoveEast;

    [DataField]
    public bool MoveWest;

    [DataField]
    public float Speed = 12f;

    [DataField]
    public float Acceleration = 6f;

    public float CurrentSpeed;
}

[RegisterComponent, Access(typeof(ChildishOniSystem))]
public sealed partial class ChildishOniSpiralingComponent : Component
{
    [DataField]
    public float SpiralSpeed = 0.2f;

    [DataField]
    public float SpiralDistance = 8f;

    [DataField]
    public float SpiralAcceleration = 1.2f;

    [DataField]
    public float SpiralMaxSpeed = 10f;

    [DataField]
    public bool DeleteOnEnd = true;

    public float Angle;
    public float Radius;
    public float CurrentSpeed;
    public Vector2 Origin;
    public bool MovementInitialized;
}

[RegisterComponent, Access(typeof(ChildishOniSystem))]
public sealed partial class ChildishOniOrbitingComponent : Component
{
    public float Radius;

    [DataField]
    public float MaxRadius = 2f;

    [DataField]
    public float GrowSpeed = 1f;

    public float Angle;
}
