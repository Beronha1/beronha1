// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Lavaland.Shared.Megafauna.Harvesting;
using Content.Shared.EntityTable;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;

// ReSharper disable EnforceForeachStatementBraces
// ReSharper disable EnforceIfStatementBraces
namespace Content.Lavaland.Server.Mobs;

public sealed partial class SpawnOnDeathSystem : EntitySystem
{
    [Dependency] private EntityTableSystem _entityTable = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SpawnLootOnDeathComponent, AttackedEvent>(OnDropAttacked);
        SubscribeLocalEvent<SpawnLootOnDeathComponent, MobStateChangedEvent>(OnDropKilled);
    }

    private void OnDropAttacked(EntityUid uid, SpawnLootOnDeathComponent comp, ref AttackedEvent args)
    {
        if (_mobState.IsDead(uid))
            return;

        // Once a non-qualifying source damages the boss, later crusher/PKA hits must
        // not restore the trophy. This makes the "special weapon only" contract real.
        comp.DoSpecialLoot &= _whitelist.IsWhitelistPassOrNull(comp.SpecialWeaponWhitelist, args.Used);
    }

    private void OnDropKilled(EntityUid uid, SpawnLootOnDeathComponent comp, ref MobStateChangedEvent args)
    {
        if (!_mobState.IsDead(uid))
            return;

        // A harvestable dead boss is the carcass. It is deleted after its final stage.
        if (comp.DeleteOnDeath && !HasComp<MegafaunaHarvestableComponent>(uid))
            QueueDel(uid);

        if (comp.DropOnDeath)
            TryDropLoot((uid, comp));
    }

    /// <summary>
    /// Resolves an entity's normal and special loot tables exactly once. This is
    /// public so multi-phase encounters can award their loot at their real end.
    /// </summary>
    public bool TryDropLoot(Entity<SpawnLootOnDeathComponent> ent)
    {
        if (ent.Comp.HasDropped || Deleted(ent))
            return false;

        ent.Comp.HasDropped = true;
        var coords = Transform(ent).Coordinates;

        var droppedSpecial = false;
        if (ent.Comp.DoSpecialLoot && ent.Comp.SpecialTable != null)
        {
            var specialLoot = _entityTable.GetSpawns(ent.Comp.SpecialTable);
            foreach (var item in specialLoot)
                Spawn(item, coords);

            droppedSpecial = true;
        }

        if (ent.Comp.Table == null)
            return true;

        var loot = _entityTable.GetSpawns(ent.Comp.Table);
        if (droppedSpecial)
        {
            if (ent.Comp.DropBoth)
                foreach (var item in loot)
                    Spawn(item, coords);
        }
        else
            foreach (var item in loot)
                Spawn(item, coords);

        return true;
    }
}
