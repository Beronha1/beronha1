// All modifications and original work in ss14-wega under the Corvax-Wega tag
// and _Wega directories are licensed under GNU GPL v3.
// https://github.com/corvax-team/ss14-wega/blob/master/LICENSE.TXT

using Content.Lavaland.Shared.Artifacts;
using Content.Server.Popups;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Lavaland.Server.Artifacts;

public sealed partial class LegionCoreSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LegionCoreComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<LegionCoreComponent, AfterInteractEvent>(OnInteract);
    }

    private void OnUse(Entity<LegionCoreComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryHeal(args.User, args.User, ent);
    }

    private void OnInteract(Entity<LegionCoreComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        args.Handled = TryHeal(target, args.User, ent);
    }

    private bool TryHeal(EntityUid target, EntityUid user, Entity<LegionCoreComponent> core)
    {
        if (!HasComp<DamageableComponent>(target))
            return false;

        _damage.TryChangeDamage(target, core.Comp.HealAmount, true, false);
        QueueDel(core);
        return true;
    }
}
