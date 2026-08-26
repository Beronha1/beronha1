// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.Lavaland.Server.Megafauna.Bubblegum;
using Content.Lavaland.Server.Megafauna.Classic;
using Content.Lavaland.Server.Megafauna.ChildishOni;
using Content.Lavaland.Server.Megafauna.Director;
using Content.Lavaland.Server.Megafauna.Mercury;
using Content.Lavaland.Server.Mobs;
using Content.Lavaland.Server.NPC;
using Content.Lavaland.Server.Weapons;
using Content.Lavaland.Shared.Aggression;
using Content.Lavaland.Shared.Artifacts;
using Content.Lavaland.Shared.Audio;
using Content.Lavaland.Shared.CCVar;
using Content.Shared.Chasm;
using Content.Lavaland.Shared.Chasm.Teleport;
using Content.Shared.Clothing.Components;
using Content.Lavaland.Shared.EntityShapes;
using Content.Lavaland.Shared.EntityShapes.Shapes;
using Content.Lavaland.Shared.Megafauna;
using Content.Lavaland.Shared.Megafauna.Components;
using Content.Lavaland.Shared.Megafauna.Conditions;
using Content.Lavaland.Shared.Megafauna.Events;
using Content.Lavaland.Shared.Megafauna.Mercury;
using Content.Lavaland.Shared.Megafauna.Selectors;
using Content.Lavaland.Shared.Megafauna.Systems;
using Content.Lavaland.Shared.MobPhases;
using Content.Lavaland.Shared.Procedural;
using Content.Lavaland.Shared.Procedural.Prototypes;
using Content.Shared.Actions.Components;
using Content.Shared.Actions;
using Content.Shared.Construction.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityTable;
using Content.Shared.FixedPoint;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Pinpointer;
using Content.Shared.Weapons.Melee;
using Content.Shared.Whitelist;
using Content.Trauma.Shared.Weather;
using Robust.Client.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Physics.Components;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests._Lavaland.Megafauna;

[TestFixture]
[TestOf(typeof(MegafaunaAiComponent))]
[TestOf(typeof(MegafaunaSystem))]
public sealed partial class MegafaunaTest : GameTest
{
    public const string TestBoss = "MobHierophant";
    public const string TestMusic = "Hierophant";
    public const string BubblegumArena = "BubblegumArena";
    public const string BubblegumMarker = "SpawnBubblegumLavaland";
    public const string BubblegumBoss = "LavalandBossBubblegum";
    public const string AshDrakeArena = "DragonLair";
    public const string ColossusArena = "ColossusArena";
    public const string MegaLegionArena = "MegaLegionArena";
    public const string BloodDrunkMinerArena = "BloodDrunkMinerArena";
    public const string ChildishOniArena = "ChildishOniArena";
    public const string MercuryFissure = "MercuryFissure";
    private static readonly ProtoId<DamageTypePrototype> BluntDamage = "Blunt";
    private static readonly ProtoId<MegafaunaSelectorPrototype> ChildishOniClawSelector = "ChildishOniClawSlash";
    private static readonly ProtoId<MegafaunaSelectorPrototype> ChildishOniRampageSelector = "ChildishOniRampage";

