// All modifications and original work in ss14-wega under the Corvax-Wega tag
// and _Wega directories are licensed under GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Shared.Damage;

namespace Content.Lavaland.Shared.Artifacts;

[RegisterComponent]
public sealed partial class LegionCoreComponent : Component
{
    [DataField]
    public bool Active = true;

    [DataField]
    public bool Stabilized;

    [DataField]
    public DamageSpecifier HealAmount = new();

    [DataField]
    public TimeSpan ActiveDuration = TimeSpan.FromSeconds(150);

    [ViewVariables]
    public TimeSpan ActiveEndTime;
}
