// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Lavaland.Shared.Megafauna.Utility;
using Content.Server.Administration.Logs;
using Content.Server.NPC;
using Content.Server.NPC.Systems;
using Content.Shared.Database;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Robust.Shared.Map;

namespace Content.Lavaland.Server.Megafauna.Utility;

/// <summary>
/// Grows legion servitors from processed skull cultures and binds them to the user who opened the culture.
/// </summary>
public sealed partial class LegionServitorCultureSystem : EntitySystem
{
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private NpcFactionSystem _npcFaction = default!;
    [Dependency] private NPCSystem _npc = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LegionServitorCultureComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<CulturedLegionServitorComponent, EntityTerminatingEvent>(OnServitorTerminating);
    }

    private void OnUse(Entity<LegionServitorCultureComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        var controller = EnsureComp<LegionServitorControllerComponent>(args.User);
        controller.Servitors.RemoveWhere(uid => !Exists(uid) || _mobState.IsDead(uid));

        if (controller.Servitors.Count >= ent.Comp.MaxActiveServitors)
        {
            _popup.PopupClient(
                Loc.GetString("legion-servitor-culture-limit", ("maximum", ent.Comp.MaxActiveServitors)),
                ent,
                args.User);
            return;
        }

        var servitor = Spawn(ent.Comp.ServitorPrototype, Transform(args.User).Coordinates);
        var servant = EnsureComp<CulturedLegionServitorComponent>(servitor);
        servant.Creator = args.User;
        controller.Servitors.Add(servitor);

        var exception = EnsureComp<FactionExceptionComponent>(servitor);
        _npcFaction.IgnoreEntity((servitor, exception), args.User);
        _npc.SetBlackboard(
            servitor,
            NPCBlackboard.FollowTarget,
            new EntityCoordinates(args.User, Vector2.Zero));

        _popup.PopupEntity(
            Loc.GetString("legion-servitor-culture-grown", ("servitor", servitor)),
            args.User,
            args.User);
        _adminLog.Add(
            LogType.Action,
            $"{ToPrettyString(args.User):player} grew {ToPrettyString(servitor):entity} from {ToPrettyString(ent):item}");

        QueueDel(ent);
    }

    private void OnServitorTerminating(Entity<CulturedLegionServitorComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.Creator is not { } creator ||
            !TryComp<LegionServitorControllerComponent>(creator, out var controller))
        {
            return;
        }

        controller.Servitors.Remove(ent);
    }
}
