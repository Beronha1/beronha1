// SPDX-FileCopyrightText: 2026 Whiskey Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.IntegrationTests.Tests.Atmos;
using Content.Server._ES.TileFires;
using Content.Shared.Atmos.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reagent;
using Content.Shared._ES.TileFires;
using Content.Shared.FixedPoint;
using Content.Trauma.Common.Atmos;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests._ES.TileFires;

[TestFixture]
[TestOf(typeof(ESTileFireSystem))]
public sealed class ESTileFireTest : AtmosTest
{
    protected override ResPath? TestMapPath => new("Maps/Test/Atmospherics/load_atmos_test_room.yml");

    [Test]
    public async Task EventStageFireIgnitesGrowsAndSpreads()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var grid = MapData.Grid.Owner;

        EntityUid source = default;
        await server.WaitPost(() =>
        {
            var fire = server.System<ESTileFireSystem>();
            // Use the center of a sealed, oxygenated 5x5 room. The previous
            // 3x3 fixture had to remove its perimeter walls, allowing the new
            // atmosphere solver to vent the room before the spreader tick.
            var coordinates = new EntityCoordinates(grid, 2.5f, 2.5f);
            Assert.That(fire.TryDoTileFire(coordinates, stage: 2), Is.True);

            var query = entMan.EntityQueryEnumerator<ESTileFireComponent, FlammableComponent>();
            Assert.That(query.MoveNext(out source, out var tileFire, out var flammable), Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(flammable.FireStacks, Is.EqualTo(7f));
                Assert.That(entMan.HasComponent<OnFireComponent>(source), Is.True);
            });

            // Keep the production stage-2 starting point while accelerating the
            // passive growth enough for a deterministic integration test.
            tileFire.BaseSpreadChance = 1f;
            flammable.FirestackFade = 3f;
        });

        // Flammable updates once per second. Two updates take stage 2 above the
        // maximum randomized spread threshold and the next spreader pass must
        // create at least one neighboring tile fire.
        await server.WaitRunTicks(100);

        await server.WaitAssertion(() =>
        {
            Assert.That(entMan.Deleted(source), Is.False);
            Assert.That(entMan.HasComponent<OnFireComponent>(source), Is.True);

            var count = 0;
            var query = entMan.EntityQueryEnumerator<ESTileFireComponent, TransformComponent>();
            while (query.MoveNext(out _, out _, out var transform))
            {
                if (transform.ParentUid == grid)
                    count++;
            }

            Assert.That(count, Is.GreaterThan(1));
        });
    }

    [Test]
    public async Task OneUnitOfWaterExtinguishesStageOneFire()
    {
        var server = Pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var grid = MapData.Grid.Owner;
        EntityUid fireUid = default;
        await server.WaitPost(() =>
        {
            var fire = server.System<ESTileFireSystem>();
            Assert.That(fire.TryDoTileFire(new EntityCoordinates(grid, 2.5f, 2.5f)), Is.True);

            var query = entMan.EntityQueryEnumerator<ESTileFireComponent>();
            Assert.That(query.MoveNext(out fireUid, out _), Is.True);
            Assert.That(entMan.HasComponent<OnFireComponent>(fireUid), Is.True);

            var reactive = entMan.System<ReactiveSystem>();
            reactive.ReactionEntity(
                fireUid,
                ReactionMethod.Touch,
                new ReagentQuantity("Water", FixedPoint2.New(1)));
        });

        await Pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            Assert.That(entMan.Deleted(fireUid), Is.True,
                "A direct extinguisher spray should remove a stage-one tile fire.");
        });
    }
}
