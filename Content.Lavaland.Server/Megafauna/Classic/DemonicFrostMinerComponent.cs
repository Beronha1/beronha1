// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Megafauna.Events;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Server.Megafauna.Classic;

[RegisterComponent, Access(typeof(DemonicFrostMinerSystem))]
public sealed partial class DemonicFrostMinerComponent : Component
{
    [DataField]
    public float EnrageDamageFraction = 0.75f;

    [DataField]
    public TimeSpan EnrageInvulnerability = TimeSpan.FromSeconds(2);

    [DataField]
    public EntProtoId FrostOrbProjectile = "ProjectileDemonicFrostOrb";

    [DataField]
    public EntProtoId SnowballProjectile = "ProjectileDemonicSnowball";

    [DataField]
    public EntProtoId IceBlastProjectile = "ProjectileDemonicIceBlast";

    [DataField]
    public DamageSpecifier MeleeHeal = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public bool Enraged;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool Transforming;
}
