// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Weapons;
using Content.Lavaland.Common.Weapons;
using Content.Lavaland.Common.Weapons.Marker;
using Content.Lavaland.Common.Weapons.Ranged;
using Content.Lavaland.Shared.Weapons.Upgrades.Components;
using Content.Shared.Actions;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.Examine;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Trauma.Common.Weapons.Ranged;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Lavaland.Shared.Weapons.Upgrades;

public sealed partial class GunUpgradeSystem : EntitySystem
{
    [Dependency] private ActionContainerSystem _actionContainer = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedEntityEffectsSystem _effects = default!;
    [Dependency] private SharedGunSystem _gun = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;

    private EntityQuery<GunUpgradeComponent> _upgradeQuery;

    private HashSet<Entity<GunUpgradeComponent>> _upgrades = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _upgradeQuery = GetEntityQuery<GunUpgradeComponent>();

        SubscribeLocalEvent<UpgradeableWeaponComponent, EntInsertedIntoContainerMessage>(OnUpgradeInserted);
        SubscribeLocalEvent<UpgradeableWeaponComponent, ItemSlotInsertAttemptEvent>(OnItemSlotInsertAttemptEvent);
        SubscribeLocalEvent<WeaponTrophySlotComponent, ComponentInit>(OnTrophySlotsInit);
        SubscribeLocalEvent<WeaponTrophySlotComponent, ComponentRemove>(OnTrophySlotsRemoved);
        SubscribeLocalEvent<WeaponTrophySlotComponent, ItemSlotInsertAttemptEvent>(OnTrophySlotInsertAttempt);
        SubscribeLocalEvent<UpgradeableWeaponComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<UpgradeableWeaponComponent, GunRefreshModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, RechargeBasicEntityAmmoGetCooldownModifiersEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GunShotEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GunGetProjectileSpreadEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, ProjectileShotEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GetRelayMeleeWeaponEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GetMeleeDamageEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, MeleeHitEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GetLightAttackRangeEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, GetMeleeAttackRateEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, ApplyMarkerBonusEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, MarkerAttackAttemptEvent>(RelayEvent);
        SubscribeLocalEvent<UpgradeableWeaponComponent, AfterMarkerAttackedEvent>(RelayEvent);

        SubscribeLocalEvent<UpgradeableWeaponComponent, GetItemActionsEvent>(RelayGetActionEvent);

        SubscribeLocalEvent<GunUpgradeComponent, ExaminedEvent>(OnUpgradeExamine);
        SubscribeLocalEvent<CrusherTrophyComponent, ExaminedEvent>(OnTrophyExamine);

