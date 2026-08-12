// Все модификации и наработки в ss14-wega под тегом Corvax-Wega и директориях _Wega лицензированы под GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Shared.Damage;
using Content.Shared.Trigger.Systems;
using Content.Shared.Whitelist;

namespace Content.Lavaland.Shared.Trigger;

public sealed partial class DamageOnTriggerBlacklistSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DamageOnTriggerBlacklistComponent, BeforeDamageOnTriggerEvent>(OnBeforeDamage);
    }

    private void OnBeforeDamage(Entity<DamageOnTriggerBlacklistComponent> ent, ref BeforeDamageOnTriggerEvent args)
    {
        if (_whitelist.IsWhitelistPass(ent.Comp.Blacklist, args.Tripper))
            args.Damage = new DamageSpecifier();
    }
}
