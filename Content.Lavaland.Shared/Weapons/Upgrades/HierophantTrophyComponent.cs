// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Lavaland.Shared.Weapons.Upgrades;
using Robust.Shared.Prototypes;

namespace Content.Lavaland.Shared.Weapons.Upgrades;

[RegisterComponent, Access(typeof(HierophantTrophySystem))]
public sealed partial class HierophantTrophyComponent : Component
{
    [DataField]
    public EntProtoId WallPrototype = "WallHierophantTrophy";

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(12);

    [ViewVariables]
    public TimeSpan NextActivation;
}
