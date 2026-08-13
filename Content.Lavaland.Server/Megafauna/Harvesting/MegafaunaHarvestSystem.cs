// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Lavaland.Server.Megafauna.Classic;
using Content.Lavaland.Shared.Megafauna.Harvesting;
using Content.Shared.EntityTable;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;

namespace Content.Lavaland.Server.Megafauna.Harvesting;

/// <summary>
/// Performs ordered, tool-gated harvesting of dead megafauna through ToolSystem DoAfters.
/// </summary>
public sealed partial class MegafaunaHarvestSystem : EntitySystem
{
    [Dependency] private EntityTableSystem _entityTable = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedToolSystem _tools = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaHarvestableComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<MegafaunaHarvestableComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<MegafaunaHarvestableComponent, MegafaunaHarvestDoAfterEvent>(OnHarvestComplete);
        SubscribeLocalEvent<MegafaunaHarvestableComponent, ExaminedEvent>(OnExamine);
    }

    private void OnInteractHand(Entity<MegafaunaHarvestableComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !_mobState.IsDead(ent) || !TryGetCurrentStage(ent.Comp, out var stage))
            return;

        if (IsHarvestLocked(ent))
        {
            _popup.PopupEntity(Loc.GetString("megafauna-harvest-legion-locked"), ent, args.User);
            args.Handled = true;
            return;
        }

        if (!_inventory.TryGetSlotEntity(args.User, "head", out var worn) || worn is not { } wornUid)
            return;

        if (!TryComp<WearableMegafaunaHarvesterComponent>(wornUid, out var harvester) ||
            harvester.InventorySlot != "head" ||
            !stage.ToolQualities.All(harvester.ToolQualities.Contains))
            return;

        var wearable = harvester!;

        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            stage.Duration / Math.Max(0.01f, wearable.SpeedModifier),
            new MegafaunaHarvestDoAfterEvent(ent.Comp.CurrentStage),
            ent,
            target: ent,
            used: wornUid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = false,
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnInteractUsing(Entity<MegafaunaHarvestableComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !_mobState.IsDead(ent) || !TryGetCurrentStage(ent.Comp, out var stage))
            return;

        if (IsHarvestLocked(ent))
        {
            _popup.PopupEntity(Loc.GetString("megafauna-harvest-legion-locked"), ent, args.User);
            args.Handled = true;
            return;
        }

        if (!_tools.HasAllQualities(args.Used, stage.ToolQualities))
        {
            if (HasComp<ToolComponent>(args.Used))
            {
                _popup.PopupEntity(
                    Loc.GetString("megafauna-harvest-wrong-tool", ("stage", Loc.GetString(stage.Name))),
                    ent,
                    args.User);
            }

            return;
        }

        args.Handled = _tools.UseTool(
            args.Used,
            args.User,
            ent,
            stage.Duration,
            stage.ToolQualities,
            new MegafaunaHarvestDoAfterEvent(ent.Comp.CurrentStage),
            out _);
    }

    private void OnHarvestComplete(Entity<MegafaunaHarvestableComponent> ent, ref MegafaunaHarvestDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !_mobState.IsDead(ent) ||
            IsHarvestLocked(ent) ||
            args.Stage != ent.Comp.CurrentStage ||
            !TryGetCurrentStage(ent.Comp, out var stage) ||
            args.Used is not { } tool ||
            !IsValidHarvestTool(args.User, tool, stage))
        {
            return;
        }

        var coordinates = Transform(ent).Coordinates;
        foreach (var prototype in _entityTable.GetSpawns(stage.Loot))
            Spawn(prototype, coordinates);

        ent.Comp.CurrentStage++;
        args.Handled = true;

        if (ent.Comp.CurrentStage >= ent.Comp.Stages.Count &&
            TryComp<WearableMegafaunaHarvesterComponent>(tool, out var wornHarvester) &&
            !wornHarvester.CompletionHeal.Empty)
        {
            _damage.TryChangeDamage(args.User, wornHarvester.CompletionHeal, true, false, origin: tool);
        }

        _popup.PopupEntity(
            Loc.GetString("megafauna-harvest-complete", ("stage", Loc.GetString(stage.Name))),
            ent,
            args.User,
            PopupType.Medium);

        if (ent.Comp.CurrentStage < ent.Comp.Stages.Count)
            return;

        if (ent.Comp.CompletionCarcass is { } carcass)
        {
            Spawn(carcass, coordinates);
            QueueDel(ent);
            return;
        }

        if (ent.Comp.DeleteOnCompletion)
            QueueDel(ent);
    }

    private bool IsValidHarvestTool(EntityUid user, EntityUid tool, MegafaunaHarvestStage stage)
    {
        if (_tools.HasAllQualities(tool, stage.ToolQualities))
            return true;

        if (!TryComp<WearableMegafaunaHarvesterComponent>(tool, out var harvester))
            return false;

        var wearable = harvester!;
        if (!stage.ToolQualities.All(wearable.ToolQualities.Contains) ||
            !_inventory.TryGetSlotEntity(user, wearable.InventorySlot, out var equipped))
        {
            return false;
        }

        if (equipped is not { } equippedUid)
            return false;

        return equippedUid == tool;
    }

    private void OnExamine(Entity<MegafaunaHarvestableComponent> ent, ref ExaminedEvent args)
    {
        if (!_mobState.IsDead(ent) || !TryGetCurrentStage(ent.Comp, out var stage))
            return;

        if (IsHarvestLocked(ent))
        {
            args.PushMarkup(Loc.GetString("megafauna-harvest-legion-locked-examine"));
            return;
        }

        var tools = new List<string>();
        foreach (var qualityId in stage.ToolQualities)
        {
            if (ProtoMan.TryIndex<ToolQualityPrototype>(qualityId, out var quality))
                tools.Add(Loc.GetString(quality.ToolName));
            else
                tools.Add(qualityId);
        }

        args.PushMarkup(Loc.GetString(
            "megafauna-harvest-examine",
            ("stage", Loc.GetString(stage.Name)),
            ("current", ent.Comp.CurrentStage + 1),
            ("total", ent.Comp.Stages.Count),
            ("tools", string.Join(" + ", tools)),
            ("seconds", stage.Duration.TotalSeconds)));
    }

    private static bool TryGetCurrentStage(MegafaunaHarvestableComponent component, out MegafaunaHarvestStage stage)
    {
        if (component.CurrentStage < 0 || component.CurrentStage >= component.Stages.Count)
        {
            stage = default!;
            return false;
        }

        stage = component.Stages[component.CurrentStage];
        return true;
    }

    private bool IsHarvestLocked(EntityUid uid)
    {
        return TryComp<LegionEncounterComponent>(uid, out var encounter) && !encounter.Completed;
    }
}