    [Test]
    public async Task BloodDrunkMinerUsesPhysicalDashAndLavalandBossComponents()
    {
        var pair = Pair;
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();

        EntityUid miner = default;
        await server.WaitPost(() =>
            miner = entMan.SpawnAtPosition(new EntProtoId("MobBloodDrunkMiner"), testMap.GridCoords));

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(entMan.HasComponent<AshStormImmuneComponent>(miner), Is.True);
                Assert.That(entMan.HasComponent<AggressiveComponent>(miner), Is.True);
                Assert.That(entMan.HasComponent<BossMusicComponent>(miner), Is.True);
                Assert.That(entMan.HasComponent<MegafaunaDirectorComponent>(miner), Is.True);
                Assert.That(entMan.HasComponent<SpawnLootOnDeathComponent>(miner), Is.True);
                Assert.That(entMan.HasComponent<LoadoutComponent>(miner), Is.False);
            });

            var component = entMan.GetComponent<BloodDrunkMinerComponent>(miner);
            var transforms = entSysMan.GetEntitySystem<SharedTransformSystem>();
            var before = transforms.GetMapCoordinates(miner);
            var target = new MapCoordinates(before.Position + new Vector2(3f, 0f), before.MapId);
            var dash = entSysMan.GetEntitySystem<BloodDrunkMinerDashSystem>();

            Assert.That(dash.TryDash((miner, component), target), Is.True);

            var after = transforms.GetMapCoordinates(miner);
            var physics = entMan.GetComponent<PhysicsComponent>(miner);
            Assert.Multiple(() =>
            {
                // A SetCoordinates teleport would move immediately. The ADT dash
                // starts with an impulse and lets collision/physics move the boss.
                Assert.That((after.Position - before.Position).Length(), Is.LessThan(0.001f));
                Assert.That(physics.LinearVelocity.Length(), Is.GreaterThan(0f));
            });
        });
    }

    [Test]
    public async Task LaunchAndShutdownMegafauna()
    {
        var pair = Pair;
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();

        EntityUid bossEntity = default;
        MegafaunaAiComponent megafaunaAi = null;
        MegafaunaSystem megafaunaSystem = null;

        await server.WaitPost(() =>
        {
            bossEntity = entMan.SpawnAtPosition(TestBoss, testMap.GridCoords);
            megafaunaAi = entMan.GetComponent<MegafaunaAiComponent>(bossEntity);
            megafaunaSystem = entSysMan.GetEntitySystem<MegafaunaSystem>();
        });

        await server.WaitRunTicks(5);

        // Check that boss is clear
        Assert.That(megafaunaAi.Active, Is.False);
        Assert.That(megafaunaAi.Schedule, Is.Empty);

        await server.WaitAssertion(() =>
        {
            Assert.DoesNotThrow(() =>
            {
                megafaunaSystem.StartupMegafauna((bossEntity, megafaunaAi));
                megafaunaSystem.StartupMegafauna((bossEntity, megafaunaAi));
            });
        });

        await server.WaitRunTicks(1);

        // Should start up now
        Assert.That(megafaunaAi.Active, Is.True);
        Assert.That(megafaunaAi.Schedule, Has.Count.EqualTo(1));

        await server.WaitAssertion(() =>
        {
            Assert.DoesNotThrow(() =>
            {
                megafaunaSystem.ShutdownMegafauna((bossEntity, megafaunaAi));
            });
        });

        await server.WaitRunTicks(1);

        // Should be clear again
        Assert.That(megafaunaAi.Active, Is.False);
        Assert.That(megafaunaAi.Schedule, Is.Empty);

        await server.WaitAssertion(() =>
        {
            megafaunaSystem.StartupMegafauna((bossEntity, megafaunaAi));
            megafaunaSystem.KillMegafauna((bossEntity, megafaunaAi));
        });

        Assert.That(megafaunaAi.Active, Is.False);
        Assert.That(megafaunaAi.Schedule, Is.Empty);
    }

    [Test]
    public async Task PhaseScalingPreservesBaseline()
    {
        var pair = Pair;
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();

        EntityUid bossEntity = default;
        MobPhasesComponent phases = null!;
        MobPhasesSystem phaseSystem = null!;

        await server.WaitPost(() =>
        {
            bossEntity = entMan.SpawnAtPosition(TestBoss, testMap.GridCoords);
            phases = entMan.GetComponent<MobPhasesComponent>(bossEntity);
            phaseSystem = entSysMan.GetEntitySystem<MobPhasesSystem>();
        });

        await server.WaitRunTicks(1);

        var baseline = new Dictionary<FixedPoint2, int>(phases.BasePhaseThresholds);
        await server.WaitAssertion(() => phaseSystem.ScaleAllPhaseThresholds(bossEntity, 2f));

        Assert.That(phases.BasePhaseThresholds, Is.EquivalentTo(baseline));
        Assert.That(phases.PhaseThresholds, Is.Not.SameAs(phases.BasePhaseThresholds));

        await server.WaitAssertion(() => phaseSystem.UnscaleAllPhaseThresholds(bossEntity));

        Assert.That(phases.PhaseThresholds, Is.EquivalentTo(baseline));
        Assert.That(phases.PhaseThresholds, Is.Not.SameAs(phases.BasePhaseThresholds));
    }

    [TestCase("MobChildishOni")]
    [TestCase("MobSpiderMercuryUltimate")]
    public async Task ArenaFieldCleanupRecoversLostTrackingWhenHarvestableBossDies(string prototype)
    {
        var server = Pair.Server;
        var client = Pair.Client;
        var testMap = await Pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();

        EntityUid boss = default;
        MegafaunaFieldGeneratorComponent field = null!;
        MegafaunaAiComponent ai = null!;
        EntityUid[] spawnedWalls = [];

        await server.WaitAssertion(() =>
        {
            boss = entMan.SpawnAtPosition(new EntProtoId(prototype), testMap.GridCoords);
            field = entMan.GetComponent<MegafaunaFieldGeneratorComponent>(boss);
            ai = entMan.GetComponent<MegafaunaAiComponent>(boss);
            entSysMan.GetEntitySystem<MegafaunaSystem>().StartupMegafauna((boss, ai));

            Assert.That(field.Enabled, Is.True);
            Assert.That(ai.Active, Is.True);
            Assert.That(field.Walls, Is.Not.Empty);
            Assert.That(field.Walls.All(wall =>
                entMan.TryGetComponent<MegafaunaFieldWallComponent>(wall, out var owned) &&
                owned.Generator == boss), Is.True);
            spawnedWalls = field.Walls.ToArray();
        });

        await RunUntilSynced();
        await client.WaitAssertion(() =>
        {
            var expectedWall = prototype == "MobChildishOni"
                ? "WallChildishOniMagmaTemporary"
                : "WallHierophantArenaTemporary";
            Assert.That(CountClientPrototype(client.EntMan, expectedWall), Is.GreaterThan(0));
        });

        await server.WaitAssertion(() =>
        {
            // Reproduce the live failure: the networked list and runtime owner
            // can both be lost while the visible field entities remain. Also
            // move the boss away from the activation point before killing it.
            foreach (var wall in spawnedWalls)
                entMan.RemoveComponent<MegafaunaFieldWallComponent>(wall);
            field.Walls.Clear();
            entMan.DirtyEntity(boss);
            entSysMan.GetEntitySystem<SharedTransformSystem>().SetCoordinates(
                boss,
                testMap.GridCoords.Offset(new Vector2(25f, 0f)));

            entMan.RemoveComponent<MegafaunaGodmodeComponent>(boss);
            var thresholds = entSysMan.GetEntitySystem<MobThresholdSystem>();
            Assert.That(thresholds.TryGetThresholdForState(boss, MobState.Dead, out var dead), Is.True);
            var blunt = SProtoMan.Index(BluntDamage);
            Assert.That(entSysMan.GetEntitySystem<DamageableSystem>().TryChangeDamage(
                boss,
                new DamageSpecifier(blunt, (int) Math.Ceiling(dead!.Value.Float()) + 1),
                ignoreResistances: true,
                canMiss: false), Is.True);
        });
        await server.WaitRunTicks(5);
        await RunUntilSynced();

        await server.WaitAssertion(() =>
        {
            // Simulate any late aggression/AI callbacks from the lethal hit.
            // Neither entry point may reactivate a dead boss or its field.
            entSysMan.GetEntitySystem<MegafaunaSystem>().StartupMegafauna((boss, ai));
            entSysMan.GetEntitySystem<MegafaunaFieldSystem>().ActivateField((boss, field));
        });
        await server.WaitRunTicks(2);
        await RunUntilSynced();

        await server.WaitAssertion(() =>
        {
            Assert.That(ai.Active, Is.False);
            Assert.That(field.Enabled, Is.False);
            Assert.That(field.Walls, Is.Empty);
            Assert.That(spawnedWalls.All(uid => !entMan.EntityExists(uid)), Is.True);
        });

        await client.WaitAssertion(() =>
        {
            var expectedWall = prototype == "MobChildishOni"
                ? "WallChildishOniMagmaTemporary"
                : "WallHierophantArenaTemporary";
            Assert.That(CountClientPrototype(client.EntMan, expectedWall), Is.Zero);
        });
    }

    private static int CountClientPrototype(IEntityManager entityManager, string prototype)
    {
        var count = 0;
        var query = entityManager.EntityQueryEnumerator<MetaDataComponent>();
        while (query.MoveNext(out _, out var metadata))
        {
            if (metadata.EntityPrototype?.ID == prototype)
                count++;
        }

        return count;
    }

    [Test]
    public async Task ChildishOniPhaseChangesReachClientSprite()
    {
        var server = Pair.Server;
        var client = Pair.Client;
        var testMap = await Pair.CreateTestMap();
        var serverEntities = server.ResolveDependency<IEntityManager>();
        var damage = server.System<DamageableSystem>();
        var blunt = SProtoMan.Index(BluntDamage);

        EntityUid oni = default;
        await server.WaitAssertion(() =>
        {
            oni = serverEntities.SpawnAtPosition(new EntProtoId("MobChildishOni"), testMap.GridCoords);
            serverEntities.RemoveComponent<MegafaunaGodmodeComponent>(oni);
        });

        foreach (var (amount, expectedState) in new[]
                 {
                     (2, "phase_1"),
                     (700, "phase_2"),
                     (900, "phase_3"),
                 })
        {
            await server.WaitAssertion(() => Assert.That(
                damage.TryChangeDamage(
                    oni,
                    new DamageSpecifier(blunt, amount),
                    ignoreResistances: true,
                    canMiss: false),
                Is.True));
            await server.WaitRunTicks(2);
            await RunUntilSynced();

            var clientOni = client.EntMan.GetEntity(serverEntities.GetNetEntity(oni));
            await client.WaitAssertion(() =>
            {
                var sprite = client.EntMan.GetComponent<SpriteComponent>(clientOni);
                var spriteSystem = client.System<SpriteSystem>();
                Assert.That(spriteSystem.LayerMapTryGet((clientOni, sprite), "oni", out var layer, false), Is.True);
                Assert.That(spriteSystem.LayerGetRsiState((clientOni, sprite), layer).Name, Is.EqualTo(expectedState));
            });
        }
    }

    [Test]
    public async Task LegionEncounterHasBoundedSummonBudget()
    {
        var server = Pair.Server;
        var testMap = await Pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();

        await server.WaitAssertion(() =>
        {
            var legion = entMan.SpawnAtPosition(new EntProtoId("LavalandBossMegaLegion"), testMap.GridCoords);
            var boss = entMan.GetComponent<LegionBossComponent>(legion);
            var greaterLegion = entMan.SpawnAtPosition(new EntProtoId("MobLegionLarge"), testMap.GridCoords);

            Assert.That(boss.MaximumActiveSummons, Is.EqualTo(8));
            Assert.That(boss.MaximumActiveLargeSummons, Is.EqualTo(2));
            Assert.That(boss.ReactiveSummonCooldown, Is.EqualTo(2.5f));
            Assert.That(entMan.HasComponent<Robust.Shared.Spawners.TimedDespawnComponent>(greaterLegion), Is.True);
        });
    }

    [Test]
    public async Task PlayerScalingIgnoresAbandonedBodies()
    {
        var pair = Pair;
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();
        var sessions = await server.AddDummySessions(2);

        EntityUid boss = default;
        await server.WaitAssertion(() =>
        {
            boss = entMan.SpawnAtPosition(TestBoss, testMap.GridCoords);
            var coordinates = entMan.GetComponent<TransformComponent>(boss).Coordinates;
            var abandonedBody = entMan.SpawnEntity(null, coordinates);
            var secondPlayer = entMan.SpawnEntity(null, coordinates);
            var currentBody = entMan.SpawnEntity(null, coordinates);
            var aggressive = entMan.GetComponent<AggressiveComponent>(boss);
            var aggression = entSysMan.GetEntitySystem<AggressorsSystem>();

            server.PlayerMan.SetAttachedEntity(sessions[0], abandonedBody);
            server.PlayerMan.SetAttachedEntity(sessions[1], secondPlayer);
            aggression.AddAggressor((boss, aggressive), abandonedBody);
            aggression.AddAggressor((boss, aggressive), secondPlayer);

            server.PlayerMan.SetAttachedEntity(sessions[0], currentBody);
            aggression.AddAggressor((boss, aggressive), currentBody);

            Assert.That(aggressive.Aggressors, Has.Count.EqualTo(3));
            Assert.That(aggression.CountActivePlayers((boss, aggressive)), Is.EqualTo(2));
        });

        await server.WaitAssertion(() =>
        {
            var thresholds = entSysMan.GetEntitySystem<Content.Shared.Mobs.Systems.MobThresholdSystem>();
            Assert.That(thresholds.TryGetThresholdForState(boss, Content.Shared.Mobs.MobState.Dead, out var dead), Is.True);
            Assert.That(dead!.Value.Float(), Is.EqualTo(3000f).Within(0.01f));

            var phases = entMan.GetComponent<MobPhasesComponent>(boss);
            Assert.That(phases.PhaseThresholds.Keys, Does.Contain((FixedPoint2) 1200));
            Assert.That(phases.PhaseThresholds.Keys, Does.Contain((FixedPoint2) 2400));
        });
    }

    [Test]
    public async Task ChildishOniNestedTimedSpawnerDoesNotInvalidateQuery()
    {
        var pair = Pair;
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();

        await server.WaitPost(() =>
            entMan.SpawnAtPosition(new EntProtoId("ChildishOniSpiralMarker"), testMap.GridCoords));
        // The marker fires after 0.5 seconds; allow enough server ticks for it to
        // spawn a spiral skull that registers its own TimedSpawner component.
        await server.WaitRunTicks(20);

        await server.WaitAssertion(() =>
        {
            var query = entMan.EntityQueryEnumerator<ChildishOniSpiralingComponent>();
            Assert.That(query.MoveNext(out _, out _), Is.True);
        });
    }

    [Test]
    public async Task ChildishOniClawRequiresValidInRangeTarget()
    {
        var pair = Pair;
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var log = server.ResolveDependency<ILogManager>().GetSawmill("lavaland.oni-claw.test");
        var actions = server.System<SharedActionsSystem>();
        var megafauna = server.System<MegafaunaSystem>();
        var xform = server.System<SharedTransformSystem>();

        EntityUid oni = default;
        EntityUid target = default;
        await server.WaitPost(() =>
        {
            oni = entMan.SpawnAtPosition(new EntProtoId("MobChildishOni"), testMap.GridCoords);
            target = entMan.SpawnEntity(null, testMap.GridCoords);
        });
        await server.WaitRunTicks(1);

        await server.WaitAssertion(() =>
        {
            Assert.That(actions.TryGetActionById(oni, "ActionChildishOniClawSlash", out var claw), Is.True);
            Assert.That(entMan.HasComponent<EntityTargetActionComponent>(claw!.Value.Owner), Is.True);

            var oniPosition = xform.GetWorldPosition(oni);
            xform.SetWorldPosition(target, oniPosition + new Vector2(3f, 0f));

            var targeting = entMan.EnsureComponent<MegafaunaAiTargetingComponent>(oni);
            targeting.TargetEnt = target;
            targeting.TargetCoords = entMan.GetComponent<TransformComponent>(target).Coordinates;

            var request = megafauna.GetPerformEvent(oni, claw.Value.Owner);
            var oniActions = entMan.GetComponent<ActionsComponent>(oni);
            Assert.That(actions.CanPerformAction((oni, oniActions), claw.Value, request), Is.False);

            // This is the exact selector path that previously accepted the invalid
            // target and reached PerformActionSelector's fatal debug assertion.
            var selector = protoMan.Index(ChildishOniClawSelector).Selector;
            var random = new RobustRandom();
            random.SetSeed(6734);
            var args = new MegafaunaCalculationBaseArgs(megafauna, oni, entMan, protoMan, log, random);
            Assert.That(selector.CheckConditions(args), Is.False);
        });
    }

    [Test]
    public async Task ChildishOniRampageGracefullyDeclinesZeroLengthTarget()
    {
        var pair = Pair;
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var log = server.ResolveDependency<ILogManager>().GetSawmill("lavaland.oni-rampage.test");
        var actions = server.System<SharedActionsSystem>();
        var megafauna = server.System<MegafaunaSystem>();

        EntityUid oni = default;
        await server.WaitPost(() =>
            oni = entMan.SpawnAtPosition(new EntProtoId("MobChildishOni"), testMap.GridCoords));
        await server.WaitRunTicks(1);

        await server.WaitAssertion(() =>
        {
            Assert.That(actions.TryGetActionById(oni, "ActionChildishOniRampage", out var rampage), Is.True);

            var targeting = entMan.EnsureComponent<MegafaunaAiTargetingComponent>(oni);
            targeting.TargetEnt = null;
            targeting.TargetCoords = entMan.GetComponent<TransformComponent>(oni).Coordinates;

            var request = megafauna.GetPerformEvent(oni, rampage!.Value.Owner);
            var oniActions = entMan.GetComponent<ActionsComponent>(oni);
            Assert.That(actions.CanPerformAction((oni, oniActions), rampage.Value, request), Is.True);

            var selector = protoMan.Index(ChildishOniRampageSelector).Selector;
            var random = new RobustRandom();
            random.SetSeed(8741);
            var args = new MegafaunaCalculationBaseArgs(megafauna, oni, entMan, protoMan, log, random);

            Assert.That(selector.CheckConditions(args), Is.True);
            Assert.That(selector.Invoke(args), Is.EqualTo(selector.FailDelay));
        });
    }

    [Test]
    public async Task OniGourdUsesOnlyItsOriginalSpriteState()
    {
        var client = Pair.Client;
        var entMan = client.EntMan;
        var spriteSystem = client.System<SpriteSystem>();

        await client.WaitAssertion(() =>
        {
            var gourd = entMan.Spawn("DrinkOniGourd");
            var sprite = entMan.GetComponent<SpriteComponent>(gourd);
            var visualizer = entMan.GetComponent<GenericVisualizerComponent>(gourd);

            Assert.Multiple(() =>
            {
                Assert.That(visualizer.Visuals, Is.Empty);
                Assert.That(spriteSystem.LayerMapTryGet(gourd, OpenableVisuals.Layer, out _, false), Is.False);
                Assert.That(spriteSystem.LayerGetRsiState((gourd, sprite), 0).Name, Is.EqualTo("icon"));
            });

            entMan.DeleteEntity(gourd);
        });
    }

    [Test]
    public async Task CancelledChasmFallNeverRestoresZeroSpriteScale()
    {
        var client = Pair.Client;
        var entMan = client.EntMan;

        await client.WaitAssertion(() =>
        {
            var entity = entMan.Spawn("MobSkeletonPerson");
            var sprite = entMan.GetComponent<SpriteComponent>(entity);
            var falling = entMan.EnsureComponent<ChasmFallingComponent>(entity);

            // Reproduce the stale zero value that used to arrive from the server
            // when Mercury's teleporting fissure cancelled an ordinary chasm fall.
            falling.OriginalScale = Vector2.Zero;
            entMan.RemoveComponent<ChasmFallingComponent>(entity);

            Assert.Multiple(() =>
            {
                Assert.That(MathF.Abs(sprite.Scale.X), Is.GreaterThanOrEqualTo(0.01f));
                Assert.That(MathF.Abs(sprite.Scale.Y), Is.GreaterThanOrEqualTo(0.01f));
            });

            entMan.DeleteEntity(entity);
        });
    }

    [Test]
    public async Task ConcurrentMegafaunaBlinksDoNotInvalidateTheirQuery()
    {
        var server = Pair.Server;
        var testMap = await Pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var blinkSystem = server.System<MegafaunaBlinkSystem>();
        EntityUid first = default;
        EntityUid second = default;

        await server.WaitPost(() =>
        {
            first = entMan.SpawnEntity(null, testMap.GridCoords);
            second = entMan.SpawnEntity(null, testMap.GridCoords);
            blinkSystem.Blink(first, testMap.GridCoords.Offset(new Vector2(1f, 0f)), TimeSpan.Zero);
            blinkSystem.Blink(second, testMap.GridCoords.Offset(new Vector2(2f, 0f)), TimeSpan.Zero);
        });

        await server.WaitRunTicks(2);
        await server.WaitAssertion(() =>
        {
            Assert.That(entMan.HasComponent<MegafaunaActiveBlinkComponent>(first), Is.False);
            Assert.That(entMan.HasComponent<MegafaunaActiveBlinkComponent>(second), Is.False);
        });
    }

    [Test]
    public async Task MercuryStarCellAlwaysCompletesUltimateTransition()
    {
        var server = Pair.Server;
        var testMap = await Pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();

        await server.WaitPost(() =>
            entMan.SpawnAtPosition(new EntProtoId("SpiderMercuryStarCell"), testMap.GridCoords));

        // Star cell (8 s) -> warp-in (1.3 s) -> ultimate form.
        await server.WaitRunTicks(330);
        await server.WaitAssertion(() =>
        {
            var found = false;
            var query = entMan.EntityQueryEnumerator<MetaDataComponent, TransformComponent>();
            while (query.MoveNext(out _, out var metadata, out var transform))
            {
                if (metadata.EntityPrototype?.ID == "MobSpiderMercuryUltimate"
                    && transform.MapID == testMap.MapId)
                {
                    found = true;
                    break;
                }
            }

            Assert.That(found, Is.True);
        });
    }

    [Test]
    public async Task MercuryThunderSoundCollectionSpawnsCleanly()
    {
        var server = Pair.Server;
        var testMap = await Pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            Assert.That(protoMan.HasIndex(new ProtoId<SoundCollectionPrototype>("ThunderStrike")), Is.True);
            Assert.DoesNotThrow(() =>
                entMan.SpawnAtPosition(new EntProtoId("ThunderSound"), testMap.GridCoords));
        });
    }

    [Test]
    public async Task ClientAggressionCleanupHandlesReplicationRemovalOrder()
    {
        var client = Pair.Client;
        var entMan = client.EntMan;

        await client.WaitAssertion(() =>
        {
            var boss = entMan.Spawn("MobSkeletonPerson");
            var player = entMan.Spawn("MobSkeletonPerson");
            var aggressive = entMan.EnsureComponent<AggressiveComponent>(boss);

            // Reproduce a client game-state deletion in which the boss's
            // forward set arrives after the player's reverse component was
            // already removed by replication.
            aggressive.Aggressors.Add(player);
            Assert.That(entMan.HasComponent<AggressorComponent>(player), Is.False);

            Assert.DoesNotThrow(() => entMan.DeleteEntity(boss));
            entMan.DeleteEntity(player);
        });
    }

    [Test]
    public async Task ClassicBossRewardsMatchRedStarSource()
    {
        var server = Pair.Server;
        var testMap = await Pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var tables = server.System<EntityTableSystem>();
        var whitelist = server.System<EntityWhitelistSystem>();

        static HashSet<string> SpawnIds(EntityTableSystem system, EntityTablePrototype table)
            => system.ListSpawns(table).Select(entry => entry.spawn.Id).ToHashSet();

        await server.WaitAssertion(() =>
        {
            var ashTable = protoMan.Index(new ProtoId<EntityTablePrototype>("AshDrakeNecropolisCrateTable"));
            var colossusTable = protoMan.Index(new ProtoId<EntityTablePrototype>("ColossusNecropolisCrateTable"));
            var bubblegumTable = protoMan.Index(new ProtoId<EntityTablePrototype>("BubblegumNecropolisCrateTable"));

            Assert.That(SpawnIds(tables, ashTable), Is.EquivalentTo(new[]
            {
                "WeaponSpectralBlade",
                "LavaStaffRod",
                "BottleDragonBlood",
                "SacredFlameSpellbook",
                "WeaponWandFireball",
            }));
            Assert.That(SpawnIds(tables, colossusTable), Is.EquivalentTo(new[]
            {
                "ColossusAnomalousCrystal",
                "ColossusAnomalousCrystalReprise",
                "ColossusAnomalousCrystalRepulsion",
                "ColossusAnomalousCrystalStasis",
                "ColossusAnomalousCrystalWard",
                "WeaponCainAbel",
            }));
            Assert.That(SpawnIds(tables, bubblegumTable), Is.EquivalentTo(new[]
            {
                "ClothingOuterArmorHostileEnv",
                "ClothingHeadHelmetHostileEnv",
                "MayhemBottle",
                "WeaponSoulScythe",
                "BloodContract",
            }));

            var bubblegum = entMan.SpawnAtPosition(new EntProtoId(BubblegumBoss), testMap.GridCoords);
            var colossus = entMan.SpawnAtPosition(new EntProtoId("LavalandBossColossus"), testMap.GridCoords);
            var ashDrake = entMan.SpawnAtPosition(new EntProtoId("LavalandBossAshDrake"), testMap.GridCoords);
            var legion = entMan.SpawnAtPosition(new EntProtoId("LavalandBossMegaLegion"), testMap.GridCoords);
            var pka = entMan.SpawnAtPosition(new EntProtoId("WeaponProtoKineticAccelerator"), testMap.GridCoords);
            var crowbar = entMan.SpawnAtPosition(new EntProtoId("Crowbar"), testMap.GridCoords);

            Assert.That(entMan.GetComponent<BubblegumBossComponent>(bubblegum).RewardsProto,
                Is.EquivalentTo(new[] { new EntProtoId("LavalandCrateNecropolisBubblegumFilled") }));

            foreach (var boss in new[] { colossus, ashDrake })
            {
                var loot = entMan.GetComponent<SpawnLootOnDeathComponent>(boss);
                Assert.That(loot.Table, Is.Not.Null);
                Assert.That(whitelist.IsWhitelistPassOrNull(loot.SpecialWeaponWhitelist, pka), Is.True);
                Assert.That(whitelist.IsWhitelistPassOrNull(loot.SpecialWeaponWhitelist, crowbar), Is.False);
            }

            Assert.That(SpawnIds(tables, protoMan.Index(
                    new ProtoId<EntityTablePrototype>("ColossusNecropolisCrateTable"))),
                Does.Not.Contain("OrganDivineVocalCords"),
                "the vocal cords are an exclusive carcass-harvest reward");

            var legionBoss = entMan.GetComponent<LegionBossComponent>(legion);
            var legionLoot = entMan.GetComponent<SpawnLootOnDeathComponent>(legion);
            Assert.Multiple(() =>
            {
                Assert.That(legionBoss.SplitPrototypes, Has.Count.EqualTo(3));
                Assert.That(legionLoot.DropOnDeath, Is.False);
                Assert.That(legionLoot.DropBoth, Is.True);
                Assert.That(legionLoot.Table, Is.Not.Null);
                Assert.That(legionLoot.SpecialTable, Is.Not.Null);
                Assert.That(tables.GetSpawns(legionLoot.Table!),
                    Is.EquivalentTo(new[] { new EntProtoId("LavalandCrateNecropolisFilled") }));
                Assert.That(tables.GetSpawns(legionLoot.SpecialTable!),
                    Is.EquivalentTo(new[] { new EntProtoId("TrophyLavalandLegionSkull") }));
                Assert.That(whitelist.IsWhitelistPassOrNull(legionLoot.SpecialWeaponWhitelist, pka), Is.True);
                Assert.That(whitelist.IsWhitelistPassOrNull(legionLoot.SpecialWeaponWhitelist, crowbar), Is.False);
                Assert.That(entMan.HasComponent<MegafaunaWeaponLooterComponent>(pka), Is.True);
                Assert.That(entMan.HasComponent<MegafaunaWeaponLooterComponent>(crowbar), Is.False);
            });

            var rewardItems = new[]
            {
                "WeaponSpectralBlade",
                "LavaStaffRod",
                "BottleDragonBlood",
                "OrganDivineVocalCords",
                "WeaponSpellBlade",
                "GemHollowCrystal",
                "GemBloodStone",
                "ClothingOuterArmorHostileEnv",
                "ClothingHeadHelmetHostileEnv",
                "LegionCore",
                "ChemistryBottleStabilizingSerum",
                "MobLowerAshDrake",
            };
            foreach (var id in rewardItems)
                Assert.DoesNotThrow(() => entMan.SpawnAtPosition(new EntProtoId(id), testMap.GridCoords), id);

            var core = entMan.SpawnAtPosition(new EntProtoId("LegionCore"), testMap.GridCoords);
            Assert.That(entMan.HasComponent<LegionCoreComponent>(core), Is.True);
            var blood = entMan.SpawnAtPosition(new EntProtoId("BottleDragonBlood"), testMap.GridCoords);
            Assert.That(entMan.HasComponent<DragonBloodComponent>(blood), Is.True);
            var blade = entMan.SpawnAtPosition(new EntProtoId("WeaponSpectralBlade"), testMap.GridCoords);
            Assert.That(entMan.HasComponent<SoulStorageComponent>(blade), Is.True);
        });
    }

    [Test]
    public async Task BubblegumTripleChargeTelegraphsMovesAndDealsDamage()
    {
        var server = Pair.Server;
        var testMap = await Pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();
        var mapSystem = server.System<SharedMapSystem>();

        EntityUid bubblegum = default;
        EntityUid target = default;
        await server.WaitPost(() =>
        {
            // Create enough real floor for a six-tile charge. CreateTestMap only supplies one tile.
            for (var x = -10; x <= 10; x++)
            {
                for (var y = -10; y <= 10; y++)
                    mapSystem.SetTile(testMap.Grid, new Vector2i(x, y), new Tile(1));
            }

            bubblegum = entMan.SpawnAtPosition(new EntProtoId(BubblegumBoss), testMap.GridCoords);
            target = entMan.SpawnAtPosition(new EntProtoId("MobHuman"),
                testMap.GridCoords.Offset(new Vector2(4f, 0f)));
        });
        await server.WaitRunTicks(2);

        await server.WaitAssertion(() =>
        {
            Assert.That(entMan.HasComponent<MegafaunaSpecialAttackOnlyComponent>(bubblegum), Is.True,
                "ported megafauna must retain melee targeting without being allowed to perform basic attacks");

            var selector = entSysMan.GetEntitySystem<NPCUseActionsOnTargetSystem>();
            var actions = entMan.GetComponent<NPCUseActionsOnTargetComponent>(bubblegum);
            var tripleDashId = new EntProtoId<TargetActionComponent>("ActionBubblegumTripleDash");

            selector.SetActions(bubblegum,
                new List<EntProtoId<TargetActionComponent>> { tripleDashId },
                new Dictionary<EntProtoId<TargetActionComponent>, float>
                {
                    [tripleDashId] = 1f,
                },
                actions);

            Assert.That(selector.TryUseRandomAction((bubblegum, actions), target), Is.True,
                "Bubblegum must execute its Paradise phase-one Triple Dash");
            Assert.That(actions.RecentActions, Is.EquivalentTo(new[] { tripleDashId }));
        });

        // Let the action event finish and its spawned telegraph enter the entity query.
        await server.WaitRunTicks(1);
        await server.WaitAssertion(() =>
        {
            var boss = entMan.GetComponent<BubblegumBossComponent>(bubblegum);
            Assert.That(boss.LastDashStatus, Is.EqualTo("telegraphed"));
            Assert.That(boss.LastDashMarker, Is.Not.Null,
                "Triple Dash must create a visible landing telegraph before movement begins");
        });

        // The three Paradise-style rev windows plus movement complete in under four seconds.
        await server.WaitRunTicks(120);
        await server.WaitAssertion(() =>
        {
            var damageable = entMan.GetComponent<DamageableComponent>(target);
            var damage = entSysMan.GetEntitySystem<DamageableSystem>().GetAllDamage((target, damageable));
            Assert.That(damage.DamageDict.TryGetValue("Blunt", out var blunt) ? blunt : FixedPoint2.Zero,
                Is.GreaterThanOrEqualTo(FixedPoint2.New(30)),
                "Triple Dash must deal damage when it crosses its target");
        });
    }

    [Test]
    public async Task ClassicBossRewardSpritesLoadOnClient()
    {
        var client = Pair.Client;
        var entMan = client.EntMan;
        var rewards = new[]
        {
            "GemHollowCrystal",
            "GemBloodStone",
            "ClothingOuterArmorHostileEnv",
            "ClothingHeadHelmetHostileEnv",
            "LavaStaffRod",
            "BottleDragonBlood",
            "OrganDivineVocalCords",
            "WeaponSpectralBlade",
            "WeaponSpellBlade",
            "LegionCore",
        };

        await client.WaitAssertion(() =>
        {
            foreach (var id in rewards)
            {
                var item = entMan.Spawn(id);
                Assert.That(entMan.HasComponent<SpriteComponent>(item), Is.True, id);
                entMan.DeleteEntity(item);
            }
        });
    }

    [Test]
    public async Task MercuryArenaAwakensFromDormantCore()
    {
        var server = Pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();
        var mapLoader = entSysMan.GetEntitySystem<MapLoaderSystem>();
        var damageable = entSysMan.GetEntitySystem<DamageableSystem>();
        var blunt = server.ResolveDependency<IPrototypeManager>().Index(BluntDamage);

        Entity<MapComponent>? arenaMap = null;
        EntityUid dormant = default;
        await server.WaitAssertion(() =>
        {
            Assert.That(mapLoader.TryLoadMap(
                new ResPath("/Maps/_Lavaland/Lavaland/ORT_arena.yml"),
                out arenaMap,
                out _,
                options: new DeserializationOptions { InitializeMaps = true }), Is.True);
            Assert.That(arenaMap, Is.Not.Null);
            Assert.That(entMan.HasComponent<MapLightComponent>(arenaMap.Value.Owner), Is.True);
            entSysMan.GetEntitySystem<SharedMapSystem>().SetPaused(arenaMap.Value.Comp.MapId, false);

            var query = entMan.EntityQueryEnumerator<MetaDataComponent, TransformComponent>();
            while (query.MoveNext(out var uid, out var metadata, out var transform))
            {
                if (metadata.EntityPrototype?.ID == "MegafaunaORTDormant"
                    && transform.MapUid == arenaMap.Value.Owner)
                {
                    dormant = uid;
                    break;
                }
            }

            Assert.That(dormant, Is.Not.EqualTo(default(EntityUid)));
            Assert.That(damageable.TryChangeDamage(
                dormant,
                new DamageSpecifier(blunt, 50),
                ignoreResistances: true,
                canMiss: false), Is.True);
        });

        await server.WaitRunTicks(2);
        EntityUid forming = default;
        await server.WaitAssertion(() =>
        {
            var query = entMan.EntityQueryEnumerator<MetaDataComponent, TransformComponent>();
            while (query.MoveNext(out var uid, out var metadata, out var transform))
            {
                if (metadata.EntityPrototype?.ID == "ORTFormingAnimation"
                    && transform.MapUid == arenaMap!.Value.Owner)
                {
                    forming = uid;
                    break;
                }
            }

            Assert.That(forming, Is.Not.EqualTo(default(EntityUid)));
        });

        // The source animation lasts 6.5 seconds before spawning the pulsing shell.
        await server.WaitRunTicks(210);
        EntityUid shell = default;
        await server.WaitAssertion(() =>
        {
            var query = entMan.EntityQueryEnumerator<MetaDataComponent, TransformComponent>();
            while (query.MoveNext(out var uid, out var metadata, out var transform))
            {
                if (metadata.EntityPrototype?.ID == "ORTForming"
                    && transform.MapUid == arenaMap!.Value.Owner)
                {
                    shell = uid;
                    break;
                }
            }

            Assert.That(shell, Is.Not.EqualTo(default(EntityUid)));
            Assert.That(damageable.TryChangeDamage(
                shell,
                new DamageSpecifier(blunt, 500),
                ignoreResistances: true,
                canMiss: false), Is.True);
        });

        // Destroying the shell plays a short combat animation, then starts phase one.
        await server.WaitRunTicks(35);
        await server.WaitAssertion(() =>
        {
            var foundBoss = false;
            var query = entMan.EntityQueryEnumerator<MetaDataComponent, TransformComponent>();
            while (query.MoveNext(out _, out var metadata, out var transform))
            {
                if (metadata.EntityPrototype?.ID == "MobSpiderMercury"
                    && transform.MapUid == arenaMap!.Value.Owner)
                {
                    foundBoss = true;
                    break;
                }
            }

            Assert.That(foundBoss, Is.True);
        });
    }

    [Test]
    public async Task CoreFrameworkContracts()
    {
        var pair = Pair;
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var bubblegumTestMap = await pair.CreateTestMap();
        var ashDrakeTestMap = await pair.CreateTestMap();
        var colossusTestMap = await pair.CreateTestMap();
        var megaLegionTestMap = await pair.CreateTestMap();
        var bloodDrunkMinerTestMap = await pair.CreateTestMap();
        var childishOniTestMap = await pair.CreateTestMap();
        var spiderMercuryTestMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var configuration = server.ResolveDependency<IConfigurationManager>();
        var mapLoader = entSysMan.GetEntitySystem<MapLoaderSystem>();
        var log = server.ResolveDependency<ILogManager>().GetSawmill("lavaland.megafauna.test");
        var random = new RobustRandom();
        random.SetSeed(3269);

        // Selector conditions short-circuit correctly and All selectors run in priority order.
        var calls = new List<int>();
        var conditional = new RecordingSelector(calls, 1, 3f)
        {
            FailDelay = 7f,
            Conditions =
            [
                new ConstantCondition(true),
                new ConstantCondition(false),
            ],
        };
        var emptyArgs = new MegafaunaCalculationBaseArgs(null!, default, null!, null!, null!, random);
        Assert.That(conditional.Invoke(emptyArgs), Is.EqualTo(7f));
        Assert.That(calls, Is.Empty);

        conditional.RequireAllConditions = false;
        Assert.That(conditional.Invoke(emptyArgs), Is.EqualTo(3f));
        Assert.That(calls, Is.EqualTo(new[] { 1 }));

        calls.Clear();
        var all = new AllMegafaunaSelector
        {
            Children =
            [
                new RecordingSelector(calls, 20, 2f) { Priority = 20 },
                new RecordingSelector(calls, -10, 5f) { Priority = -10 },
            ],
        };
        Assert.That(all.Invoke(emptyArgs), Is.EqualTo(5f));
        Assert.That(calls, Is.EqualTo(new[] { -10, 20 }));

        // Shape calls must be deterministic and must not mutate shared prototype state.
        var single = new SingleEntityShape();
        Assert.That(single.GetShape(random, protoMan, new Vector2(4, 6)),
            Is.EqualTo(new[] { new Vector2(4, 6) }));
        Assert.That(single.Offset, Is.EqualTo(Vector2.Zero));
        Assert.That(single.GetShape(random, protoMan), Is.EqualTo(new[] { Vector2.Zero }));
        Assert.That(ShapeHelpers.MakeBoxFilled(Vector2.Zero, 3).Count(), Is.EqualTo(9));
        Assert.That(ShapeHelpers.MakeBoxHollow(Vector2.Zero, 1).Count(), Is.EqualTo(8));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ShapeHelpers.MakeCross(Vector2.Zero, 3, 0f).ToList());

        // Ruin candidates and their seeded shuffle are reproducible, and
        // reservations actually subtract coordinates instead of unioning them back in.
        var candidatesA = LavalandRuinPlacement.GenerateCandidates(2, 2);
        var candidatesB = LavalandRuinPlacement.GenerateCandidates(2, 2);
        IRobustRandom placementRandomA = new RobustRandom();
        IRobustRandom placementRandomB = new RobustRandom();
        placementRandomA.SetSeed(117);
        placementRandomB.SetSeed(117);
        placementRandomA.Shuffle(candidatesA);
        placementRandomB.Shuffle(candidatesB);
        Assert.That(candidatesA, Is.EqualTo(candidatesB));
        Assert.That(candidatesA.Distinct().Count(), Is.EqualTo(candidatesA.Count));

        var filtered = LavalandRuinPlacement.ExcludeReserved(
            candidatesA,
            new[] { new Box2(-0.5f, -0.5f, 0.5f, 0.5f) });
        Assert.That(filtered, Does.Not.Contain(Vector2i.Zero));
        Assert.That(filtered, Has.Count.EqualTo(candidatesA.Count - 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LavalandRuinPlacement.GenerateCandidates(0, 2));

        var bubblegumArena = protoMan.Index<LavalandGridRuinPrototype>(BubblegumArena);
        var ashDrakeArena = protoMan.Index<LavalandGridRuinPrototype>(AshDrakeArena);
        var colossusArena = protoMan.Index<LavalandGridRuinPrototype>(ColossusArena);
        var megaLegionArena = protoMan.Index<LavalandGridRuinPrototype>(MegaLegionArena);
        var bloodDrunkMinerArena = protoMan.Index<LavalandGridRuinPrototype>(BloodDrunkMinerArena);
        var childishOniArena = protoMan.Index<LavalandGridRuinPrototype>(ChildishOniArena);
        var mercuryFissure = protoMan.Index<LavalandGridRuinPrototype>(MercuryFissure);
        var alloyConstruction = protoMan.Index<ConstructionPrototype>(
            new ProtoId<ConstructionPrototype>("SpiderMercuryAlloyConstruction"));
        var alloyGraph = protoMan.Index(alloyConstruction.Graph);
        var railgunConstruction = protoMan.Index<ConstructionPrototype>(
            new ProtoId<ConstructionPrototype>("SpiderMercuryRailgunConstruction"));
        var railgunGraph = protoMan.Index(railgunConstruction.Graph);
        Assert.Multiple(() =>
        {
            Assert.That(bubblegumArena.Path.ToString(),
                Is.EqualTo("/Maps/_Wega/Lavaland/bubblegumspawn.yml"));
            Assert.That(bubblegumArena.Clearance, Is.EqualTo(12f));
            Assert.That(protoMan.HasIndex<EntityPrototype>(BubblegumMarker), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(BubblegumBoss), Is.True);
            Assert.That(ashDrakeArena.Path.ToString(),
                Is.EqualTo("/Maps/_Wega/Lavaland/dragonlair.yml"));
            Assert.That(colossusArena.Path.ToString(),
                Is.EqualTo("/Maps/_Wega/Lavaland/colossusspawn.yml"));
            Assert.That(megaLegionArena.Path.ToString(),
                Is.EqualTo("/Maps/_Wega/Lavaland/megalegionspawn.yml"));
            Assert.That(bloodDrunkMinerArena.Path.ToString(),
                Is.EqualTo("/Maps/_Wega/Lavaland/blooddrunkminerspawn.yml"));
            Assert.That(childishOniArena.Path.ToString(),
                Is.EqualTo("/Maps/_Lavaland/Lavaland/ruin_childish_oni_arena.yml"));
            Assert.That(mercuryFissure.Path.ToString(),
                Is.EqualTo("/Maps/_Lavaland/Lavaland/ORT_fissure.yml"));
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("TrophyLavalandLegionSkull")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("TrophyLavalandBDMEye")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("TrophyLavalandAshDrakeSpike")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("TrophyLavalandBubblegumDemonClaws")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("TrophyLavalandColossusBlasterTubes")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("TrophyChildishOniHorn")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("TrophySpiderMercuryAlloy")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("WeaponChildishOniBlade")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("ChildishOniSpiralMarker")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("ChildishOniSkullTemporaryLong")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("ChildishOniHandLeft")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("ChildishOniHandRight")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("SpiderMercuryCore")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("MaterialSpiderMercuryKeratin10")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("MaterialSpiderMercuryAlloy1")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("WeaponSpiderMercuryRailgun")), Is.True);
            Assert.That(protoMan.HasIndex<ConstructionPrototype>(
                new ProtoId<ConstructionPrototype>("SpiderMercuryAlloyConstruction")), Is.True);
            Assert.That(protoMan.HasIndex<ConstructionPrototype>(
                new ProtoId<ConstructionPrototype>("SpiderMercuryRailgunConstruction")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("MobSpiderMercuryUfo")), Is.True);
            Assert.That(protoMan.HasIndex<EntityPrototype>(new EntProtoId("MobSpiderMercuryUltimate")), Is.True);
            Assert.That(configuration.GetCVar(LavalandCVars.MegafaunaDirectorEnabled), Is.True);
            Assert.That(Attribute.IsDefined(typeof(ChildishOniVisuals), typeof(NetSerializableAttribute)), Is.True);
            Assert.That(Attribute.IsDefined(typeof(ChildishOniPhaseVisual), typeof(NetSerializableAttribute)), Is.True);
            Assert.That(alloyGraph.TryPath(alloyConstruction.StartNode, alloyConstruction.TargetNode, out var alloyPath), Is.True);
            Assert.That(alloyPath, Has.Length.GreaterThanOrEqualTo(1));
            Assert.That(railgunGraph.TryPath(railgunConstruction.StartNode, railgunConstruction.TargetNode, out var railgunPath), Is.True);
            Assert.That(railgunPath, Has.Length.GreaterThanOrEqualTo(1));
        });

        EntityUid boss = default;
        EntityUid target = default;
        EntityUid multiTargetAction = default;
        MegafaunaAiComponent ai = null!;
        AggressiveComponent aggressive = null!;
        MegafaunaSystem megafauna = null!;
        AggressorsSystem aggressors = null!;
        SharedTransformSystem xform = null!;

        await server.WaitPost(() =>
        {
            boss = entMan.SpawnAtPosition(TestBoss, testMap.GridCoords);
            target = entMan.SpawnEntity(null, testMap.GridCoords);
            multiTargetAction = entMan.SpawnEntity("ActionHierophantChasers", MapCoordinates.Nullspace);
            ai = entMan.GetComponent<MegafaunaAiComponent>(boss);
            aggressive = entMan.GetComponent<AggressiveComponent>(boss);
            aggressive.UpdateDelay = TimeSpan.Zero;
            aggressive.NextUpdate = TimeSpan.Zero;
            megafauna = entSysMan.GetEntitySystem<MegafaunaSystem>();
            aggressors = entSysMan.GetEntitySystem<AggressorsSystem>();
            xform = entSysMan.GetEntitySystem<SharedTransformSystem>();

            aggressors.AddAggressor((boss, aggressive), target);
            aggressors.AddAggressor((boss, aggressive), target); // Idempotent.
        });

        Assert.Multiple(() =>
        {
            Assert.That(ai.Active, Is.True);
            Assert.That(ai.Schedule, Has.Count.EqualTo(1));
            Assert.That(aggressive.Aggressors, Is.EqualTo(new[] { target }));
            Assert.That(entMan.GetComponent<AggressorComponent>(target).Aggressives,
                Is.EqualTo(new[] { boss }));
        });

        await server.WaitAssertion(() =>
        {
            var args = new MegafaunaCalculationBaseArgs(megafauna, boss, entMan, protoMan, log, random);
            var pickTarget = new AggressivePickTargetSelector();
            Assert.That(pickTarget.Invoke(args), Is.EqualTo(0f));

            var targeting = entMan.GetComponent<MegafaunaAiTargetingComponent>(boss);
            Assert.That(targeting.TargetEnt, Is.EqualTo(target));
            Assert.That(targeting.TargetCoords,
                Is.EqualTo(entMan.GetComponent<TransformComponent>(target).Coordinates));

            Assert.That(entMan.HasComponent<EntityTargetActionComponent>(multiTargetAction), Is.True);
            Assert.That(entMan.HasComponent<WorldTargetActionComponent>(multiTargetAction), Is.True);
            var perform = megafauna.GetPerformEvent(boss, multiTargetAction);
            Assert.That(perform.EntityTarget, Is.EqualTo(entMan.GetNetEntity(target)));
            Assert.That(perform.EntityCoordinatesTarget, Is.Not.Null);

            xform.SetWorldPosition(target, xform.GetWorldPosition(boss) + new Vector2(50, 0));
        });

        await server.WaitRunTicks(1);

        Assert.Multiple(() =>
        {
            Assert.That(aggressive.Aggressors, Is.Empty);
            Assert.That(entMan.HasComponent<AggressorComponent>(target), Is.False);
            Assert.That(ai.Active, Is.False);
            Assert.That(ai.Schedule, Is.Empty);
            var targeting = entMan.GetComponent<MegafaunaAiTargetingComponent>(boss);
            Assert.That(targeting.TargetEnt, Is.Null);
            Assert.That(targeting.TargetCoords, Is.Null);
            Assert.That(protoMan.HasIndex<BossMusicPrototype>(TestMusic), Is.True);
        });

        Entity<MapGridComponent>? arenaGrid = null;
        await server.WaitAssertion(() =>
        {
            Assert.That(mapLoader.TryLoadGrid(bubblegumTestMap.MapId, bubblegumArena.Path, out arenaGrid), Is.True);
        });
        Assert.That(arenaGrid, Is.Not.Null);

        Entity<MapGridComponent>? ashDrakeGrid = null;
        Entity<MapGridComponent>? colossusGrid = null;
        Entity<MapGridComponent>? megaLegionGrid = null;
        Entity<MapGridComponent>? bloodDrunkMinerGrid = null;
        Entity<MapGridComponent>? childishOniGrid = null;
        Entity<MapGridComponent>? mercuryFissureGrid = null;
        Entity<MapComponent>? mercuryArenaMap = null;
        await server.WaitAssertion(() =>
        {
            Assert.That(mapLoader.TryLoadGrid(ashDrakeTestMap.MapId, ashDrakeArena.Path, out ashDrakeGrid), Is.True);
            Assert.That(mapLoader.TryLoadGrid(colossusTestMap.MapId, colossusArena.Path, out colossusGrid), Is.True);
            Assert.That(mapLoader.TryLoadGrid(megaLegionTestMap.MapId, megaLegionArena.Path, out megaLegionGrid), Is.True);
            Assert.That(mapLoader.TryLoadGrid(bloodDrunkMinerTestMap.MapId, bloodDrunkMinerArena.Path, out bloodDrunkMinerGrid), Is.True);
            Assert.That(mapLoader.TryLoadGrid(childishOniTestMap.MapId, childishOniArena.Path, out childishOniGrid), Is.True);
            Assert.That(mapLoader.TryLoadGrid(spiderMercuryTestMap.MapId, mercuryFissure.Path, out mercuryFissureGrid), Is.True);
            Assert.That(mapLoader.TryLoadMap(
                new ResPath("/Maps/_Lavaland/Lavaland/ORT_arena.yml"),
                out mercuryArenaMap,
                out _,
                options: new DeserializationOptions { InitializeMaps = true }), Is.True);
        });
        Assert.Multiple(() =>
        {
            Assert.That(ashDrakeGrid, Is.Not.Null);
            Assert.That(colossusGrid, Is.Not.Null);
            Assert.That(megaLegionGrid, Is.Not.Null);
            Assert.That(bloodDrunkMinerGrid, Is.Not.Null);
            Assert.That(childishOniGrid, Is.Not.Null);
            Assert.That(mercuryFissureGrid, Is.Not.Null);
            Assert.That(mercuryArenaMap, Is.Not.Null);
            Assert.That(entMan.HasComponent<MapLightComponent>(mercuryArenaMap!.Value.Owner), Is.True);
        });

        // All generated boss grids must be discoverable in Ghost Warp. The five
        // Wega source grids lacked beacons and were therefore present but hidden.
        await server.WaitAssertion(() =>
        {
            var beaconParents = new HashSet<EntityUid>();
            var beaconQuery = entMan.EntityQueryEnumerator<NavMapBeaconComponent, TransformComponent>();
            while (beaconQuery.MoveNext(out _, out _, out var transform))
                beaconParents.Add(transform.ParentUid);

            Assert.That(beaconParents, Does.Contain(arenaGrid!.Value.Owner));
            Assert.That(beaconParents, Does.Contain(ashDrakeGrid!.Value.Owner));
            Assert.That(beaconParents, Does.Contain(colossusGrid!.Value.Owner));
            Assert.That(beaconParents, Does.Contain(megaLegionGrid!.Value.Owner));
            Assert.That(beaconParents, Does.Contain(bloodDrunkMinerGrid!.Value.Owner));
            Assert.That(beaconParents, Does.Contain(childishOniGrid!.Value.Owner));
            Assert.That(beaconParents, Does.Contain(mercuryFissureGrid!.Value.Owner));
        });

        await server.WaitAssertion(() =>
        {
            var foundPit = false;
            var pitQuery = entMan.EntityQueryEnumerator<ChasmTeleportComponent, TransformComponent>();
            while (pitQuery.MoveNext(out _, out _, out var transform))
            {
                if (transform.ParentUid != mercuryFissureGrid!.Value.Owner)
                    continue;

                foundPit = true;
                break;
            }

            var foundDormantMercury = false;
            var dormantQuery = entMan.EntityQueryEnumerator<VicinitySpawnerComponent,
                MetaDataComponent,
                TransformComponent>();
            while (dormantQuery.MoveNext(out _, out _, out var metadata, out var transform))
            {
                if (metadata.EntityPrototype?.ID != "MegafaunaORTDormant"
                    || transform.MapUid != mercuryArenaMap!.Value.Owner)
                    continue;

                foundDormantMercury = true;
                break;
            }

            Assert.That(foundPit, Is.True);
            Assert.That(foundDormantMercury, Is.True);
        });

        // The arena marker must materialize the complete boss, including its initial action set.
        await server.WaitRunTicks(2);

        // The arena legitimately chooses between four Blood-Drunk Miner variants and the Demonic Frost
        // Miner. Spawn the specific actor this contract exercises when the arena selected Frost, instead of
        // making the test randomly fail one run in five.
        await server.WaitPost(() =>
        {
            var minerQuery = entMan.EntityQueryEnumerator<BloodDrunkMinerComponent, TransformComponent>();
            while (minerQuery.MoveNext(out _, out _, out var transform))
            {
                if (transform.ParentUid == bloodDrunkMinerGrid!.Value.Owner)
                    return;
            }

            entMan.SpawnAtPosition(new EntProtoId("MobBloodDrunkMiner"),
                new EntityCoordinates(bloodDrunkMinerGrid!.Value.Owner, Vector2.Zero));
        });
        await server.WaitRunTicks(2);

        EntityUid bloodDrunkMiner = default;
        await server.WaitAssertion(() =>
        {
            var ashQuery = entMan.EntityQueryEnumerator<AshDrakeBossComponent,
                NPCUseActionsOnTargetComponent,
                TransformComponent>();
            var foundAshDrake = false;
            while (ashQuery.MoveNext(out _, out _, out var actions, out var transform))
            {
                if (transform.ParentUid != ashDrakeGrid!.Value.Owner)
                    continue;

                Assert.That(transform.ParentUid, Is.EqualTo(ashDrakeGrid.Value.Owner));
                Assert.That(actions.ActionEnts, Has.Count.EqualTo(3));
                foundAshDrake = true;
                break;
            }

            var colossusQuery = entMan.EntityQueryEnumerator<ColossusBossComponent,
                NPCUseActionsOnTargetComponent,
                TransformComponent>();
            var foundColossus = false;
            while (colossusQuery.MoveNext(out _, out _, out var actions, out var transform))
            {
                if (transform.ParentUid != colossusGrid!.Value.Owner)
                    continue;

                Assert.That(transform.ParentUid, Is.EqualTo(colossusGrid.Value.Owner));
                Assert.That(actions.ActionEnts, Has.Count.EqualTo(4));
                foundColossus = true;
                break;
            }

            Assert.That(foundAshDrake, Is.True);
            Assert.That(foundColossus, Is.True);

            var legionQuery = entMan.EntityQueryEnumerator<LegionBossComponent,
                NPCUseActionsOnTargetComponent,
                TransformComponent>();
            var foundLegion = false;
            while (legionQuery.MoveNext(out _, out _, out var actions, out var transform))
            {
                if (transform.ParentUid != megaLegionGrid!.Value.Owner)
                    continue;

                Assert.That(actions.ActionEnts, Has.Count.EqualTo(1));
                foundLegion = true;
                break;
            }

            var minerQuery = entMan.EntityQueryEnumerator<BloodDrunkMinerComponent,
                MegafaunaDirectorComponent,
                TransformComponent>();
            var foundMiner = false;
            while (minerQuery.MoveNext(out var uid, out _, out _, out var transform))
            {
                if (transform.ParentUid != bloodDrunkMinerGrid!.Value.Owner)
                    continue;

                Assert.Multiple(() =>
                {
                    Assert.That(entMan.HasComponent<AshStormImmuneComponent>(uid), Is.True);
                    Assert.That(entMan.HasComponent<AggressiveComponent>(uid), Is.True);
                    Assert.That(entMan.HasComponent<BossMusicComponent>(uid), Is.True);
                    Assert.That(entMan.HasComponent<SpawnLootOnDeathComponent>(uid), Is.True);
                    Assert.That(entMan.HasComponent<LoadoutComponent>(uid), Is.False);
                });
                bloodDrunkMiner = uid;
                foundMiner = true;
                break;
            }

            Assert.That(foundLegion, Is.True);
            Assert.That(foundMiner, Is.True);
        });

        // The ADT-derived miner owns its melee component directly; NavSmash and
        // the HTN operator must resolve that same owner/component pair.
        await server.WaitAssertion(() =>
        {
            var melee = entSysMan.GetEntitySystem<SharedMeleeWeaponSystem>();
            Assert.That(melee.TryGetWeapon(bloodDrunkMiner, out var weaponUid, out var weapon), Is.True);
            Assert.That(entMan.GetComponent<MeleeWeaponComponent>(weaponUid), Is.SameAs(weapon));
        });

        EntityUid childishOni = default;
        EntityUid spiderCombat = default;
        EntityUid spiderUfo = default;
        EntityUid spiderUltimate = default;
        EntityUid spiderRailgun = default;
        MegafaunaDirectorComponent childishOniDirector = null!;
        await server.WaitPost(() =>
        {
            spiderCombat = entMan.SpawnAtPosition(new EntProtoId("MobSpiderMercury"), testMap.GridCoords);
            spiderUfo = entMan.SpawnAtPosition(new EntProtoId("MobSpiderMercuryUfo"), testMap.GridCoords);
            spiderUltimate = entMan.SpawnAtPosition(new EntProtoId("MobSpiderMercuryUltimate"), testMap.GridCoords);
            spiderRailgun = entMan.SpawnAtPosition(new EntProtoId("WeaponSpiderMercuryRailgun"), testMap.GridCoords);
        });

        await server.WaitAssertion(() =>
        {
            var actionsSystem = entSysMan.GetEntitySystem<SharedActionsSystem>();
            var oniQuery = entMan.EntityQueryEnumerator<ChildishOniComponent,
                MegafaunaDirectorComponent,
                TransformComponent>();
            while (oniQuery.MoveNext(out var uid, out _, out var director, out var transform))
            {
                if (transform.ParentUid != childishOniGrid!.Value.Owner)
                    continue;

                childishOni = uid;
                childishOniDirector = director;
                Assert.That(actionsSystem.GetActions(uid).Count(), Is.GreaterThanOrEqualTo(9));
                Assert.That(actionsSystem.TryGetActionById(uid, "ActionChildishOniFlurry", out var flurry), Is.True);
                Assert.That(entMan.HasComponent<InstantActionComponent>(flurry!.Value.Owner), Is.True);
                Assert.That(entMan.HasComponent<TargetActionComponent>(flurry.Value.Owner), Is.False);
                Assert.That(actionsSystem.TryGetActionById(uid, "ActionChildishOniRingWide", out var ring), Is.True);
                Assert.That(entMan.HasComponent<InstantActionComponent>(ring!.Value.Owner), Is.True);
                Assert.That(entMan.HasComponent<TargetActionComponent>(ring.Value.Owner), Is.False);
                Assert.That(actionsSystem.TryGetActionById(uid, "ActionChildishOniHandBarrage", out var hands), Is.True);
                Assert.That(entMan.HasComponent<WorldTargetActionComponent>(hands!.Value.Owner), Is.True);
                Assert.That(entMan.HasComponent<TargetActionComponent>(hands.Value.Owner), Is.True);
                Assert.That(director.BaseHealthThreshold, Is.EqualTo((FixedPoint2) 2600));
                Assert.That(entMan.HasComponent<SpawnLootOnDeathComponent>(uid), Is.True);
                break;
            }

            var spiderQuery = entMan.EntityQueryEnumerator<EtherDrainComponent,
                SpiderMercuryStageComponent,
                MegafaunaDirectorComponent,
                TransformComponent>();
            var foundSpider = false;
            while (spiderQuery.MoveNext(out var uid, out _, out var stage, out var director, out var transform))
            {
                if (uid != spiderCombat)
                    continue;

                Assert.That(actionsSystem.GetActions(uid).Count(), Is.GreaterThanOrEqualTo(5));
                Assert.That(actionsSystem.TryGetActionById(uid, "ActionORTEtherDrain", out _), Is.True);
                Assert.That(actionsSystem.TryGetActionById(uid, "ActionORTPlanktonFlood", out _), Is.True);
                Assert.That(actionsSystem.TryGetActionById(uid, "ActionORTCosmicRays", out _), Is.True);
                Assert.That(actionsSystem.TryGetActionById(uid, "ActionORTResonanceVertical", out _), Is.True);
                Assert.That(actionsSystem.TryGetActionById(uid, "ActionORTResonanceHorizontal", out _), Is.True);
                Assert.That(entMan.HasComponent<CosmicRayCirculatorComponent>(uid), Is.True);
                Assert.That(entMan.HasComponent<EnvironmentalResonanceComponent>(uid), Is.True);
                Assert.That(stage.NextStage, Is.EqualTo(new EntProtoId("MobSpiderMercuryUfo")));
                Assert.That(director.CountKill, Is.False);
                Assert.That(director.BaseHealthThreshold, Is.EqualTo((FixedPoint2) 1500));
                foundSpider = true;
                break;
            }

            Assert.That(childishOni, Is.Not.EqualTo(default(EntityUid)));
            Assert.That(foundSpider, Is.True);

            var ufoStage = entMan.GetComponent<SpiderMercuryStageComponent>(spiderUfo);
            var ufoDirector = entMan.GetComponent<MegafaunaDirectorComponent>(spiderUfo);
            Assert.That(actionsSystem.GetActions(spiderUfo).Count(), Is.GreaterThanOrEqualTo(3));
            Assert.That(ufoStage.NextStage, Is.Null);
            Assert.That(ufoStage.TransitionEffect, Is.EqualTo(new EntProtoId("SpiderMercuryUfoDeathAnimation")));
            Assert.That(ufoDirector.CountKill, Is.False);

            var ultimateDirector = entMan.GetComponent<MegafaunaDirectorComponent>(spiderUltimate);
            Assert.That(actionsSystem.GetActions(spiderUltimate).Count(), Is.GreaterThanOrEqualTo(10));
            Assert.That(entMan.HasComponent<SpiderMercuryStageComponent>(spiderUltimate), Is.False);
            Assert.That(ultimateDirector.CountKill, Is.True);
            Assert.That(entMan.HasComponent<SpawnLootOnDeathComponent>(spiderUltimate), Is.True);
            Assert.That(entMan.HasComponent<ORTSolarStormComponent>(spiderUltimate), Is.True);
            Assert.That(entMan.HasComponent<ParadigmInflationComponent>(spiderUltimate), Is.True);
            Assert.That(entMan.HasComponent<PhaseConversionComponent>(spiderUltimate), Is.True);
            Assert.That(entMan.HasComponent<ReflectiveThreadsComponent>(spiderUltimate), Is.True);
            Assert.That(entMan.HasComponent<OrbitingRingComponent>(spiderUltimate), Is.True);
            Assert.That(entMan.HasComponent<ORTConvergenceComponent>(spiderUltimate), Is.True);
            Assert.That(entMan.HasComponent<ORTTransportMatterComponent>(spiderUltimate), Is.True);

            Assert.That(actionsSystem.TryGetActionById(spiderUltimate, "ActionORTSolarStorm", out var solar), Is.True);
            Assert.That(entMan.HasComponent<InstantActionComponent>(solar!.Value.Owner), Is.True);
            Assert.That(entMan.HasComponent<TargetActionComponent>(solar.Value.Owner), Is.False);
            Assert.That(actionsSystem.TryGetActionById(spiderUltimate, "ActionORTParadigmInflation", out var paradigm), Is.True);
            Assert.That(entMan.HasComponent<EntityTargetActionComponent>(paradigm!.Value.Owner), Is.True);
            Assert.That(entMan.HasComponent<TargetActionComponent>(paradigm.Value.Owner), Is.True);

            var railgunItem = entMan.GetComponent<ItemComponent>(spiderRailgun);
            Assert.That(railgunItem.Size, Is.EqualTo(new ProtoId<ItemSizePrototype>("Ginormous")));
            Assert.That(railgunItem.Shape, Is.EqualTo(new[] { new Box2i(0, 0, 3, 1) }));
        });

        // The first hit advances the Oni to phase two and writes a network-safe
        // appearance value. This is the path that previously crashed PVS serialization.
        var oniDamageable = entSysMan.GetEntitySystem<DamageableSystem>();
        var bluntPrototype = protoMan.Index(BluntDamage);
        await server.WaitPost(() =>
        {
            // The integration entity has no player session to satisfy the normal
            // MegafaunaGodmode origin check, so remove only that test guard.
            entMan.RemoveComponent<MegafaunaGodmodeComponent>(childishOni);
            Assert.That(
                oniDamageable.TryChangeDamage(
                    childishOni,
                    new DamageSpecifier(bluntPrototype, 2),
                    ignoreResistances: true,
                    canMiss: false),
                Is.True);
        });
        await server.WaitRunTicks(2);
        await server.WaitAssertion(() =>
        {
            var phases = entMan.GetComponent<MobPhasesComponent>(childishOni);
            var appearance = entSysMan.GetEntitySystem<SharedAppearanceSystem>();
            Assert.That(phases.CurrentPhase, Is.EqualTo(2));
            Assert.That(appearance.TryGetData(
                childishOni,
                ChildishOniVisuals.Phase,
                out ChildishOniPhaseVisual visual), Is.True);
            Assert.That(visual, Is.EqualTo(ChildishOniPhaseVisual.Phase1));
        });

        // The Whiskey Director scales from the immutable threshold using active
        // player sessions. Reattaching a session to another body must not count
        // both the abandoned body and the new body as separate participants.
        var participants = await server.AddDummySessions(2);
        await server.WaitAssertion(() =>
        {
            var oniAggressive = entMan.GetComponent<AggressiveComponent>(childishOni);
            var oniCoords = entMan.GetComponent<TransformComponent>(childishOni).Coordinates;
            var staleBody = entMan.SpawnEntity(null, oniCoords);
            var participantTwo = entMan.SpawnEntity(null, oniCoords);
            var participantOne = entMan.SpawnEntity(null, oniCoords);
            var aggressionSystem = entSysMan.GetEntitySystem<AggressorsSystem>();

            server.PlayerMan.SetAttachedEntity(participants[0], staleBody);
            server.PlayerMan.SetAttachedEntity(participants[1], participantTwo);
            aggressionSystem.AddAggressor((childishOni, oniAggressive), staleBody);
            aggressionSystem.AddAggressor((childishOni, oniAggressive), participantTwo);
            server.PlayerMan.SetAttachedEntity(participants[0], participantOne);
            aggressionSystem.AddAggressor((childishOni, oniAggressive), participantOne);

            Assert.That(oniAggressive.Aggressors, Has.Count.EqualTo(3));
            Assert.That(aggressionSystem.CountActivePlayers((childishOni, oniAggressive)), Is.EqualTo(2));
            Assert.That(childishOniDirector.PeakPartySize, Is.EqualTo(2));
            Assert.That(childishOniDirector.AppliedHealthMultiplier, Is.EqualTo(1.2f).Within(0.001f));
            var thresholds = entSysMan.GetEntitySystem<Content.Shared.Mobs.Systems.MobThresholdSystem>();
            Assert.That(thresholds.TryGetThresholdForState(childishOni, Content.Shared.Mobs.MobState.Dead, out var dead), Is.True);
            Assert.That(dead!.Value.Float(), Is.EqualTo(3120f).Within(0.01f));
        });

        // The server CVar restores prototype health/cadence immediately and
        // re-enables scaling from the same immutable baseline.
        FixedPoint2? disabledThreshold = null;
        var disabledDirector = false;
        await server.WaitPost(() =>
        {
            configuration.SetCVar(LavalandCVars.MegafaunaDirectorEnabled, false);
            var directorSystem = entSysMan.GetEntitySystem<MegafaunaDirectorSystem>();
            disabledDirector = !directorSystem.Enabled;
            var thresholds = entSysMan.GetEntitySystem<Content.Shared.Mobs.Systems.MobThresholdSystem>();
            thresholds.TryGetThresholdForState(childishOni, Content.Shared.Mobs.MobState.Dead, out disabledThreshold);

            // Always restore the shared test process, even if an assertion below fails.
            configuration.SetCVar(LavalandCVars.MegafaunaDirectorEnabled, true);
        });

        Assert.Multiple(() =>
        {
            Assert.That(disabledDirector, Is.True);
            Assert.That(disabledThreshold!.Value.Float(), Is.EqualTo(2600f).Within(0.01f));
            Assert.That(childishOniDirector.AppliedHealthMultiplier, Is.EqualTo(1.2f).Within(0.001f));
        });

        // Round elapsed time is the third bounded difficulty axis. Collapse
        // the interval in the test and cap it to one deterministic step.
        await server.WaitAssertion(() =>
        {
            childishOniDirector.ElapsedDifficultyInterval = TimeSpan.FromTicks(1);
            childishOniDirector.MaximumElapsedIntervals = 1;
            childishOniDirector.HealthPerElapsedInterval = 0.1f;
            childishOniDirector.ActionSpeedPerElapsedInterval = 0f;
            entSysMan.GetEntitySystem<MegafaunaDirectorSystem>()
                .ApplyDifficulty((childishOni, childishOniDirector));

            Assert.That(childishOniDirector.ElapsedDifficultySteps, Is.EqualTo(1));
            Assert.That(childishOniDirector.AppliedHealthMultiplier, Is.EqualTo(1.3f).Within(0.001f));
            var thresholds = entSysMan.GetEntitySystem<Content.Shared.Mobs.Systems.MobThresholdSystem>();
            Assert.That(thresholds.TryGetThresholdForState(childishOni, Content.Shared.Mobs.MobState.Dead, out var dead), Is.True);
            Assert.That(dead!.Value.Float(), Is.EqualTo(3380f).Within(0.01f));
        });

        EntityUid bubblegum = default;
        BubblegumBossComponent bubblegumComp = null!;
        NPCUseActionsOnTargetComponent bubblegumActions = null!;
        await server.WaitAssertion(() =>
        {
            var query = entMan.EntityQueryEnumerator<BubblegumBossComponent,
                NPCUseActionsOnTargetComponent,
                TransformComponent>();
            while (query.MoveNext(out var uid, out var bossComp, out var actions, out var transform))
            {
                if (transform.ParentUid != arenaGrid!.Value.Owner)
                    continue;

                Assert.That(transform.ParentUid, Is.EqualTo(arenaGrid.Value.Owner));
                bubblegum = uid;
                bubblegumComp = bossComp;
                bubblegumActions = actions;
                break;
            }

            Assert.That(bubblegum, Is.Not.EqualTo(default(EntityUid)));
            Assert.That(bubblegumActions.ActionIds, Is.EquivalentTo(bubblegumComp.Phase1Actions));
            Assert.That(bubblegumActions.ActionEnts, Has.Count.EqualTo(1));
            Assert.That(bubblegumActions.ActionEnts.Values.All(action =>
                action.HasValue && entMan.EntityExists(action.Value)), Is.True);
        });

        // Crossing 50% health atomically swaps Triple Dash for Paradise's three enraged charge patterns.
        // Blood Warp is a conditional supplement in the reference behavior, not a standalone rotation slot.
        var damageable = entSysMan.GetEntitySystem<DamageableSystem>();
        var blunt = protoMan.Index(BluntDamage);
        await server.WaitPost(() => Assert.That(
            damageable.TryChangeDamage(
                bubblegum,
                new DamageSpecifier(blunt, 1300),
                ignoreResistances: true,
                canMiss: false),
            Is.True));
        await server.WaitRunTicks(1);

        Assert.Multiple(() =>
        {
            Assert.That(bubblegumComp.CurrentPhase, Is.EqualTo(BubblegumPhase.Enraged));
            Assert.That(bubblegumActions.ActionIds, Is.EquivalentTo(bubblegumComp.Phase2Actions));
            Assert.That(bubblegumActions.ActionEnts, Has.Count.EqualTo(3));
            Assert.That(bubblegumActions.ActionEnts.Values.All(action =>
                action.HasValue && entMan.EntityExists(action.Value)), Is.True);
        });
    }

    private sealed partial class RecordingSelector : MegafaunaSelector
    {
        private readonly List<int> _calls;
        private readonly int _marker;
        private readonly float _delay;

        public RecordingSelector()
        {
            _calls = new List<int>();
        }

        public RecordingSelector(List<int> calls, int marker, float delay)
        {
            _calls = calls;
            _marker = marker;
            _delay = delay;
        }

        protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
        {
            _calls.Add(_marker);
            return _delay;
        }
    }

    private sealed partial class ConstantCondition : MegafaunaCondition
    {
        private readonly bool _result;

        public ConstantCondition()
        {
        }

        public ConstantCondition(bool result)
        {
            _result = result;
        }

        public override bool EvaluateImplementation(MegafaunaCalculationBaseArgs args) => _result;
    }

    /*[Test]
    public async Task TestMegafaunaAi()
    {
        var pair = Pair;
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();

        EntityUid bossEntity = default;
        MegafaunaAiComponent megafaunaAi = null;
        MegafaunaSystem megafaunaSystem = null;

        await server.WaitPost(() =>
        {
            bossEntity = entMan.SpawnAtPosition(TestBoss, testMap.GridCoords);
            megafaunaAi = entMan.GetComponent<MegafaunaAiComponent>(bossEntity);
            megafaunaSystem = entSysMan.GetEntitySystem<MegafaunaSystem>();
        });

        await server.WaitRunTicks(5);
    }*/
}
