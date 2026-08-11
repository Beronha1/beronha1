// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Lavaland.Shared.Trigger;
using Content.Shared.Whitelist;

namespace Content.Lavaland.Shared.Trigger;

[RegisterComponent, Access(typeof(DamageOnTriggerBlacklistSystem))]
public sealed partial class DamageOnTriggerBlacklistComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Blacklist = new();
}
