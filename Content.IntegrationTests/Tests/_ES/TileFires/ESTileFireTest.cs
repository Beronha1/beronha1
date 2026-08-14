// SPDX-FileCopyrightText: 2026 Whiskey Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.IntegrationTests.Fixtures;
using Content.Server.Atmos.Components;
using Content.Server._ES.TileFires;
using Content.Shared.Atmos.Components;
using Content.Shared._ES.TileFires;
using Content.Trauma.Common.Atmos;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests._ES.TileFires;

[TestFixture]
[TestOf(typeof(ESTileFireSystem))]
public sealed class ESTileFireTest : GameTest
{
    [Test]
    public async Task EventStageFireIgnitesGrowsAndSpreads()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var mapLoader = entMan.System<MapLoaderSystem>();
        var mapSystem = entMan.System<SharedMapSystem>();
        var testMapName = new ResPath("Maps/Test/Breathing/3by3-20oxy-80nit.yml");

        EntityUid? grid = null;
        await server.WaitPost(() =>
        {
            mapSystem.CreateMap(out var mapId);
            Assert.That(mapLoader.TryLoadGrid(mapId, testMapName, out var gridEntity), Is.True);
            grid = gridEntity!.Value.Owner;
        });

        Assert.That(grid, Is.Not.Null, $"Test blueprint {testMapName} not found.");

        // This breathing fixture encloses its oxygenated 3x3 floor with eight
        // reinforced walls. Remove only those blockers so the test exercises
        // spreading rather than the generic edge-spreader wall rules.
        await server.WaitPost(() =>
        {
            var blockers = new List<EntityUid>();
            var query = entMan.EntityQueryEnumerator<AirtightComponent, TransformComponent>();
            while (query.MoveNext(out var uid, out _, out var transform))
            {
                if (transform.ParentUid == grid.Value)
                    blockers.Add(uid);
            }

            foreach (var blocker in blockers)
                entMan.DeleteEntity(blocker);
        });

        EntityUid source = default;
        await server.WaitPost(() =>
        {
            var fire = server.System<ESTileFireSystem>();
            var coordinates = new EntityCoordinates(grid.Value, 0.5f, 0.5f);
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
                if (transform.ParentUid == grid.Value)
                    count++;
            }

            Assert.That(count, Is.GreaterThan(1));
        });
    }
}
