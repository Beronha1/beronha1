// SPDX-FileCopyrightText: 2026 AdventureTime SS14 contributors
// SPDX-FileCopyrightText: 2026 Whiskey Station contributors
//
// SPDX-License-Identifier: MIT

using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.Classic;

/// <summary>
/// Runs the miner's ranged attack, dash and saw decisions alongside HTN melee.
/// </summary>
public sealed partial class BloodDrunkMinerCombatSystem : EntitySystem
{
    [Dependency] private BloodDrunkMinerDashSystem _dash = default!;
    [Dependency] private BloodDrunkMinerSystem _miner = default!;
    [Dependency] private GunSystem _gun = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private NPCSystem _npc = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private const float MinShootDistance = 1.5f;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<BloodDrunkMinerComponent, HTNComponent>();
        while (query.MoveNext(out var uid, out var comp, out var htn))
        {
            if (HasComp<ActorComponent>(uid) || !_npc.IsAwake(uid, htn) || _mobState.IsDead(uid))
                continue;

            if (now < comp.NextDecisionAt)
                continue;

            comp.NextDecisionAt = now + comp.DecisionInterval;

            if (!htn.Blackboard.TryGetValue<EntityUid>("Target", out var target, EntityManager)
                || TerminatingOrDeleted(target))
                continue;

            OpenFire((uid, comp), target);
        }
    }

    private void OpenFire(Entity<BloodDrunkMinerComponent> ent, EntityUid target)
    {
        var targetCoords = _transform.GetMapCoordinates(target);
        if (targetCoords.MapId == MapId.Nullspace)
            return;

        var origin = _transform.GetMapCoordinates(ent);
        if (origin.MapId != targetCoords.MapId)
            return;

        if ((targetCoords.Position - origin.Position).Length() > ent.Comp.DashRange)
            _dash.TryDash(ent, targetCoords);

        TryShoot(ent, target);
        _miner.TryTransformSaw(ent);
    }

    private bool TryShoot(Entity<BloodDrunkMinerComponent> ent, EntityUid target)
    {
        if (_timing.CurTime < ent.Comp.NextShotAt || _mobState.IsDead(target))
            return false;

        var origin = _transform.GetMapCoordinates(ent);
        var targetCoords = _transform.GetMapCoordinates(target);
        if (origin.MapId != targetCoords.MapId)
            return false;

        var distance = (targetCoords.Position - origin.Position).Length();
        if (distance > ent.Comp.DashRange || distance < MinShootDistance)
            return false;

        var gun = Spawn(ent.Comp.GunProto, Transform(ent).Coordinates);
        if (!TryComp<GunComponent>(gun, out var gunComp))
        {
            QueueDel(gun);
            return false;
        }

        var shot = _gun.AttemptShoot(ent.Owner, (gun, gunComp), Transform(target).Coordinates, (EntityUid?) target);
        QueueDel(gun);

        if (shot)
            ent.Comp.NextShotAt = _timing.CurTime + ent.Comp.RangedCooldown;

        return shot;
    }
}
