// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Megafauna.Utility;
using Content.Shared.Inventory.Events;
using Content.Shared.Storage.Components;
using Robust.Shared.Containers;

namespace Content.Lavaland.Server.Megafauna.Utility;

/// <summary>
/// Applies the interdepartmental utility of harvest rewards that modify another entity.
/// </summary>
public sealed partial class MegafaunaUtilitySystem : EntitySystem
{
    [Dependency] private MegafaunaHeatProtectionSystem _heatProtection = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DensityCoreComponent, EntGotInsertedIntoContainerMessage>(OnDensityCoreInserted);
        SubscribeLocalEvent<DensityCoreComponent, EntGotRemovedFromContainerMessage>(OnDensityCoreRemoved);
        SubscribeLocalEvent<DragonArmorComponent, GotEquippedEvent>(OnDragonArmorEquipped);
        SubscribeLocalEvent<DragonArmorComponent, GotUnequippedEvent>(OnDragonArmorUnequipped);
    }

    private void OnDensityCoreInserted(Entity<DensityCoreComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (ent.Comp.AppliedTo != null ||
            !TryComp<DensityCoreReceiverComponent>(args.Container.Owner, out var receiver) ||
            args.Container.ID != receiver.SlotId ||
            !TryComp<EntityStorageComponent>(args.Container.Owner, out var storage))
        {
            return;
        }

        storage.Capacity += receiver.CapacityBonus;
        ent.Comp.AppliedTo = args.Container.Owner;
        Dirty(args.Container.Owner, storage);
        Dirty(ent);
    }

    private void OnDensityCoreRemoved(Entity<DensityCoreComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (ent.Comp.AppliedTo != args.Container.Owner ||
            !TryComp<DensityCoreReceiverComponent>(args.Container.Owner, out var receiver) ||
            !TryComp<EntityStorageComponent>(args.Container.Owner, out var storage))
        {
            return;
        }

        storage.Capacity = Math.Max(0, storage.Capacity - receiver.CapacityBonus);
        ent.Comp.AppliedTo = null;
        Dirty(args.Container.Owner, storage);
        Dirty(ent);
    }

    private void OnDragonArmorEquipped(Entity<DragonArmorComponent> ent, ref GotEquippedEvent args)
    {
        if (ent.Comp.Wearer != null)
            return;

        ent.Comp.Wearer = args.EquipTarget;
        ent.Comp.ProtectionGeneration = _heatProtection.AddOrRefreshSource(args.EquipTarget, ent);
        Dirty(ent);
    }

    private void OnDragonArmorUnequipped(Entity<DragonArmorComponent> ent, ref GotUnequippedEvent args)
    {
        if (ent.Comp.Wearer != args.EquipTarget)
            return;

        _heatProtection.RemoveSource(args.EquipTarget, ent, ent.Comp.ProtectionGeneration);

        ent.Comp.Wearer = null;
        ent.Comp.ProtectionGeneration = 0;
        Dirty(ent);
    }
}
