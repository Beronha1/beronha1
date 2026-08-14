// SPDX-FileCopyrightText: 2026 AdventureTime SS14 contributors
// SPDX-FileCopyrightText: 2026 Whiskey Station contributors
//
// SPDX-License-Identifier: MIT

using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Lavaland.Server.Megafauna.Classic;

/// <summary>
/// Performs a collision-aware physical dash instead of changing coordinates.
/// </summary>
public sealed partial class BloodDrunkMinerDashSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private ThrowingSystem _throwing = default!;

    public bool TryDash(Entity<BloodDrunkMinerComponent> ent, MapCoordinates target)
    {
        var now = _timing.CurTime;
        if (now < ent.Comp.NextDashAt || _mobState.IsDead(ent))
            return false;

        var origin = _transform.GetMapCoordinates(ent);
        if (origin.MapId == MapId.Nullspace || origin.MapId != target.MapId)
            return false;

        var diff = target.Position - origin.Position;
        var distance = diff.Length();
        if (distance < 0.01f)
            return false;

        var direction = diff / distance;
        var dashDistance = Math.Min(ent.Comp.DashRange, distance - 1f);
        if (dashDistance < 1f)
            return false;

        var ray = new CollisionRay(origin.Position, direction, (int) CollisionGroup.Impassable);
        foreach (var result in _physics.IntersectRay(origin.MapId, ray, dashDistance, ent.Owner, false))
        {
            dashDistance = Math.Min(dashDistance, result.Distance - 0.5f);
        }

        if (dashDistance < 1f)
            return false;

        if (ent.Comp.DashSmokeProto is { } smoke)
            Spawn(smoke, origin);

        _audio.PlayPvs(ent.Comp.DashSound, ent);
        _throwing.TryThrow(
            ent.Owner,
            direction * dashDistance,
            ent.Comp.DashSpeed,
            animated: false,
            playSound: false,
            doSpin: false,
            compensateFriction: true);

        ent.Comp.NextDashAt = now + ent.Comp.DashCooldown;
        return true;
    }
}
