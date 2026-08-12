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
using Content.Shared.Visuals;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Artifacts;

public sealed partial class LegionCoreSystem : EntitySystem
{
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LegionCoreComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<LegionCoreComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<LegionCoreComponent, AfterInteractEvent>(OnInteract);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<LegionCoreComponent>();
        while (query.MoveNext(out var uid, out var core))
        {
            if (!core.Active || core.Stabilized || core.ActiveEndTime > _timing.CurTime)
                continue;

            core.Active = false;
            _appearance.SetData(uid, VisualLayers.Enabled, false);
        }
    }

    private void OnMapInit(Entity<LegionCoreComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.ActiveEndTime = _timing.CurTime + ent.Comp.ActiveDuration;
        _appearance.SetData(ent, VisualLayers.Enabled, true);
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
        if (!core.Comp.Stabilized && core.Comp.ActiveEndTime <= _timing.CurTime)
        {
            core.Comp.Active = false;
            _appearance.SetData(core, VisualLayers.Enabled, false);
        }

        if (!core.Comp.Active)
        {
            _popup.PopupEntity(
                Loc.GetString("legion-core-inert"),
                core,
                user,
                PopupType.SmallCaution);
            return false;
        }

        if (!HasComp<DamageableComponent>(target))
            return false;

        _damage.TryChangeDamage(target, core.Comp.HealAmount, true, false);
        QueueDel(core);
        return true;
    }
}