        InitializeUpgrades();
    }

    private void RelayEvent<T>(Entity<UpgradeableWeaponComponent> ent, ref T args) where T : notnull
    {
        foreach (var upgrade in GetCurrentUpgrades(ent))
        {
            RaiseLocalEvent(upgrade, ref args);
        }
    }

    // Because of how action container work we need that workaround for GetItemActionsEvent
    private void RelayGetActionEvent(Entity<UpgradeableWeaponComponent> ent, ref GetItemActionsEvent args)
    {
        foreach (var upgrade in GetCurrentUpgrades(ent))
        {
            var ev = new GetItemActionsEvent(_actionContainer, args.User, upgrade.Owner, isEquipping: args.IsEquipping);
            RaiseLocalEvent(upgrade.Owner, ev);

            if (ev.Actions.Count == 0)
                continue;

            if (!args.IsEquipping)
            {
                _actions.RemoveProvidedActions(args.User, upgrade.Owner);
                _actions.SaveActions(args.User);
                continue;
            }

            _actions.GrantActions(args.User, ev.Actions, upgrade.Owner);
            _actions.LoadActions(args.User);
        }
    }

    private void OnExamine(Entity<UpgradeableWeaponComponent> ent, ref ExaminedEvent args)
    {
        var usedCapacity = 0;
        var usedTrophyCapacity = 0;
        using (args.PushGroup(nameof(UpgradeableWeaponComponent)))
        {
            foreach (var upgrade in GetCurrentUpgrades(ent))
            {
                if (TryComp<CrusherTrophyComponent>(upgrade, out var trophy))
                {
                    usedTrophyCapacity += trophy.CapacityCost;
                    continue;
                }

                if (upgrade.Comp.InsertedTextType != null)
                    args.PushMarkup(Loc.GetString(upgrade.Comp.InsertedTextType.Value, ("name", Loc.GetString(upgrade.Comp.Name))));
                if (upgrade.Comp.CapacityCost != null)
                    usedCapacity += upgrade.Comp.CapacityCost.Value;
            }

            if (ent.Comp.MaxUpgradeCapacity != null)
                args.PushMarkup(Loc.GetString("upgradeable-gun-total-remaining-capacity", ("value", ent.Comp.MaxUpgradeCapacity.Value - usedCapacity)));
        }


        if (TryComp<WeaponTrophySlotComponent>(ent, out var trophySlots))
        {
            using (args.PushGroup(nameof(WeaponTrophySlotComponent)))
            {
                foreach (var trophy in GetCurrentTrophies(ent, trophySlots))
                {
                    if (trophy.Comp1.InsertedTextType != null)
                        args.PushMarkup(Loc.GetString(trophy.Comp1.InsertedTextType.Value,
                            ("name", Loc.GetString(trophy.Comp1.Name))));
                }

                args.PushMarkup(Loc.GetString("weapon-trophy-total-remaining-capacity",
                    ("value", trophySlots.MaxTrophyCapacity - usedTrophyCapacity)));
            }
        }
    }

    private void OnUpgradeExamine(Entity<GunUpgradeComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.ExamineTextType != null) // TODO add a list of all weapon types that this gun upgrade can be inserted to
            args.PushMarkup(Loc.GetString(ent.Comp.ExamineTextType.Value, ("name", Loc.GetString(ent.Comp.Name))));

        if (ent.Comp.CapacityCost != null)
            args.PushMarkup(Loc.GetString("gun-upgrade-capacity-cost", ("value", ent.Comp.CapacityCost.Value)));
    }

    private void OnTrophyExamine(Entity<CrusherTrophyComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("weapon-trophy-capacity-cost", ("value", ent.Comp.CapacityCost)));
    }

    private void OnUpgradeInserted(Entity<UpgradeableWeaponComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        // Update some characteristics here.
        if (TryComp(ent.Owner, out GunComponent? gun))
            _gun.RefreshModifiers((ent.Owner, gun));
    }

    private void OnItemSlotInsertAttemptEvent(Entity<UpgradeableWeaponComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (!_upgradeQuery.TryComp(args.Item, out var upgradeComp)
            || !TryComp<ItemSlotsComponent>(ent, out var itemSlots))
            return;

        var currentUpgrades = GetCurrentUpgrades(ent, itemSlots);
        var totalCapacityCost = currentUpgrades.Sum(upgrade => upgrade.Comp.CapacityCost);
        if (totalCapacityCost + upgradeComp.CapacityCost > ent.Comp.MaxUpgradeCapacity)
        {
            args.Cancelled = true;
            return;
        }

        foreach (var curUpgrade in currentUpgrades)
        {
            if (upgradeComp.UniqueGroup == null
                || curUpgrade.Comp.UniqueGroup == null
                || upgradeComp.UniqueGroup != curUpgrade.Comp.UniqueGroup)
                continue;

            args.Cancelled = true;
            return;
        }
    }

    private void OnTrophySlotInsertAttempt(Entity<WeaponTrophySlotComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (!TryComp<CrusherTrophyComponent>(args.Item, out var insertedTrophy) ||
            !TryComp<ItemSlotsComponent>(ent, out var itemSlots))
        {
            return;
        }

        string? attemptedSlotId = null;
        foreach (var (slotId, slot) in itemSlots.Slots)
        {
            if (!ReferenceEquals(args.Slot, slot))
                continue;

            attemptedSlotId = slotId;
            break;
        }

        if (attemptedSlotId == null)
            return;

        // Trophy inheritance still supplies legacy blade/handle tags, but a trophy may
        // only occupy one of the dedicated trophy containers.
        if (!attemptedSlotId.StartsWith(ent.Comp.SlotPrefix, StringComparison.Ordinal))
        {
            args.Cancelled = true;
            return;
        }

        var usedCapacity = 0;
        foreach (var trophy in GetCurrentTrophies(ent, ent.Comp, itemSlots))
        {
            usedCapacity += trophy.Comp2.CapacityCost;
            if (trophy.Comp2.TrophyId == insertedTrophy.TrophyId)
            {
                args.Cancelled = true;
                return;
            }
        }

        if (usedCapacity + insertedTrophy.CapacityCost > ent.Comp.MaxTrophyCapacity)
            args.Cancelled = true;
    }

    private void OnTrophySlotsInit(Entity<WeaponTrophySlotComponent> ent, ref ComponentInit args)
    {
        ent.Comp.RuntimeSlots.Clear();
        for (var i = 1; i <= ent.Comp.SlotCount; i++)
        {
            var slot = new ItemSlot(ent.Comp.Slot);
            ent.Comp.RuntimeSlots.Add(slot);
            _itemSlots.AddItemSlot(ent, $"{ent.Comp.SlotPrefix}{i}", slot);
        }
    }

    private void OnTrophySlotsRemoved(Entity<WeaponTrophySlotComponent> ent, ref ComponentRemove args)
    {
        foreach (var slot in ent.Comp.RuntimeSlots)
            _itemSlots.RemoveItemSlot(ent, slot);

        ent.Comp.RuntimeSlots.Clear();
    }

    /// <summary>
    /// Returns a reused hashset of upgrades in a weapon.
    /// Do not store this hashset between calls.
    /// </summary>
    public IReadOnlyList<Entity<GunUpgradeComponent>> GetCurrentUpgrades(Entity<UpgradeableWeaponComponent> ent, ItemSlotsComponent? itemSlots = null)
    {
        _upgrades.Clear();
        if (!Resolve(ent, ref itemSlots))
            return Array.Empty<Entity<GunUpgradeComponent>>();

        foreach (var itemSlot in itemSlots.Slots.Values)
        {
            if (itemSlot.Item is { } item && _upgradeQuery.TryComp(item, out var upgrade))
                _upgrades.Add((item, upgrade));
        }

        return _upgrades
            .OrderBy(upgrade => upgrade.Comp.PipelinePriority)
            .ThenBy(upgrade => upgrade.Comp.UniqueGroup ?? upgrade.Comp.Name.Id, StringComparer.Ordinal)
            .ToList();
    }

    private List<Entity<GunUpgradeComponent, CrusherTrophyComponent>> GetCurrentTrophies(
        EntityUid uid,
        WeaponTrophySlotComponent trophySlots,
        ItemSlotsComponent? itemSlots = null)
    {
        var trophies = new List<Entity<GunUpgradeComponent, CrusherTrophyComponent>>();
        if (!Resolve(uid, ref itemSlots))
            return trophies;

        foreach (var (slotId, slot) in itemSlots.Slots)
        {
            if (!slotId.StartsWith(trophySlots.SlotPrefix, StringComparison.Ordinal) ||
                slot.Item is not { } item ||
                !TryComp<GunUpgradeComponent>(item, out var upgrade) ||
                !TryComp<CrusherTrophyComponent>(item, out var trophy))
            {
                continue;
            }

            trophies.Add((item, upgrade, trophy));
        }

        trophies.Sort((left, right) => string.CompareOrdinal(left.Comp2.TrophyId, right.Comp2.TrophyId));
        return trophies;
    }
}
