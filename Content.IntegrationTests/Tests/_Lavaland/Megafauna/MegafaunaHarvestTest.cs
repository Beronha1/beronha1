// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.IntegrationTests.Tests.Interaction;
using Content.Lavaland.Common.Weapons.Marker;
using Content.Lavaland.Shared.BossRewards;
using Content.Lavaland.Shared.Chasm;
using Content.Lavaland.Shared.Artifacts;
using Content.Lavaland.Shared.Megafauna.Components;
using Content.Lavaland.Shared.Megafauna.Harvesting;
using Content.Lavaland.Shared.Megafauna.Mercury;
using Content.Lavaland.Shared.Megafauna.Utility;
using Content.Lavaland.Shared.MobPhases;
using Content.Lavaland.Shared.Pressure;
using Content.Lavaland.Shared.Research;
using Content.Lavaland.Shared.Weapons.Upgrades;
using Content.Lavaland.Server.Megafauna.Classic;
using Content.Lavaland.Server.Megafauna.Bubblegum;
using Content.Lavaland.Server.Mobs;
using Content.Lavaland.Server.Weapons;
using Content.Medical.Shared.Body;
using Content.Server.Construction.Components;
using Content.Medical.Shared.Surgery.Tools;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Body;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Construction.NodeEntities;
using Content.Shared.Construction.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Lathe;
using Content.Shared.Magic.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Research.Prototypes;
using Content.Shared.Stacks;
using Content.Shared.Storage.Components;
using Content.Shared.Timing;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;
using Content.Shared.Weapons.Reflect;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Trauma.Common.Bulletholes;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Localization;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Server.GameObjects;

namespace Content.IntegrationTests.Tests._Lavaland.Megafauna;

[TestFixture]
[TestOf(typeof(MegafaunaHarvestableComponent))]
public sealed class MegafaunaHarvestTest : GameTest
{
    [TestPrototypes]
    private const string MercuryRelicTestPrototypes = """
        - type: entity
          id: MercuryRelicTestUser
          parent: MobHuman

        - type: entity
          id: MegafaunaLootAccountingDummy
          parent: MobHuman
          components:
          - type: SpawnLootOnDeath
            dropOnDeath: false
            weaponWhitelist:
              components:
              - MegafaunaWeaponLooter
        """;

    private static readonly string[] HarvestBosses =
    [
        "LavalandBossAshDrake",
        "LavalandBossColossus",
        "LavalandBossBubblegum",
        "LavalandBossMegaLegion",
        "MobBloodDrunkMiner",
        "MobSpiderMercuryUltimate",
        "MobChildishOni",
    ];

    private static readonly string[] ProtectedExistingMobs =
    [
        "MobHierophant",
        "MobLavalandGoliath",
        "MobWatcherLavaland",
    ];

    private static readonly string[] Trophies =
    [
        "TrophyLavalandAshDrakeSpike",
        "TrophyLavalandColossusBlasterTubes",
        "TrophyLavalandBubblegumDemonClaws",
        "TrophyLavalandLegionSkull",
        "TrophyLavalandBDMEye",
        "TrophySpiderMercuryAlloy",
        "TrophyChildishOniHorn",
        "TrophyDemonicFrostMinerIceTalisman",
    ];

    private static readonly string[] ColossusCrystals =
    [
        "ColossusAnomalousCrystalReprise",
        "ColossusAnomalousCrystalRepulsion",
        "ColossusAnomalousCrystalStasis",
        "ColossusAnomalousCrystalWard",
    ];

    private static readonly string[] LocalizedRewardItems =
    [
        "LavaStaffRod",
        "BottleDragonBlood",
        "WeaponSpectralBlade",
        "WeaponSpellBlade",
        "SacredFlameSpellbook",
        "ChemistryBottleExothermicBlood",
        "MaterialDragonHide1",
        "MaterialDragonBone1",
        "MaterialDragonWingMembrane1",
        "MaterialMegafaunaSinew1",
        "TrophyLavalandAshDrakeSpike",
        "ColossusAnomalousCrystal",
        "ColossusAnomalousCrystalReprise",
        "ColossusAnomalousCrystalRepulsion",
        "ColossusAnomalousCrystalStasis",
        "ColossusAnomalousCrystalWard",
        "OrganDivineVocalCords",
        "MaterialNecroAlloy1",
        "WeaponCainAbel",
        "PersonalForcefieldGenerator",
        "WeaponColossusReflectionEmitter",
        "TrophyLavalandColossusBlasterTubes",
        "ClothingOuterArmorHostileEnv",
        "ClothingHeadHelmetHostileEnv",
        "BloodContract",
        "WeaponSoulScythe",
        "MayhemBottle",
        "FoodDemonicChewingGum",
        "ChemistryBottleDemonicBlood",
        "TrophyLavalandBubblegumDemonClaws",
        "OrganStabilizedLegionCore",
        "LegionCore",
        "ChemistryBottleStabilizingSerum",
        "LegionServitorCulture",
        "StaffOfStorms",
        "MaterialLegionSkull1",
        "TrophyLavalandLegionSkull",
        "WeaponCleavingSaw",
        "WeaponWildhunterKnife",
        "WeaponBloodDrunkKineticAccelerator",
        "WeaponPlasmaCutterOverclocked",
        "MaterialServoTier4_1",
        "MaterialHighDensityCircuit1",
        "MaterialTitaniumAlloy1",
        "TrophyLavalandBDMEye",
        "DemonicResurrectionCrystal",
        "ClothingShoesBootsCursedIce",
        "WeaponDemonicJackhammer",
        "IceEnergyCrystal",
        "TrophyDemonicFrostMinerIceTalisman",
        "MaterialSpiderMercuryKeratin1",
        "MaterialSpiderMercuryAlloy1",
        "MaterialMercurySilk1",
        "MaterialMirroredChitin1",
        "ChemistryBottleMercuryVenom",
        "SpiderMercuryCore",
        "SpiderMercuryEtherDrinker",
        "SpiderMercuryParadoxCanceller",
        "SpiderMercuryRadiantShield",
        "WeaponSpiderMercuryRailgun",
        "ClothingModsuitMercuryRadiant",
        "ClothingModsuitGauntletsMercuryRadiant",
        "ClothingModsuitHelmetMercuryRadiant",
        "ClothingModsuitChestplateMercuryRadiant",
        "ClothingModsuitBootsMercuryRadiant",
        "TrophySpiderMercuryAlloy",
        "DrinkOniGourd",
        "ChemistryBottleOniGastricEnzymes",
        "OniDensityCore",
        "WeaponChildishOniBlade",
        "ClothingOuterChildishOniFloweryDress",
        "ClothingHandsChildishOniCursed",
        "ClothingShoesChildishOniCursed",
        "ClothingBeltChildishOniObi",
        "ClothingHeadChildishOniZukin",
        "TrophyChildishOniHorn",
        "ClothingOuterArmorDragon",
        "ClothingHeadHelmetDragon",
        "ClothingOuterArmorDragonAdvanced",
        "ClothingOuterArmorGodslayer",
        "ClothingHeadHelmetGodslayer",
        "ClothingOuterArmorMercuryReflective",
        "CableSuperconductingStack10",
        "ClothingOuterArmorDrakeMercuryAegis",
        "ShieldColossusMercuryAnomalous",
        "MedipenLegionBubblegumRegenerator",
        "ClothingBeltAshFrostThermalRegulator",
        "WeaponBloodDrunkMercuryPhaseCutter",
        "OrganCompressedLegionCore",
        "DrakeRemains",
        "ResearchDestructorMachineCircuitboard",
        "WeaponProtoKineticRailgun",
        "WeaponProtoKineticShockwave",
        "PKAUpgradeMiningAoE",
        "PKAUpgradeOffensiveAoE",
        "PKAUpgradeHybridAoE",
        "PKAUpgradeHumanPassthrough",
        "PKAUpgradeDronePassthrough",
        "PKAUpgradeRapidRepeater",
        "PKAUpgradeResonatorBlast",
        "PKAUpgradeDeathSyphon",
        "PKAUpgradeTracerAmber",
    ];

    private static readonly (string Recipe, string Graph, string Node, string Result)[] MegafaunaConstructions =
    [
        ("DrakeRemainsConstruction", "DrakeRemainsConstructionGraph", "remains", "DrakeRemains"),
        ("DragonArmorConstruction", "DragonArmorConstructionGraph", "armor", "ClothingOuterArmorDragon"),
        ("LegionServitorCultureConstruction", "LegionServitorCultureConstructionGraph", "culture", "LegionServitorCulture"),
        ("GodslayerArmorConstruction", "GodslayerArmorConstructionGraph", "armor", "ClothingOuterArmorGodslayer"),
        ("DrakeMercuryAegisConstruction", "DrakeMercuryAegisConstructionGraph", "aegis", "ClothingOuterArmorDrakeMercuryAegis"),
        ("ColossusMercuryShieldConstruction", "ColossusMercuryShieldConstructionGraph", "shield", "ShieldColossusMercuryAnomalous"),
        ("LegionBubblegumRegeneratorConstruction", "LegionBubblegumRegeneratorConstructionGraph", "regenerator", "MedipenLegionBubblegumRegenerator"),
        ("AshFrostThermalRegulatorConstruction", "AshFrostThermalRegulatorConstructionGraph", "regulator", "ClothingBeltAshFrostThermalRegulator"),
        ("BloodDrunkMercuryPhaseCutterConstruction", "BloodDrunkMercuryPhaseCutterConstructionGraph", "cutter", "WeaponBloodDrunkMercuryPhaseCutter"),
        ("CompressedLegionCoreConstruction", "CompressedLegionCoreConstructionGraph", "core", "OrganCompressedLegionCore"),
        ("SpiderMercuryAlloyConstruction", "SpiderMercuryAlloyConstructionGraph", "alloy", "MaterialSpiderMercuryAlloy1"),
        ("SpiderMercuryRailgunConstruction", "SpiderMercuryRailgunConstructionGraph", "railgun", "WeaponSpiderMercuryRailgun"),
        ("SpiderMercuryCoreConstruction", "SpiderMercuryCoreConstructionGraph", "mercuryCore", "SpiderMercuryCore"),
        ("SpiderMercuryEtherDrinkerConstruction", "SpiderMercuryEtherDrinkerConstructionGraph", "etherDrinker", "SpiderMercuryEtherDrinker"),
        ("SpiderMercuryParadoxCancellerConstruction", "SpiderMercuryParadoxCancellerConstructionGraph", "paradoxCanceller", "SpiderMercuryParadoxCanceller"),
        ("SpiderMercuryRadiantShieldConstruction", "SpiderMercuryRadiantShieldConstructionGraph", "radiantShield", "SpiderMercuryRadiantShield"),
        ("SpiderMercuryRadiantModsuitConstruction", "SpiderMercuryRadiantModsuitConstructionGraph", "radiantModsuit", "ClothingModsuitMercuryRadiant"),
    ];

    private static readonly string[] MegafaunaLatheRecipes =
    [
        "ClothingOuterArmorDragonAdvanced",
        "CableSuperconductingStack10",
        "ClothingOuterArmorMercuryReflective",
        "PersonalForcefieldGenerator",
        "WeaponColossusReflectionEmitter",
        "GygaxArmorMegafauna",
        "GygaxCentralElectronicsMegafauna",
        "GygaxTargetingElectronicsMegafauna",
        "ResearchDestructorMachineCircuitboard",
        "SpiderMercuryCoreLathe",
        "SpiderMercuryEtherDrinkerLathe",
        "SpiderMercuryParadoxCancellerLathe",
        "SpiderMercuryRadiantShieldLathe",
        "WeaponSpiderMercuryRailgunLathe",
        "ClothingModsuitMercuryRadiantLathe",
        "WeaponProtoKineticRailgun",
        "WeaponProtoKineticShockwave",
        "PKAUpgradeMiningAoE",
        "PKAUpgradeOffensiveAoE",
        "PKAUpgradeHumanPassthrough",
        "PKAUpgradeDronePassthrough",
        "PKAUpgradeTracerAmber",
    ];

    // Prototype IDs in these data-driven assertions intentionally come from
    // arrays. Keep the analyzer-facing API in one place instead of repeating
    // obsolete component accessors and literal-ID warnings throughout the test.
    private static T Prototype<T>(IPrototypeManager prototypes, string id)
        where T : class, IPrototype
        => prototypes.Index<T>(id);

    private static bool HasPrototype<T>(IPrototypeManager prototypes, string id)
        where T : class, IPrototype
        => prototypes.HasIndex<T>(id);

    [Test]
    public async Task HarvestInfrastructureIsWiredToExactlyTheRequestedBosses()
    {
        var server = Pair.Server;
        var prototypes = server.ResolveDependency<IPrototypeManager>();
        var components = server.ResolveDependency<IComponentFactory>();
        var localization = server.ResolveDependency<ILocalizationManager>();

        await server.WaitAssertion(() =>
        {
            foreach (var id in HarvestBosses)
            {
                var prototype = Prototype<EntityPrototype>(prototypes, id);
                Assert.That(
                    prototype.TryComp<MegafaunaHarvestableComponent>(out var harvest, components),
                    Is.True,
                    $"{id} must retain a harvestable carcass");
                Assert.That(harvest!.Stages, Has.Count.EqualTo(2), $"{id} must have two ordered harvest stages");
                Assert.That(harvest.Stages.All(stage => stage.ToolQualities.Count > 0), Is.True);
                foreach (var stage in harvest.Stages)
                {
                    foreach (var qualityId in stage.ToolQualities)
                    {
                        Assert.That(prototypes.TryIndex<ToolQualityPrototype>(qualityId, out var quality), Is.True,
                            $"{id} harvest stage {stage.Name} references unknown tool quality {qualityId}");
                        Assert.That(localization.HasString(quality!.ToolName), Is.True,
                            $"{qualityId} must provide a localized tool name for carcass instructions");
                    }
                }
            }

            Assert.That(() => localization.GetString(
                    "megafauna-harvest-examine",
                    ("stage", "test tissue"),
                    ("current", 1),
                    ("total", 2),
                    ("tools", "Knife"),
                    ("seconds", 10d)),
                Throws.Nothing,
                "the instructional carcass examine text must accept all runtime arguments");

            foreach (var id in ProtectedExistingMobs)
            {
                var prototype = Prototype<EntityPrototype>(prototypes, id);
                Assert.That(
                    prototype.TryComp<MegafaunaHarvestableComponent>(out _, components),
                    Is.False,
                    $"{id} was explicitly outside this feature's scope");
            }

            foreach (var (bossId, carcassId) in new Dictionary<string, string>
                     {
                         ["LavalandBossAshDrake"] = "AshDrakeResidualCarcass",
                         ["LavalandBossBubblegum"] = "BubblegumResidualCarcass",
                         ["MobSpiderMercuryUltimate"] = "MercurySpiderResidualCarcass",
                         ["MobChildishOni"] = "ChildishOniResidualCarcass",
                     })
            {
                var prototype = Prototype<EntityPrototype>(prototypes, bossId);
                Assert.That(prototype.TryComp<MegafaunaHarvestableComponent>(out var harvest, components), Is.True);
                Assert.That(harvest!.CompletionCarcass, Is.EqualTo((EntProtoId) carcassId),
                    $"{bossId} must finish as its inert extractable carcass");
            }

            var trophyIds = new HashSet<string>();
            foreach (var id in Trophies)
            {
                var prototype = Prototype<EntityPrototype>(prototypes, id);
                Assert.That(prototype.TryComp<CrusherTrophyComponent>(out var trophy, components), Is.True);
                Assert.That(trophy!.TrophyId, Is.Not.Empty, $"{id} needs a stable trophy identity");
                Assert.That(trophyIds.Add(trophy.TrophyId), Is.True,
                    $"{id} duplicates the trophy identity {trophy.TrophyId}");
                Assert.That(trophy.CapacityCost, Is.GreaterThan(0).And.LessThanOrEqualTo(100));
                Assert.That(
                    prototype.TryComp<TrophyRecyclableComponent>(out _, components),
                    Is.True,
                    $"{id} must be finitely recyclable with the Wildhunter knife");
            }

            foreach (var id in LocalizedRewardItems)
            {
                var reward = Prototype<EntityPrototype>(prototypes, id);
                Assert.That(reward.Name, Is.Not.Empty, $"{id} must have an English display name");
                Assert.That(reward.Description, Is.Not.Empty, $"{id} must have an English description");
                Assert.That(reward.Name, Is.Not.EqualTo("solution").IgnoreCase,
                    $"{id} inherited the chemical base prototype name instead of its material name");
                Assert.That(reward.Description, Is.Not.EqualTo("A raw material.").IgnoreCase,
                    $"{id} retained the generic material description");
                Assert.That(reward.Name, Does.Not.EndWith("-name"),
                    $"{id} exposes an unresolved Fluent localization ID as its name");
                Assert.That(reward.Description, Does.Not.EndWith("-desc"),
                    $"{id} exposes an unresolved Fluent localization ID as its description");
            }

            // This intentionally follows the marker component instead of a hand-maintained list.
            // Any future physical boss-derived item is covered as soon as it joins the cargo chain.
            foreach (var reward in prototypes.EnumeratePrototypes<EntityPrototype>())
            {
                if (!reward.TryComp<MegafaunaProvenanceComponent>(out _, components) ||
                    !reward.TryComp<ItemComponent>(out _, components))
                    continue;

                Assert.That(reward.Name, Is.Not.Empty, $"{reward.ID} must have an English display name");
                Assert.That(reward.Description, Is.Not.Empty, $"{reward.ID} must have an English description");
                Assert.That(reward.Name, Is.Not.EqualTo("solution").IgnoreCase,
                    $"{reward.ID} inherited the chemical base prototype name");
                Assert.That(reward.Description, Is.Not.EqualTo("A raw material.").IgnoreCase,
                    $"{reward.ID} retained the generic material description");

                if (!reward.TryComp<StackComponent>(out var stack, components))
                    continue;

                Assert.That(
                    stack!.LayerStates,
                    Has.Count.GreaterThan(1),
                    $"{reward.ID} must not crash RoundToEqualLevels in the client stack visualizer");
            }

            var frostMiner = Prototype<EntityPrototype>(prototypes, "LavalandBossDemonicFrostMiner");
            Assert.That(frostMiner.TryComp<DemonicFrostMinerComponent>(out _, components), Is.True);
            Assert.That(frostMiner.TryComp<MegafaunaHarvestableComponent>(out var frostHarvest, components), Is.True);
            Assert.That(frostHarvest!.Stages, Has.Count.EqualTo(2));

            var bubblegumFirstLife = Prototype<EntityPrototype>(prototypes, "LavalandBossBubblegum");
            Assert.That(bubblegumFirstLife.TryComp<BubblegumBossComponent>(out var bubblegum, components), Is.True);
            Assert.That(bubblegum!.EnableSecondLife, Is.False);
            Assert.That(bubblegum.SecondLife, Is.False);
            Assert.That(
                bubblegumFirstLife.TryComp<MegafaunaHarvestableComponent>(out _, components),
                Is.True,
                "the normal Bubblegum encounter must leave the harvestable body on its original map");

            var bubblegumSecondLife = Prototype<EntityPrototype>(prototypes, "LavalandBossBubblegumSecondLife");
            Assert.That(bubblegumSecondLife.TryComp<BubblegumBossComponent>(out var unbound, components), Is.True);
            Assert.That(unbound!.SecondLife, Is.True);

            var ashDrake = Prototype<EntityPrototype>(prototypes, "LavalandBossAshDrake");
            Assert.That(
                ashDrake.TryComp<ToolRefinableComponent>(out _, components),
                Is.False,
                "the legacy slicing refiner consumes the whole carcass and bypasses staged harvesting");

            foreach (var id in new[] { "MaterialDragonWingMembrane", "MaterialMegafaunaSinew" })
            {
                var material = Prototype<EntityPrototype>(prototypes, id);
                Assert.That(material.TryComp<StackComponent>(out var stack, components), Is.True);
                Assert.That(
                    stack!.LayerStates,
                    Has.Count.GreaterThan(1),
                    $"{id} must not crash RoundToEqualLevels in the client stack visualizer");
            }

            var cleavingSaw = Prototype<EntityPrototype>(prototypes, "WeaponCleavingSaw");
            Assert.That(cleavingSaw.TryComp<CleavingSawComponent>(out _, components), Is.True);
            var wildhunter = Prototype<EntityPrototype>(prototypes, "WeaponWildhunterKnife");
            Assert.That(wildhunter.TryComp<WildhunterKnifeComponent>(out _, components), Is.True);
            var jackhammer = Prototype<EntityPrototype>(prototypes, "WeaponDemonicJackhammer");
            Assert.That(jackhammer.TryComp<DemonicJackhammerComponent>(out _, components), Is.True);
            var resurrectionCrystal = Prototype<EntityPrototype>(prototypes, "DemonicResurrectionCrystal");
            Assert.That(resurrectionCrystal.TryComp<ResurrectionCrystalComponent>(out var resurrection, components), Is.True);
            Assert.That(resurrection!.ReviveTime, Is.EqualTo(TimeSpan.FromSeconds(8)));
            var cursedBoots = Prototype<EntityPrototype>(prototypes, "ClothingShoesBootsCursedIce");
            Assert.That(cursedBoots.TryComp<CursedIceBootsComponent>(out _, components), Is.True);
            var godslayer = Prototype<EntityPrototype>(prototypes, "ClothingOuterArmorGodslayer");
            Assert.That(godslayer.TryComp<GodslayerArmorComponent>(out var godslayerArmor, components), Is.True);
            Assert.That(godslayerArmor!.RevivalDelay, Is.EqualTo(TimeSpan.FromSeconds(4)));

            var sacredFlameBook = Prototype<EntityPrototype>(prototypes, "SacredFlameSpellbook");
            Assert.That(sacredFlameBook.TryComp<SpellbookComponent>(out var sacredBook, components), Is.True);
            Assert.That(sacredBook!.SpellActions.Keys, Does.Contain((EntProtoId) "ActionSacredFlame"));
            var sacredFlameAction = Prototype<EntityPrototype>(prototypes, "ActionSacredFlame");
            Assert.That(sacredFlameAction.TryComp<InstantActionComponent>(out _, components), Is.True);

            foreach (var id in new[] { "MobBloodDrunkMinerGuidance", "MobBloodDrunkMinerHunter", "MobBloodDrunkMinerDoom" })
            {
                var variant = Prototype<EntityPrototype>(prototypes, id);
                Assert.That(variant.TryComp<BloodDrunkMinerComponent>(out _, components), Is.True);
            }

            foreach (var (recipeId, graphId, nodeId, resultId) in MegafaunaConstructions)
            {
                var recipe = Prototype<ConstructionPrototype>(prototypes, recipeId);
                Assert.That(recipe.Graph.Id, Is.EqualTo(graphId), $"{recipeId} points to the wrong graph");
                Assert.That(recipe.TargetNode, Is.EqualTo(nodeId), $"{recipeId} points to the wrong target node");

                var graph = Prototype<ConstructionGraphPrototype>(prototypes, graphId);
                Assert.That(graph.Nodes.TryGetValue(nodeId, out var node), Is.True,
                    $"{graphId} has no {nodeId} output node");
                Assert.That(node!.Entity, Is.TypeOf<StaticNodeEntity>(),
                    $"{graphId}:{nodeId} must produce a fixed entity prototype");
                Assert.That(((StaticNodeEntity) node.Entity).Id, Is.EqualTo(resultId),
                    $"{graphId}:{nodeId} produces the wrong entity");

                var result = Prototype<EntityPrototype>(prototypes, resultId);
                Assert.That(
                    result.TryComp<ConstructionComponent>(out var construction, components),
                    Is.True,
                    $"{resultId} must retain its construction graph or initial crafting deletes the result");
                Assert.That(construction!.Graph, Is.EqualTo(graphId));
                Assert.That(construction.Node, Is.EqualTo(nodeId));
            }

            var crusher = Prototype<EntityPrototype>(prototypes, "WeaponCrusher");
            Assert.That(crusher.TryComp<WeaponTrophySlotComponent>(out var crusherTrophySlots, components), Is.True);
            Assert.That(crusherTrophySlots!.SlotCount, Is.EqualTo(8));
            Assert.That(crusherTrophySlots.MaxTrophyCapacity, Is.EqualTo(100));

            var portablePka = Prototype<EntityPrototype>(prototypes, "WeaponProtoKineticAccelerator");
            Assert.That(portablePka.TryComp<WeaponTrophySlotComponent>(out var pkaTrophySlots, components), Is.True);
            Assert.That(pkaTrophySlots!.SlotCount, Is.EqualTo(8));
            Assert.That(pkaTrophySlots.MaxTrophyCapacity, Is.EqualTo(100));

            var railgunBolt = Prototype<EntityPrototype>(prototypes, "BulletKineticRailgun");
            Assert.That(railgunBolt.TryComp<KineticMobPenetrationProjectileComponent>(out _, components), Is.True,
                "the Paradise railgun must pass through mobs but stop on structures");

            var shockwaveSpread = Prototype<EntityPrototype>(prototypes, "BulletKineticShockwaveSpread");
            Assert.That(shockwaveSpread.TryComp<ProjectileSpreadComponent>(out var spread, components), Is.True);
            Assert.That(spread!.Count, Is.EqualTo(8));
            Assert.That(spread.Proto, Is.EqualTo((EntProtoId) "BulletKineticShockwave"),
                "shockwave children must use a non-spreading prototype to prevent recursive fragmentation");

            var demonClaws = Prototype<EntityPrototype>(prototypes, "TrophyLavalandBubblegumDemonClaws");
            Assert.That(demonClaws.TryComp<CrusherDemonClawsUpgradeComponent>(out var shotgun, components), Is.True);
            Assert.That(shotgun!.ProjectileCount, Is.EqualTo(3));
            Assert.That(shotgun.ProjectileSpread, Is.EqualTo(Angle.FromDegrees(45)));

            var legionSkull = Prototype<EntityPrototype>(prototypes, "TrophyLavalandLegionSkull");
            Assert.That(legionSkull.TryComp<CrusherLegionSkullUpgradeComponent>(out var skullLauncher, components), Is.True);
            Assert.That(skullLauncher!.SkullHitsRequired, Is.GreaterThanOrEqualTo(3));
            Assert.That(skullLauncher.SkullCooldown, Is.GreaterThan(TimeSpan.Zero),
                "the ranged Legion trophy must remain bounded by hit count and cooldown");

            var cords = Prototype<EntityPrototype>(prototypes, "OrganDivineVocalCords");
            Assert.That(cords.TryComp<DivineVocalCordsOrganComponent>(out _, components), Is.True);
            Assert.That(cords.TryComp<OrganComponent>(out var vocalOrgan, components), Is.True);
            Assert.That(vocalOrgan!.Category, Is.EqualTo((ProtoId<OrganCategoryPrototype>) "DivineVocalCords"));
            Assert.That(cords.TryComp<OrganActionsComponent>(out var vocalActions, components), Is.True);
            Assert.That(vocalActions!.Actions, Does.Contain((EntProtoId<ActionComponent>) "ActionColossusRoar"));

            var legionOrgan = Prototype<EntityPrototype>(prototypes, "OrganStabilizedLegionCore");
            Assert.That(legionOrgan.TryComp<StabilizedLegionCoreOrganComponent>(out var legionCore, components), Is.True);
            Assert.That(legionCore!.RevivalDelay, Is.EqualTo(TimeSpan.FromSeconds(4)));
            Assert.That(legionOrgan.TryComp<OrganComponent>(out var regenerativeOrgan, components), Is.True);
            Assert.That(regenerativeOrgan!.Category, Is.EqualTo((ProtoId<OrganCategoryPrototype>) "RegenerativeCore"));

            foreach (var surgery in new[]
                     {
                         "SurgeryOpenDivineVocalCordsCavity",
                         "SurgeryInsertDivineVocalCords",
                         "SurgeryRemoveDivineVocalCords",
                         "SurgeryOpenRegenerativeCoreCavity",
                         "SurgeryInsertRegenerativeCore",
                         "SurgeryRemoveRegenerativeCore",
                     })
            {
                Assert.That(HasPrototype<EntityPrototype>(prototypes, surgery), Is.True, surgery);
            }

            var crystal = Prototype<EntityPrototype>(prototypes, "ColossusAnomalousCrystal");
            Assert.That(crystal.TryComp<ResearchArtifactComponent>(out var artifact, components), Is.True);
            Assert.That(artifact!.Technologies, Does.Contain((ProtoId<TechnologyPrototype>) "ColossusAnomalyApplications"));

            foreach (var id in ColossusCrystals)
            {
                var crystalVariant = Prototype<EntityPrototype>(prototypes, id);
                Assert.That(crystalVariant.TryComp<AnomalousCrystalComponent>(out _, components), Is.True);
                Assert.That(crystalVariant.TryComp<ResearchArtifactComponent>(out var variantArtifact, components), Is.True);
                Assert.That(
                    variantArtifact!.Technologies,
                    Does.Contain((ProtoId<TechnologyPrototype>) "ColossusAnomalyApplications"),
                    $"{id} must unlock the Colossus research node when destructively analyzed");
            }

            var heckHelmet = Prototype<EntityPrototype>(prototypes, "ClothingHeadHelmetHostileEnv");
            Assert.That(heckHelmet.TryComp<WearableMegafaunaHarvesterComponent>(out var wearable, components), Is.True);
            Assert.That(heckHelmet.TryComp<HeckHelmetComponent>(out _, components), Is.True);
            Assert.That(wearable!.ToolQualities, Does.Contain("Slicing"));
            Assert.That(wearable.ToolQualities, Does.Contain("Sawing"));

            var heckSuit = Prototype<EntityPrototype>(prototypes, "ClothingOuterArmorHostileEnv");
            Assert.That(heckSuit.TryComp<HeckSuitComponent>(out _, components), Is.True);

            var bloodContract = Prototype<EntityPrototype>(prototypes, "BloodContract");
            Assert.That(bloodContract.TryComp<BloodContractComponent>(out var contract, components), Is.True);
            Assert.That(contract!.MarkDuration, Is.EqualTo(TimeSpan.FromMinutes(2)));

            var cainAbel = Prototype<EntityPrototype>(prototypes, "WeaponCainAbel");
            Assert.That(cainAbel.TryComp<CainAbelComponent>(out _, components), Is.True);

            var soulScythe = Prototype<EntityPrototype>(prototypes, "WeaponSoulScythe");
            Assert.That(soulScythe.TryComp<SoulScytheComponent>(out _, components), Is.True);

            var mayhem = Prototype<EntityPrototype>(prototypes, "MayhemBottle");
            Assert.That(mayhem.TryComp<MayhemBottleComponent>(out _, components), Is.True);

            var mercuryCore = Prototype<EntityPrototype>(prototypes, "SpiderMercuryCore");
            Assert.That(mercuryCore.TryComp<ResearchArtifactComponent>(out var mercuryResearch, components), Is.True);
            Assert.That(
                mercuryResearch!.Technologies,
                Does.Contain((ProtoId<TechnologyPrototype>) "MercuryORTApplications"));

            var etherDrinker = Prototype<EntityPrototype>(prototypes, "SpiderMercuryEtherDrinker");
            Assert.That(etherDrinker.TryComp<MercuryEtherDrinkerComponent>(out _, components), Is.True);

            var paradoxCanceller = Prototype<EntityPrototype>(prototypes, "SpiderMercuryParadoxCanceller");
            Assert.That(paradoxCanceller.TryComp<MercuryParadoxCancellerComponent>(out _, components), Is.True);

            var radiantShield = Prototype<EntityPrototype>(prototypes, "SpiderMercuryRadiantShield");
            Assert.That(radiantShield.TryComp<ReflectComponent>(out var reflect, components), Is.True);
            Assert.That(reflect!.ReflectProb, Is.EqualTo(0.6f));

            var radiantChest = Prototype<EntityPrototype>(prototypes, "ClothingModsuitChestplateMercuryRadiant");
            Assert.That(radiantChest.TryComp<PreventChasmFallingComponent>(out var chasmProtection, components), Is.True);
            Assert.That(chasmProtection!.DeleteOnUse, Is.False);

            var oni = Prototype<EntityPrototype>(prototypes, "MobChildishOni");
            Assert.That(oni.TryComp<MegafaunaPhaseDialogueComponent>(out var dialogue, components), Is.True);
            Assert.That(dialogue!.Phases.Keys, Is.EquivalentTo(new[] { 1, 2, 3, 4 }));
            Assert.That(dialogue.Phases[4].Lines, Has.Count.EqualTo(9));

            var oniSword = Prototype<EntityPrototype>(prototypes, "WeaponChildishOniBlade");
            Assert.That(oniSword.TryComp<ToolComponent>(out var swordTool, components), Is.True);
            Assert.That(swordTool!.Qualities, Does.Contain("Slicing"));
            Assert.That(swordTool.Qualities, Does.Contain("Sawing"));
            Assert.That(oniSword.TryComp<ScalpelComponent>(out _, components), Is.True);
            Assert.That(oniSword.TryComp<BoneSawComponent>(out _, components), Is.True);
            Assert.That(oniSword.TryComp<ItemActionGrantComponent>(out var swordAction, components), Is.True);
            Assert.That(swordAction, Is.Not.Null);
            Assert.That(oniSword.TryComp<ActionGrantComponent>(out var actionGrant, components), Is.True);
            Assert.That(actionGrant!.Actions, Does.Contain((EntProtoId) "ActionChildishOniBladeFlamingSlash"));

            foreach (var oniReward in new[]
                     {
                         "ClothingOuterChildishOniFloweryDress",
                         "ClothingHandsChildishOniCursed",
                         "ClothingShoesChildishOniCursed",
                         "ClothingBeltChildishOniObi",
                         "ClothingHeadChildishOniZukin",
                     })
            {
                Assert.That(HasPrototype<EntityPrototype>(prototypes, oniReward), Is.True);
            }

            var oniHands = Prototype<EntityPrototype>(prototypes, "ClothingHandsChildishOniCursed");
            Assert.That(oniHands.TryComp<ToolComponent>(out var handTool, components), Is.True);
            Assert.That(handTool!.Qualities, Does.Contain("Slicing"));
            Assert.That(oniHands.TryComp<ScalpelComponent>(out _, components), Is.True);

            var densityCore = Prototype<EntityPrototype>(prototypes, "OniDensityCore");
            Assert.That(densityCore.TryComp<DensityCoreComponent>(out _, components), Is.True);

            var cargoManipulator = Prototype<EntityPrototype>(prototypes, "CrateCargoDensityManipulator");
            Assert.That(cargoManipulator.TryComp<EntityStorageComponent>(out var cargoStorage, components), Is.True);
            Assert.That(cargoStorage!.Capacity, Is.EqualTo(60));
            Assert.That(cargoManipulator.TryComp<DensityCoreReceiverComponent>(out var cargoReceiver, components), Is.True);
            Assert.That(cargoReceiver!.CapacityBonus, Is.EqualTo(90));

            var trophyVault = Prototype<EntityPrototype>(prototypes, "CrateMegafaunaTrophyVault");
            Assert.That(trophyVault.TryComp<EntityStorageComponent>(out var trophyStorage, components), Is.True);
            Assert.That(trophyStorage!.Capacity, Is.EqualTo(8));

            foreach (var multibossReward in new[]
                     {
                         "ClothingOuterArmorDrakeMercuryAegis",
                         "ShieldColossusMercuryAnomalous",
                         "MedipenLegionBubblegumRegenerator",
                         "ClothingBeltAshFrostThermalRegulator",
                         "WeaponBloodDrunkMercuryPhaseCutter",
                         "OrganCompressedLegionCore",
                     })
            {
                var reward = Prototype<EntityPrototype>(prototypes, multibossReward);
                Assert.That(reward.TryComp<MegafaunaProcessedRewardComponent>(out _, components), Is.True);
                Assert.That(reward.TryComp<MegafaunaProvenanceComponent>(out var provenance, components), Is.True);
                Assert.That(provenance!.Grade, Is.EqualTo(MegafaunaProvenanceGrade.Processed));
            }

            var compressedCore = Prototype<EntityPrototype>(prototypes, "OrganCompressedLegionCore");
            Assert.That(compressedCore.TryComp<CompressedLegionCoreComponent>(out var compressed, components), Is.True);
            Assert.That(compressed!.Cooldown, Is.EqualTo(TimeSpan.FromSeconds(60)));
            Assert.That(compressedCore.TryComp<StabilizedLegionCoreOrganComponent>(out _, components), Is.False,
                "the combined core grants Density Surge but must not provide another automatic resurrection");

            foreach (var construction in new[]
                     {
                         "DrakeMercuryAegisConstruction",
                         "ColossusMercuryShieldConstruction",
                         "LegionBubblegumRegeneratorConstruction",
                         "AshFrostThermalRegulatorConstruction",
                         "BloodDrunkMercuryPhaseCutterConstruction",
                         "CompressedLegionCoreConstruction",
                     })
            {
                Assert.That(HasPrototype<ConstructionPrototype>(prototypes, construction), Is.True);
            }

            foreach (var bounty in new[]
                     {
                         "BountyMegafaunaRawSamples",
                         "BountyMegafaunaIntactSpecimen",
                         "BountyMegafaunaProcessedEquipment",
                     })
            {
                Assert.That(HasPrototype<CargoBountyPrototype>(prototypes, bounty), Is.True);
            }

            foreach (var construction in new[]
                     {
                         "SpiderMercuryCoreConstruction",
                         "SpiderMercuryEtherDrinkerConstruction",
                         "SpiderMercuryParadoxCancellerConstruction",
                         "SpiderMercuryRadiantShieldConstruction",
                         "SpiderMercuryRailgunConstruction",
                         "SpiderMercuryRadiantModsuitConstruction",
                     })
            {
                Assert.That(HasPrototype<ConstructionPrototype>(prototypes, construction), Is.True);
            }

            foreach (var recipeId in MegafaunaLatheRecipes)
            {
                var recipe = Prototype<LatheRecipePrototype>(prototypes, recipeId);
                Assert.That(recipe.Result, Is.Not.Null, $"{recipeId} must produce an entity");
                Assert.That(HasPrototype<EntityPrototype>(prototypes, recipe.Result!.Value.Id), Is.True,
                    $"{recipeId} points to the missing result {recipe.Result.Value.Id}");
                Assert.That(recipe.Materials, Is.Not.Empty, $"{recipeId} must consume materials");
                Assert.That(recipe.Materials.Values.All(amount => amount > 0), Is.True,
                    $"{recipeId} contains a zero or negative material cost");
            }

            Assert.That(HasPrototype<TechnologyPrototype>(prototypes, "MegafaunaMaterialEngineering"), Is.True);
            Assert.That(HasPrototype<TechnologyPrototype>(prototypes, "MegafaunaBiomechanics"), Is.True);
            Assert.That(HasPrototype<TechnologyPrototype>(prototypes, "MercuryORTApplications"), Is.True);
            Assert.That(HasPrototype<LatheRecipePrototype>(prototypes, "ClothingOuterArmorDragonAdvanced"), Is.True);
            Assert.That(HasPrototype<LatheRecipePrototype>(prototypes, "GygaxArmorMegafauna"), Is.True);
            foreach (var reagent in new[]
                     {
                         "ExothermicBlood",
                         "DemonicBlood",
                         "MercuryVenom",
                         "OniGastricEnzymes",
                         "ExothermicHemostate",
                         "DemonicCytostimulant",
                         "MercurialNeurochelate",
                         "OniEnzymeFiltrate",
                         "ExothermicRegenerativeSuspension",
                         "ThermomercurialPerfusate",
                         "ExothermicEnzymeBuffer",
                         "DemonicNeuroregenerator",
                         "DemonicEnzymeCoagulant",
                         "MercurialEnzymeAntitoxin",
                         "MegafaunaPolyserum",
                     })
            {
                Assert.That(HasPrototype<ReagentPrototype>(prototypes, reagent), Is.True,
                    $"missing megafauna chemistry reagent {reagent}");
            }

            foreach (var reaction in new[]
                     {
                         "ExothermicHemostate",
                         "DemonicCytostimulant",
                         "MercurialNeurochelate",
                         "OniEnzymeFiltrate",
                         "ExothermicRegenerativeSuspension",
                         "ThermomercurialPerfusate",
                         "ExothermicEnzymeBuffer",
                         "DemonicNeuroregenerator",
                         "DemonicEnzymeCoagulant",
                         "MercurialEnzymeAntitoxin",
                         "MegafaunaPolyserum",
                     })
            {
                Assert.That(HasPrototype<ReactionPrototype>(prototypes, reaction), Is.True,
                    $"missing megafauna chemistry reaction {reaction}");
            }

            var polyserum = Prototype<ReagentPrototype>(prototypes, "MegafaunaPolyserum");
            Assert.That(polyserum.WorksOnTheDead, Is.True,
                "Megafauna Polyserum must continue metabolizing in dead bodies");
        });
    }

    [Test]
    public async Task ResidualCarcassesContainTheirExtractableBossFluids()
    {
        var server = Pair.Server;
        var entities = server.ResolveDependency<IEntityManager>();
        var solutions = server.System<SharedSolutionContainerSystem>();
        var map = await Pair.CreateTestMap();
        var spawned = new List<(EntityUid Entity, string Reagent, FixedPoint2 Quantity)>();

        var carcasses = new (string Prototype, string Reagent, FixedPoint2 Quantity)[]
        {
            ("AshDrakeResidualCarcass", "ExothermicBlood", 240),
            ("BubblegumResidualCarcass", "DemonicBlood", 300),
            ("MercurySpiderResidualCarcass", "MercuryVenom", 180),
            ("ChildishOniResidualCarcass", "OniGastricEnzymes", 240),
        };

        await server.WaitPost(() =>
        {
            foreach (var carcass in carcasses)
            {
                var entity = entities.SpawnEntity(carcass.Prototype, map.GridCoords);
                spawned.Add((entity, carcass.Reagent, carcass.Quantity));
            }
        });
        await server.WaitRunTicks(2);

        await server.WaitAssertion(() =>
        {
            foreach (var (entity, reagent, quantity) in spawned)
            {
                Assert.That(entities.HasComponent<DrawableSolutionComponent>(entity), Is.True,
                    $"{entities.ToPrettyString(entity)} cannot be drawn from with a syringe");
                Assert.That(solutions.TryGetSolution(entity, "carcass", out _, out var solution), Is.True,
                    $"{entities.ToPrettyString(entity)} has no carcass solution");
                Assert.That(solution.GetTotalPrototypeQuantity(reagent), Is.EqualTo(quantity));
            }
        });

        await server.WaitPost(() =>
        {
            foreach (var (entity, _, _) in spawned)
                entities.QueueDeleteEntity(entity);
        });
    }

    [Test]
    public async Task EveryMegafaunaRewardItemSpawnsCleanly()
    {
        var server = Pair.Server;
        var entities = server.ResolveDependency<IEntityManager>();
        var prototypes = server.ResolveDependency<IPrototypeManager>();
        var components = server.ResolveDependency<IComponentFactory>();
        var map = await Pair.CreateTestMap();
        var spawned = new List<EntityUid>();
        var rewards = new HashSet<string>(LocalizedRewardItems);

        foreach (var prototype in prototypes.EnumeratePrototypes<EntityPrototype>())
        {
            if (prototype.TryComp<MegafaunaProvenanceComponent>(out _, components) &&
                prototype.TryComp<ItemComponent>(out _, components))
                rewards.Add(prototype.ID);
        }

        await server.WaitAssertion(() =>
        {
            foreach (var prototype in rewards)
            {
                var entity = entities.SpawnEntity(prototype, map.GridCoords);
                Assert.That(entities.Deleted(entity), Is.False, $"{prototype} was deleted while spawning");
                spawned.Add(entity);
            }
        });

        // Let component startup, appearance replication and client visualizers run.
        await server.WaitRunTicks(5);
        await Pair.Client.WaitRunTicks(5);

        await server.WaitPost(() =>
        {
            foreach (var entity in spawned)
                entities.DeleteEntity(entity);
        });
        await server.WaitRunTicks(2);
    }

    [Test]
    public async Task CleavingSawPublishesItsOpenWorldAndInHandState()
    {
        var server = Pair.Server;
        var entities = server.ResolveDependency<IEntityManager>();
        var systems = server.ResolveDependency<IEntitySystemManager>();
        var appearance = systems.GetEntitySystem<SharedAppearanceSystem>();
        var map = await Pair.CreateTestMap();

        EntityUid user = default;
        EntityUid saw = default;
        await server.WaitPost(() =>
        {
            user = entities.SpawnEntity("MercuryRelicTestUser", map.GridCoords);
            saw = entities.SpawnEntity("WeaponCleavingSaw", map.GridCoords);

            var use = new UseInHandEvent(user);
            entities.EventBus.RaiseLocalEvent(saw, use);
            Assert.That(use.Handled, Is.True);
        });
        await server.WaitRunTicks(1);

        await server.WaitAssertion(() =>
        {
            Assert.That(entities.GetComponent<CleavingSawComponent>(saw).Open, Is.True);
            Assert.That(entities.GetComponent<ItemComponent>(saw).HeldPrefix, Is.EqualTo("open"));
            Assert.That(appearance.TryGetData(saw, CleavingSawVisuals.Open, out bool open), Is.True);
            Assert.That(open, Is.True);
        });
    }

    [Test]
    public async Task MercuryRelicsDischargeAndRewindWithoutSnapshottingInventory()
    {
        var server = Pair.Server;
        var entities = server.ResolveDependency<IEntityManager>();
        var systems = server.ResolveDependency<IEntitySystemManager>();
        var prototypes = server.ResolveDependency<IPrototypeManager>();
        var damage = systems.GetEntitySystem<DamageableSystem>();
        var transform = systems.GetEntitySystem<SharedTransformSystem>();
        var delaySystem = systems.GetEntitySystem<UseDelaySystem>();
        var map = await Pair.CreateTestMap();

        EntityUid user = default;
        EntityUid canceller = default;
        EntityUid drinker = default;
        Vector2 originalPosition = default;

        await server.WaitPost(() =>
        {
            user = entities.SpawnEntity("MercuryRelicTestUser", map.GridCoords);
            canceller = entities.SpawnEntity("SpiderMercuryParadoxCanceller", map.GridCoords);
            drinker = entities.SpawnEntity("SpiderMercuryEtherDrinker", map.GridCoords);
            transform.SetCoordinates(canceller, new EntityCoordinates(user, Vector2.Zero));

            var blunt = Prototype<DamageTypePrototype>(prototypes, "Blunt");
            damage.TryChangeDamage(
                user,
                new DamageSpecifier(blunt, 10),
                ignoreResistances: true,
                ignoreBlockers: true,
                canMiss: false);
            entities.GetComponent<MercuryParadoxCancellerComponent>(canceller).RewindTime = TimeSpan.FromSeconds(0.1);
            entities.GetComponent<MercuryEtherDrinkerComponent>(drinker).MaxStrikes = 1;
            originalPosition = transform.GetWorldPosition(user);
        });
        await server.WaitRunTicks(1);

        await server.WaitPost(() =>
        {
            var paradoxUse = new UseInHandEvent(user);
            entities.EventBus.RaiseLocalEvent(canceller, paradoxUse);
            Assert.That(paradoxUse.Handled, Is.True);

            transform.SetWorldPosition(user, originalPosition + new Vector2(5f, 0f));
            var blunt = Prototype<DamageTypePrototype>(prototypes, "Blunt");
            damage.TryChangeDamage(
                user,
                new DamageSpecifier(blunt, 20),
                ignoreResistances: true,
                ignoreBlockers: true,
                canMiss: false);

            var etherUse = new UseInHandEvent(user);
            entities.EventBus.RaiseLocalEvent(drinker, etherUse);
            Assert.That(etherUse.Handled, Is.True);
            Assert.That(delaySystem.IsDelayed((drinker, entities.GetComponent<UseDelayComponent>(drinker))), Is.True);

            var warnings = 0;
            var query = entities.EntityQueryEnumerator<MetaDataComponent>();
            while (query.MoveNext(out _, out var metadata))
            {
                if (metadata.EntityPrototype?.ID == "LightningCrackleNeutral")
                    warnings++;
            }
            Assert.That(warnings, Is.EqualTo(2), "a full capacitor doubles its one-strike test discharge");
        });

        await server.WaitRunTicks(10);

        await server.WaitAssertion(() =>
        {
            Assert.That(transform.GetWorldPosition(user), Is.EqualTo(originalPosition));
            Assert.That(damage.GetTotalDamage(user), Is.EqualTo(FixedPoint2.New(10)));
            Assert.That(entities.GetComponent<MercuryParadoxCancellerComponent>(canceller).RewindAt, Is.Null);
        });
    }
}

/// <summary>
/// Exercises the real interaction and ToolSystem DoAfter path. Prototype-only assertions above catch wiring
/// regressions, while this fixture catches ordering, cancellation, wrong-tool and duplicate-loot regressions.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[TestOf(typeof(MegafaunaHarvestableComponent))]
public sealed class MegafaunaHarvestInteractionTest : InteractionTest
{
    protected override string PlayerPrototype => "MegafaunaHarvestInteractionPlayer";

    [SidedDependency(Side.Server)] private readonly SharedContainerSystem _containers = default!;
    [SidedDependency(Side.Server)] private readonly DamageableSystem _damage = default!;
    [SidedDependency(Side.Server)] private readonly SharedEntityEffectsSystem _entityEffects = default!;
    [SidedDependency(Side.Server)] private readonly ItemSlotsSystem _itemSlots = default!;
    [SidedDependency(Side.Server)] private readonly SharedPhysicsSystem _physics = default!;

    private static readonly (string Recipe, string Result, (string Prototype, int Quantity)[] Ingredients)[]
        MegafaunaItemConstructionCases =
    {
        ("DrakeRemainsConstruction", "DrakeRemains", [("DragonBone", 10), ("DragonHide", 5)]),
        ("DragonArmorConstruction", "ClothingOuterArmorDragon", [("DrakeRemains", 1), ("MegafaunaSinew", 2)]),
        ("LegionServitorCultureConstruction", "LegionServitorCulture", [("LegionSkull", 3)]),
        ("GodslayerArmorConstruction", "ClothingOuterArmorGodslayer",
            [("IceEnergyCrystal", 1), ("DragonBone", 8), ("NecroAlloy", 6), ("Brass", 5)]),
        ("DrakeMercuryAegisConstruction", "ClothingOuterArmorDrakeMercuryAegis",
            [("DragonHide", 5), ("DragonWingMembrane", 2), ("MirroredChitin", 5), ("MercurySilk", 3)]),
        ("ColossusMercuryShieldConstruction", "ShieldColossusMercuryAnomalous",
            [("NecroAlloy", 5), ("SpiderMercuryAlloy", 3)]),
        ("LegionBubblegumRegeneratorConstruction", "MedipenLegionBubblegumRegenerator",
            [("OrganStabilizedLegionCore", 1), ("ChemistryBottleDemonicBlood", 1)]),
        ("AshFrostThermalRegulatorConstruction", "ClothingBeltAshFrostThermalRegulator",
            [("IceEnergyCrystal", 1), ("DragonWingMembrane", 3), ("DragonBone", 3)]),
        ("BloodDrunkMercuryPhaseCutterConstruction", "WeaponBloodDrunkMercuryPhaseCutter",
            [("WeaponPlasmaCutterOverclocked", 1), ("SpiderMercuryAlloy", 4)]),
        ("CompressedLegionCoreConstruction", "OrganCompressedLegionCore",
            [("OrganStabilizedLegionCore", 1), ("OniDensityCore", 1)]),
        ("SpiderMercuryAlloyConstruction", "MaterialSpiderMercuryAlloy1",
            [("SpiderMercuryKeratin", 4), ("Diamond", 1)]),
        ("SpiderMercuryRailgunConstruction", "WeaponSpiderMercuryRailgun",
            [("SpiderMercuryCore", 1), ("SpiderMercuryAlloy", 1)]),
        ("SpiderMercuryCoreConstruction", "SpiderMercuryCore", [("SpiderMercuryAlloy", 3)]),
        ("SpiderMercuryEtherDrinkerConstruction", "SpiderMercuryEtherDrinker", [("SpiderMercuryKeratin", 5)]),
        ("SpiderMercuryParadoxCancellerConstruction", "SpiderMercuryParadoxCanceller",
            [("SpiderMercuryKeratin", 4)]),
        ("SpiderMercuryRadiantShieldConstruction", "SpiderMercuryRadiantShield",
            [("SpiderMercuryKeratin", 2), ("SpiderMercuryAlloy", 1)]),
        ("SpiderMercuryRadiantModsuitConstruction", "ClothingModsuitMercuryRadiant",
            [("SpiderMercuryCore", 1), ("SpiderMercuryAlloy", 2)]),
    };

    [TestPrototypes]
    private const string TestPrototypes = """
- type: entity
  id: MegafaunaHarvestInteractionCarcass
  name: test megafauna carcass
  components:
  - type: MobState
    currentState: Dead
  - type: MegafaunaHarvestable
    stages:
    - name: megafauna-harvest-stage-drake-hide
      toolQualities: [ Slicing ]
      duration: 5
      loot: !type:AllSelector
        children:
        - id: MegafaunaHarvestInteractionTissue
    - name: megafauna-harvest-stage-drake-bones
      toolQualities: [ Sawing ]
      duration: 5
      loot: !type:AllSelector
        children:
        - id: MegafaunaHarvestInteractionBone

- type: entity
  id: MegafaunaHarvestInteractionTissue

- type: entity
  id: MegafaunaHarvestInteractionBone

- type: entity
  parent: BaseItem
  id: MegafaunaHarvestInteractionSlicingTool
  components:
  - type: Tool
    qualities: [ Slicing ]

- type: entity
  parent: BaseItem
  id: MegafaunaHarvestInteractionSawingTool
  components:
  - type: Tool
    qualities: [ Sawing ]

- type: entity
  parent: InteractionTestMob
  id: MegafaunaLegionTrophyFauna
  components:
  - type: Damageable
  - type: Injurable
    damageContainer: Biological
  - type: MobState
  - type: Fauna
  - type: NpcFactionMember
    factions: [ SimpleHostile ]
  - type: MobThresholds
    thresholds:
      0: Alive
      20: Dead

- type: entity
  parent: TrophyLavalandLegionSkull
  id: MegafaunaLegionTrophyTest
  components:
  - type: CrusherLegionSkullUpgrade
    raiseCooldown: 0

- type: entity
  parent: InteractionTestMob
  id: MegafaunaHarvestInteractionPlayer
  components:
  - type: HumanoidProfile
  - type: MobState

- type: entity
  parent: InteractionTestMob
  id: MegafaunaDelayedRevivalMob
  components:
  - type: Damageable
  - type: Injurable
    damageContainer: Biological
  - type: MobState
  - type: MobThresholds
    thresholds:
      0: Alive
      10: Critical
      20: Dead

- type: entity
  id: MegafaunaDelayedLegionCore
  components:
  - type: StabilizedLegionCoreOrgan
    revivalDelay: 0.1

- type: entity
  id: MegafaunaDelayedGodslayerArmor
  components:
  - type: GodslayerArmor
    revivalDelay: 0.1
    cooldown: 1
""";

    [Test]
    public async Task HarvestIsOrderedCancelableAndExactlyOnce()
    {
        await SpawnTarget("MegafaunaHarvestInteractionCarcass");
        var carcass = STarget!.Value;
        var harvest = SEntMan.GetComponent<MegafaunaHarvestableComponent>(carcass);

        Assert.That(SEntMan.GetComponent<MobStateComponent>(carcass).CurrentState, Is.EqualTo(Content.Shared.Mobs.MobState.Dead));
        Assert.That(harvest.CurrentStage, Is.Zero);

        // A tool without the required quality must not start a harvest.
        await InteractUsing("Crowbar", awaitDoAfters: false);
        Assert.That(ActiveDoAfters, Is.Empty);
        Assert.That(harvest.CurrentStage, Is.Zero);
        Assert.That(CountPrototype("MegafaunaHarvestInteractionTissue"), Is.Zero);

        // Cancelling a valid harvest must not advance the stage or spawn loot.
        await InteractUsing("MegafaunaHarvestInteractionSlicingTool", awaitDoAfters: false);
        await CancelDoAfters();
        Assert.That(harvest.CurrentStage, Is.Zero);
        Assert.That(CountPrototype("MegafaunaHarvestInteractionTissue"), Is.Zero);

        await InteractUsing("MegafaunaHarvestInteractionSlicingTool");
        Assert.That(harvest.CurrentStage, Is.EqualTo(1));
        Assert.That(CountPrototype("MegafaunaHarvestInteractionTissue"), Is.EqualTo(1));
        Assert.That(CountPrototype("MegafaunaHarvestInteractionBone"), Is.Zero);

        // The previous stage's tool cannot be reused to skip the required bone saw.
        await InteractUsing("MegafaunaHarvestInteractionSlicingTool", awaitDoAfters: false);
        Assert.That(ActiveDoAfters, Is.Empty);
        Assert.That(harvest.CurrentStage, Is.EqualTo(1));

        await InteractUsing("MegafaunaHarvestInteractionSawingTool");
        await RunTicks(2);
        Assert.That(SEntMan.Deleted(carcass), Is.True);
        Assert.That(CountPrototype("MegafaunaHarvestInteractionTissue"), Is.EqualTo(1));
        Assert.That(CountPrototype("MegafaunaHarvestInteractionBone"), Is.EqualTo(1));
    }

    [Test]
    public async Task DeadMercurySpiderCannotContinueItsTransportDash()
    {
        await SpawnTarget("MobSpiderMercuryUltimate");
        var spider = STarget!.Value;
        var transport = SEntMan.GetComponent<ORTTransportMatterComponent>(spider);
        var baselineBeams = CountPrototype("ORTBeamWarning");

        await Server.WaitPost(() =>
        {
            transport.Dashing = true;
            transport.MoveTarget = Transform.GetWorldPosition(spider) + new Vector2(8f, 0f);
            transport.NextDashDamage = TimeSpan.Zero;
            _physics.SetLinearVelocity(spider, new Vector2(10f, 0f));
            SEntMan.RemoveComponent<MegafaunaGodmodeComponent>(spider);
            _damage.TryChangeDamage(
                spider,
                new DamageSpecifier { DamageDict = { ["Blunt"] = 5000 } },
                ignoreResistances: true,
                origin: SPlayer);
        });
        await RunTicks(5);

        Assert.That(SEntMan.GetComponent<MobStateComponent>(spider).CurrentState, Is.EqualTo(Content.Shared.Mobs.MobState.Dead));
        Assert.That(transport.Dashing, Is.False);
        Assert.That(transport.MoveTarget, Is.Null);
        Assert.That(SEntMan.GetComponent<PhysicsComponent>(spider).LinearVelocity, Is.EqualTo(Vector2.Zero));
        Assert.That(CountPrototype("ORTBeamWarning"), Is.EqualTo(baselineBeams));
    }

    [Test]
    public async Task LegionFragmentsResolveAsOneEncounterWithoutIntermediateLoot()
    {
        await SpawnTarget("LavalandBossMegaLegion");
        var root = STarget!.Value;

        await Server.WaitPost(() => _damage.TryChangeDamage(
            root,
            new DamageSpecifier { DamageDict = { ["Blunt"] = 5000 } },
            ignoreResistances: true,
            origin: SPlayer));
        await RunTicks(5);

        var encounter = SEntMan.GetComponent<LegionEncounterComponent>(root);
        Assert.That(encounter.Completed, Is.False);
        Assert.That(SEntMan.GetComponent<PhysicsComponent>(root).CanCollide, Is.False);
        Assert.That(CountLivingLegionSplits(), Is.EqualTo(3));
        AssertLegionHasNoIntermediateLoot();

        // Non-kinetic damage no longer invalidates cooperative qualification.
        var crowbar = ToServer(await Spawn("Crowbar"));
        var firstSplit = FirstLivingLegionSplit();
        await Server.WaitPost(() =>
        {
            var coordinates = SEntMan.GetComponent<TransformComponent>(firstSplit).Coordinates;
            var attacked = new AttackedEvent(crowbar, SPlayer, coordinates);
            SEntMan.EventBus.RaiseLocalEvent(firstSplit, attacked);
        });
        var rootLoot = SEntMan.GetComponent<SpawnLootOnDeathComponent>(root);
        Assert.That(rootLoot.QualifiedDamage, Is.EqualTo(FixedPoint2.Zero));

        // The complete encounter owns one shared contribution pool. This test
        // focuses on split/loot lifecycle; mark the pool qualified before the
        // final generation so completion can verify the special table once.
        await Server.WaitPost(() => rootLoot.QualifiedDamage = FixedPoint2.New(5000));

        // The apparent central corpse is not harvestable while its fragments live.
        await InteractUsing("MegafaunaHarvestInteractionSlicingTool", awaitDoAfters: false);
        Assert.That(ActiveDoAfters, Is.Empty);
        Assert.That(SEntMan.GetComponent<MegafaunaHarvestableComponent>(root).CurrentStage, Is.Zero);

        await KillCurrentLegionGeneration();
        Assert.That(CountLivingLegionSplits(), Is.EqualTo(6));
        AssertLegionHasNoIntermediateLoot();

        await KillCurrentLegionGeneration();
        Assert.That(CountLivingLegionSplits(), Is.EqualTo(12));
        AssertLegionHasNoIntermediateLoot();

        await KillCurrentLegionGeneration();
        Assert.That(CountLivingLegionSplits(), Is.Zero);
        Assert.That(encounter.Completed, Is.True);
        Assert.That(CountPrototype("LavalandCrateNecropolisFilled"), Is.EqualTo(1));
        Assert.That(CountPrototype("TrophyLavalandLegionSkull"), Is.EqualTo(1));
        // The final filled Necropolis chest may legitimately contain the rare
        // Legion-skull-and-bones bundle. The checks after every generation above
        // are what guarantee that no fragment emitted intermediate floor loot.
        Assert.That(CountPrototype("LegionCore"), Is.Zero);
        Assert.That(CountPrototype("CrowbarRed"), Is.Zero);
    }

    [Test]
    public async Task StabilizingSerumConvertsRawLegionCoreIntoPermanentOrgan()
    {
        var rawCore = ToServer(await Spawn("LegionCore"));

        await Server.WaitPost(() =>
            _entityEffects.RaiseEffectEvent(rawCore, new StabilizeLegionCore(), 1f, null, predicted: false));
        await RunTicks(2);

        Assert.That(SEntMan.Deleted(rawCore), Is.True);
        Assert.That(CountPrototype("OrganStabilizedLegionCore"), Is.EqualTo(1));
    }

    [Test]
    public async Task LegionCoreWaitsBeforeRevivingAndCannotTriggerTwice()
    {
        var patient = ToServer(await Spawn("MegafaunaDelayedRevivalMob"));
        var organ = ToServer(await Spawn("MegafaunaDelayedLegionCore"));

        await Server.WaitPost(() =>
        {
            var carrier = SEntMan.EnsureComponent<LegionCoreCarrierComponent>(patient);
            carrier.Organ = organ;
            _damage.TryChangeDamage(
                patient,
                new DamageSpecifier { DamageDict = { ["Blunt"] = 15 } },
                ignoreResistances: true,
                origin: SPlayer);
        });
        await RunTicks(2);

        Assert.That(SEntMan.GetComponent<MobStateComponent>(patient).CurrentState,
            Is.EqualTo(Content.Shared.Mobs.MobState.Critical));
        Assert.That(SEntMan.GetComponent<LegionCoreCarrierComponent>(patient).RevivalPending, Is.True);

        await Server.WaitPost(() => _damage.TryChangeDamage(
            patient,
            new DamageSpecifier { DamageDict = { ["Blunt"] = 15 } },
            ignoreResistances: true,
            origin: SPlayer));
        await RunTicks(2);

        // More than the original critical-state deadline has elapsed, but less than
        // one full delay has elapsed since death. The death event must restart it.
        Assert.That(SEntMan.GetComponent<MobStateComponent>(patient).CurrentState,
            Is.EqualTo(Content.Shared.Mobs.MobState.Dead));
        Assert.That(SEntMan.GetComponent<LegionCoreCarrierComponent>(patient).RevivalPending, Is.True);
        Assert.That(SEntMan.Deleted(organ), Is.False);

        await RunTicks(4);

        Assert.That(SEntMan.GetComponent<MobStateComponent>(patient).CurrentState,
            Is.EqualTo(Content.Shared.Mobs.MobState.Alive));
        Assert.That(SEntMan.HasComponent<LegionCoreCarrierComponent>(patient), Is.False);
        Assert.That(SEntMan.Deleted(organ), Is.True);
    }

    [Test]
    public async Task GodslayerWaitsBeforeRevivingItsWearer()
    {
        var wearer = ToServer(await Spawn("MegafaunaDelayedRevivalMob"));
        var armor = ToServer(await Spawn("MegafaunaDelayedGodslayerArmor"));

        await Server.WaitPost(() =>
        {
            SEntMan.GetComponent<GodslayerArmorComponent>(armor).Wearer = wearer;
            var carrier = SEntMan.EnsureComponent<GodslayerCarrierComponent>(wearer);
            carrier.Armor = armor;
            _damage.TryChangeDamage(
                wearer,
                new DamageSpecifier { DamageDict = { ["Blunt"] = 15 } },
                ignoreResistances: true,
                origin: SPlayer);
        });
        await RunTicks(2);

        Assert.That(SEntMan.GetComponent<MobStateComponent>(wearer).CurrentState,
            Is.EqualTo(Content.Shared.Mobs.MobState.Critical));

        await Server.WaitPost(() => _damage.TryChangeDamage(
            wearer,
            new DamageSpecifier { DamageDict = { ["Blunt"] = 15 } },
            ignoreResistances: true,
            origin: SPlayer));
        await RunTicks(2);

        Assert.That(SEntMan.GetComponent<MobStateComponent>(wearer).CurrentState,
            Is.EqualTo(Content.Shared.Mobs.MobState.Dead));
        Assert.That(SEntMan.GetComponent<GodslayerCarrierComponent>(wearer).RevivalPending, Is.True);

        await RunTicks(4);

        Assert.That(SEntMan.GetComponent<MobStateComponent>(wearer).CurrentState,
            Is.EqualTo(Content.Shared.Mobs.MobState.Alive));
        Assert.That(SEntMan.GetComponent<GodslayerCarrierComponent>(wearer).RevivalPending, Is.False);
        Assert.That(SEntMan.GetComponent<GodslayerArmorComponent>(armor).NextRevival, Is.GreaterThan(TimeSpan.Zero));
    }

    [Test]
    public async Task CrusherAcceptsDistinctTrophiesWithinDedicatedCapacity()
    {
        await SpawnTarget("WeaponCrusher");
        var crusher = STarget!.Value;
        var slots = SEntMan.GetComponent<ItemSlotsComponent>(crusher);

        await InteractUsing("TrophyLavalandAshDrakeSpike");
        var ash = slots.Slots["trophy_slot_1"].Item;
        Assert.That(ash, Is.Not.Null);
        Assert.That(SEntMan.HasComponent<CrusherTrophyComponent>(ash!.Value), Is.True);

        await InteractUsing("TrophyLavalandBubblegumDemonClaws");
        var bubblegum = slots.Slots["trophy_slot_2"].Item;
        Assert.That(bubblegum, Is.Not.Null);
        Assert.That(bubblegum, Is.Not.EqualTo(ash));

        // Duplicate identities and combinations above the 100 point trophy
        // budget are rejected before a DoAfter can consume the item.
        await InteractUsing("TrophyLavalandAshDrakeSpike", awaitDoAfters: false);
        Assert.That(ActiveDoAfters, Is.Empty);
        await InteractUsing("TrophyLavalandColossusBlasterTubes", awaitDoAfters: false);
        Assert.That(ActiveDoAfters, Is.Empty);
        Assert.That(slots.Slots["trophy_slot_1"].Item, Is.EqualTo(ash));
        Assert.That(slots.Slots["trophy_slot_2"].Item, Is.EqualTo(bubblegum));
        Assert.That(slots.Slots["trophy_slot_3"].Item, Is.Null);
        Assert.That(slots.Slots["upgrade_slot_blade"].Item, Is.Null);
        Assert.That(slots.Slots["upgrade_slot_handle"].Item, Is.Null);
    }

    [Test]
    public async Task PortablePkaUsesTheSameIndependentTrophyCapacity()
    {
        await SpawnTarget("WeaponProtoKineticAccelerator");
        var slots = SEntMan.GetComponent<ItemSlotsComponent>(STarget!.Value);

        await InteractUsing("TrophyLavalandAshDrakeSpike");
        await InteractUsing("TrophyLavalandBDMEye");

        Assert.That(slots.Slots["trophy_slot_1"].Item, Is.Not.Null);
        Assert.That(slots.Slots["trophy_slot_2"].Item, Is.Not.Null);
        Assert.That(slots.Slots["upgrade_slot_1"].Item, Is.Null,
            "boss trophies must not consume normal PKA modkit slots");
    }

    [Test]
    public async Task DemonClawsMultipliesEveryKineticPlatformSpread()
    {
        var cases = new[]
        {
            (Gun: "WeaponProtoKineticAccelerator", Expected: 3),
            (Gun: "WeaponProtoKineticShotgun", Expected: 12),
            (Gun: "WeaponProtoKineticShockwave", Expected: 24),
        };

        foreach (var testCase in cases)
        {
            var (gun, projectiles) = await FireWithUpgrade(
                testCase.Gun,
                "TrophyLavalandBubblegumDemonClaws",
                "trophy_slot_1");

            Assert.That(projectiles, Has.Count.EqualTo(testCase.Expected), testCase.Gun);
            foreach (var projectile in projectiles)
            {
                Assert.That(SEntMan.TryGetComponent<KineticTrophyProjectileComponent>(projectile, out var trophy),
                    Is.True,
                    $"{testCase.Gun} produced an unmodified pellet");
                Assert.That(trophy!.DemonClawsTrophy, Is.Not.Null);
            }

            // The fan is calculated without modifying the ammunition entity in
            // flight. Ordinary PKA bolts must therefore remain non-spreading.
            if (testCase.Gun == "WeaponProtoKineticAccelerator")
            {
                Assert.That(projectiles.All(projectile =>
                    !SEntMan.HasComponent<ProjectileSpreadComponent>(projectile)), Is.True);
            }
        }
    }

    [Test]
    public async Task AshDrakeAndDemonClawsApplyToEveryShotgunPellet()
    {
        var gun = ToServer(await Spawn("WeaponProtoKineticShotgun"));
        var ash = ToServer(await Spawn("TrophyLavalandAshDrakeSpike"));
        var demon = ToServer(await Spawn("TrophyLavalandBubblegumDemonClaws"));
        List<EntityUid> projectiles = [];

        await Server.WaitAssertion(() =>
        {
            Assert.That(_itemSlots.TryInsert(gun, "trophy_slot_1", ash, null), Is.True);
            Assert.That(_itemSlots.TryInsert(gun, "trophy_slot_2", demon, null), Is.True);
            FireGunImmediately(gun);
            projectiles = ProjectilesFiredBy(gun);
        });

        Assert.That(projectiles, Has.Count.EqualTo(12));
        foreach (var uid in projectiles)
        {
            var projectile = SEntMan.GetComponent<ProjectileComponent>(uid);
            Assert.That(projectile.Damage.DamageDict.TryGetValue("Heat", out var heat), Is.True);
            Assert.That(heat, Is.GreaterThan(FixedPoint2.Zero));

            var trophies = SEntMan.GetComponent<KineticTrophyProjectileComponent>(uid);
            Assert.That(trophies.AshDrakeTrophy, Is.EqualTo(ash));
            Assert.That(trophies.DemonClawsTrophy, Is.EqualTo(demon));
        }
    }

    [Test]
    public async Task TrophyProjectileDefersStaticStructureCollisionToServer()
    {
        // A persistent physics body stands in for the projectile here. The event
        // contract depends only on physics fixtures and the trophy marker; using a
        // short-lived real bolt would despawn before the client assertion at the
        // integration runner's one-tick-per-second test rate.
        var projectile = ToServer(await Spawn("MobMouse"));
        var window = ToServer(await Spawn("Window"));
        await Server.WaitPost(() => SEntMan.EnsureComponent<KineticTrophyProjectileComponent>(projectile));
        await RunTicks(2);

        await Client.WaitAssertion(() =>
        {
            var clientProjectile = ToClient(projectile);
            var clientWindow = ToClient(window);
            // Predicted shots assemble this marker locally before entering the
            // physics world, mirroring SharedGunSystem's client firing path.
            CEntMan.EnsureComponent<KineticTrophyProjectileComponent>(clientProjectile);

            var projectilePhysics = CEntMan.GetComponent<PhysicsComponent>(clientProjectile);
            var projectileFixture = CEntMan.GetComponent<FixturesComponent>(clientProjectile)
                .Fixtures.Values.First(fixture => fixture.Hard);
            var windowPhysics = CEntMan.GetComponent<PhysicsComponent>(clientWindow);
            var windowFixture = CEntMan.GetComponent<FixturesComponent>(clientWindow)
                .Fixtures.Values.First(fixture => fixture.Hard);
            var collision = new PreventCollideEvent(
                clientProjectile,
                clientWindow,
                projectilePhysics,
                windowPhysics,
                projectileFixture,
                windowFixture);

            CEntMan.EventBus.RaiseLocalEvent(clientProjectile, ref collision);
            Assert.That(collision.Cancelled, Is.True,
                "the client predicted a trophy-pellet contact with a static window");
        });

        await Server.WaitAssertion(() =>
        {
            var projectilePhysics = SEntMan.GetComponent<PhysicsComponent>(projectile);
            var projectileFixture = SEntMan.GetComponent<FixturesComponent>(projectile)
                .Fixtures.Values.First(fixture => fixture.Hard);
            var windowPhysics = SEntMan.GetComponent<PhysicsComponent>(window);
            var windowFixture = SEntMan.GetComponent<FixturesComponent>(window)
                .Fixtures.Values.First(fixture => fixture.Hard);
            var collision = new PreventCollideEvent(
                projectile,
                window,
                projectilePhysics,
                windowPhysics,
                projectileFixture,
                windowFixture);

            SEntMan.EventBus.RaiseLocalEvent(projectile, ref collision);
            Assert.That(collision.Cancelled, Is.False,
                "the server must remain authoritative for trophy-pellet structure hits");
        });
    }

    [Test]
    public async Task EveryAddedProjectileModkitPreparesItsProjectile()
    {
        var cases = new[]
        {
            "PKAUpgradeMiningAoE",
            "PKAUpgradeOffensiveAoE",
            "PKAUpgradeHybridAoE",
            "PKAUpgradeHumanPassthrough",
            "PKAUpgradeDronePassthrough",
            "PKAUpgradeRapidRepeater",
            "PKAUpgradeResonatorBlast",
            "PKAUpgradeDeathSyphon",
            "PKAUpgradeTracerAmber",
        };

        foreach (var modkit in cases)
        {
            var (_, projectiles) = await FireWithUpgrade(
                "WeaponProtoKineticAccelerator",
                modkit,
                "upgrade_slot_1");
            Assert.That(projectiles, Has.Count.EqualTo(1), modkit);
            var projectile = projectiles[0];

            var prepared = modkit switch
            {
                "PKAUpgradeMiningAoE" =>
                    SEntMan.HasComponent<KineticMiningAreaProjectileComponent>(projectile),
                "PKAUpgradeOffensiveAoE" =>
                    SEntMan.HasComponent<ProjectileAreaDamageComponent>(projectile),
                "PKAUpgradeHybridAoE" =>
                    SEntMan.HasComponent<KineticMiningAreaProjectileComponent>(projectile) &&
                    SEntMan.HasComponent<ProjectileAreaDamageComponent>(projectile),
                "PKAUpgradeHumanPassthrough" =>
                    SEntMan.HasComponent<KineticHumanPassthroughProjectileComponent>(projectile),
                "PKAUpgradeDronePassthrough" =>
                    SEntMan.HasComponent<KineticDronePassthroughProjectileComponent>(projectile),
                "PKAUpgradeRapidRepeater" =>
                    SEntMan.HasComponent<KineticRapidRepeaterProjectileComponent>(projectile),
                "PKAUpgradeResonatorBlast" =>
                    SEntMan.HasComponent<KineticResonatorProjectileComponent>(projectile),
                "PKAUpgradeDeathSyphon" =>
                    SEntMan.HasComponent<KineticDeathSyphonProjectileComponent>(projectile),
                "PKAUpgradeTracerAmber" =>
                    SEntMan.HasComponent<PointLightComponent>(projectile),
                _ => false,
            };

            Assert.That(prepared, Is.True, $"{modkit} did not prepare its fired projectile");
        }
    }

    [Test]
    public async Task EveryBaselinePkaModkitStillModifiesTheFiringPipeline()
    {
        var (_, baselineProjectiles) = await FireWithoutUpgrade("WeaponProtoKineticAccelerator");
        var baselineProjectile = baselineProjectiles.Single();
        var baselineDamage = SEntMan.GetComponent<ProjectileComponent>(baselineProjectile).Damage.GetTotal();
        var baselineSpeed = SEntMan.GetComponent<PhysicsComponent>(baselineProjectile).LinearVelocity.Length();

        var (_, damageProjectiles) = await FireWithUpgrade(
            "WeaponProtoKineticAccelerator",
            "PKAUpgradeDamage",
            "upgrade_slot_1");
        Assert.That(
            SEntMan.GetComponent<ProjectileComponent>(damageProjectiles.Single()).Damage.GetTotal(),
            Is.GreaterThan(baselineDamage));

        var (_, rangeProjectiles) = await FireWithUpgrade(
            "WeaponProtoKineticAccelerator",
            "PKAUpgradeRange",
            "upgrade_slot_1");
        Assert.That(
            SEntMan.GetComponent<PhysicsComponent>(rangeProjectiles.Single()).LinearVelocity.Length(),
            Is.GreaterThan(baselineSpeed));

        var (fireRateGun, _) = await FireWithUpgrade(
            "WeaponProtoKineticAccelerator",
            "PKAUpgradeFireRate",
            "upgrade_slot_1");
        Assert.That(SEntMan.GetComponent<GunComponent>(fireRateGun).FireRateModified, Is.GreaterThan(0.8f));

        var (pressureGun, _) = await FireWithUpgrade(
            "WeaponProtoKineticAccelerator",
            "PKAUpgradePressure",
            "upgrade_slot_1");
        var pressureEfficiency = SEntMan.GetComponent<PressureEfficiencyComponent>(pressureGun);
        Assert.That(pressureEfficiency.LowerBound, Is.EqualTo(0f));
        Assert.That(pressureEfficiency.UpperBound, Is.EqualTo(200f));

        var (spaceGun, _) = await FireWithUpgrade(
            "WeaponProtoKineticAccelerator",
            "PKAUpgradeSpace",
            "upgrade_slot_1");
        var spaceEfficiency = SEntMan.GetComponent<PressureEfficiencyComponent>(spaceGun);
        Assert.That(spaceEfficiency.LowerBound, Is.EqualTo(0f));
        Assert.That(spaceEfficiency.UpperBound, Is.EqualTo(10f));
        Assert.That(SEntMan.GetComponent<PressureDamageChangeComponent>(spaceGun).AppliedModifier, Is.EqualTo(1.5f));
    }

    [Test]
    public async Task EveryBossTrophyTagsItsRangedProjectile()
    {
        var cases = new[]
        {
            (Id: "TrophyLavalandAshDrakeSpike", Field: "ash"),
            (Id: "TrophyLavalandColossusBlasterTubes", Field: "colossus"),
            (Id: "TrophyLavalandBubblegumDemonClaws", Field: "demon"),
            (Id: "TrophyLavalandLegionSkull", Field: "legion"),
            (Id: "TrophyLavalandBDMEye", Field: "blood-drunk"),
            (Id: "TrophySpiderMercuryAlloy", Field: "mercury"),
            (Id: "TrophyChildishOniHorn", Field: "oni"),
            (Id: "TrophyDemonicFrostMinerIceTalisman", Field: "ice"),
        };

        foreach (var testCase in cases)
        {
            var (_, projectiles) = await FireWithUpgrade(
                "WeaponProtoKineticAccelerator",
                testCase.Id,
                "trophy_slot_1");
            Assert.That(projectiles, Is.Not.Empty, testCase.Id);

            foreach (var projectile in projectiles)
            {
                var trophy = SEntMan.GetComponent<KineticTrophyProjectileComponent>(projectile);
                var source = testCase.Field switch
                {
                    "ash" => trophy.AshDrakeTrophy,
                    "colossus" => trophy.ColossusTrophy,
                    "demon" => trophy.DemonClawsTrophy,
                    "legion" => trophy.LegionTrophy,
                    "blood-drunk" => trophy.BloodDrunkTrophy,
                    "mercury" => trophy.MercuryTrophy,
                    "oni" => trophy.OniTrophy,
                    "ice" => trophy.IceTalismanTrophy,
                    _ => null,
                };
                Assert.That(source, Is.Not.Null, $"{testCase.Id} missed one fired projectile");
            }
        }
    }

    [Test]
    public async Task PortablePkaProjectileDamageContributesWithoutLastHitRequirement()
    {
        await SpawnTarget("MegafaunaLootAccountingDummy");
        var boss = STarget!.Value;
        var loot = SEntMan.GetComponent<SpawnLootOnDeathComponent>(boss);
        var pka = ToServer(await Spawn("WeaponProtoKineticAccelerator"));
        var contribution = new DamageSpecifier { DamageDict = { ["Blunt"] = 40 } };

        await Server.WaitPost(() =>
        {
            var projectile = SEntMan.SpawnAtPosition(
                "BulletKinetic",
                SEntMan.GetComponent<TransformComponent>(boss).Coordinates);
            var projectileComponent = SEntMan.GetComponent<ProjectileComponent>(projectile);
            var marker = SEntMan.EnsureComponent<MegafaunaWeaponLooterProjectileComponent>(projectile);
            marker.SourceWeapon = pka;
            projectileComponent.Shooter = SPlayer;
            projectileComponent.Weapon = pka;

            var projectileHit = new ProjectileHitEvent(contribution, boss, SPlayer);
            SEntMan.EventBus.RaiseLocalEvent(projectile, ref projectileHit);
            _damage.TryChangeDamage(boss, contribution, ignoreResistances: true, origin: SPlayer);
        });
        await RunTicks(2);

        Assert.That(loot.QualifiedDamage, Is.GreaterThan(FixedPoint2.Zero));
        var qualifiedAfterPka = loot.QualifiedDamage;

        var crowbar = ToServer(await Spawn("Crowbar"));
        await Server.WaitPost(() =>
        {
            var attacked = new AttackedEvent(
                crowbar,
                SPlayer,
                SEntMan.GetComponent<TransformComponent>(boss).Coordinates);
            SEntMan.EventBus.RaiseLocalEvent(boss, attacked);
            _damage.TryChangeDamage(boss, contribution, ignoreResistances: true, origin: SPlayer);
        });
        await RunTicks(2);

        Assert.That(loot.QualifiedDamage, Is.EqualTo(qualifiedAfterPka),
            "non-kinetic damage must neither contribute nor erase earlier cooperative progress");
    }

    private async Task<(EntityUid Gun, List<EntityUid> Projectiles)> FireWithUpgrade(
        string gunPrototype,
        string upgradePrototype,
        string slot)
    {
        var gun = ToServer(await Spawn(gunPrototype));
        var upgrade = ToServer(await Spawn(upgradePrototype));
        List<EntityUid> projectiles = [];

        await Server.WaitAssertion(() =>
        {
            Assert.That(_itemSlots.TryInsert(gun, slot, upgrade, null), Is.True,
                $"failed to insert {upgradePrototype} into {gunPrototype}:{slot}");
            FireGunImmediately(gun);
            projectiles = ProjectilesFiredBy(gun);
        });

        return (gun, projectiles);
    }

    private async Task<(EntityUid Gun, List<EntityUid> Projectiles)> FireWithoutUpgrade(string gunPrototype)
    {
        var gun = ToServer(await Spawn(gunPrototype));
        List<EntityUid> projectiles = [];
        await Server.WaitAssertion(() =>
        {
            FireGunImmediately(gun);
            projectiles = ProjectilesFiredBy(gun);
        });
        return (gun, projectiles);
    }

    private void FireGunImmediately(EntityUid gun)
    {
        var provider = SEntMan.GetComponent<BasicEntityAmmoProviderComponent>(gun);
        Assert.That(provider.Proto, Is.Not.Null, $"{SEntMan.ToPrettyString(gun)} has no entity ammunition");

        var from = SEntMan.GetComponent<TransformComponent>(SPlayer).Coordinates;
        var to = new EntityCoordinates(SPlayer, new Vector2(10f, 0f));
        var ammunition = SEntMan.SpawnAtPosition(provider.Proto!, from);
        SGun.Shoot(
            (gun, SEntMan.GetComponent<GunComponent>(gun)),
            ammunition,
            from,
            to,
            out _,
            SPlayer);
    }

    private List<EntityUid> ProjectilesFiredBy(EntityUid gun)
    {
        var result = new List<EntityUid>();
        var query = SEntMan.EntityQueryEnumerator<ProjectileComponent>();
        while (query.MoveNext(out var uid, out var projectile))
        {
            if (projectile.Weapon == gun)
                result.Add(uid);
        }

        return result;
    }

    [Test]
    public async Task DensityCoreExpandsAndRestoresCargoCapacity()
    {
        var crateNet = await Spawn("CrateGenericSteel");
        var manipulatorNet = await Spawn("CrateCargoDensityManipulator");
        var firstCoreNet = await Spawn("OniDensityCore");
        var secondCoreNet = await Spawn("OniDensityCore");
        var crate = ToServer(crateNet);
        var manipulator = ToServer(manipulatorNet);
        var firstCore = ToServer(firstCoreNet);
        var secondCore = ToServer(secondCoreNet);

        Assert.That(SEntMan.GetComponent<EntityStorageComponent>(crate).Capacity, Is.EqualTo(30));
        Assert.That(SEntMan.GetComponent<EntityStorageComponent>(manipulator).Capacity, Is.EqualTo(60));

        await Server.WaitPost(() =>
        {
            Assert.That(_containers.TryGetContainer(crate, "density_core", out var crateSlot), Is.True);
            Assert.That(_containers.Insert(firstCore, crateSlot), Is.True);
            Assert.That(_containers.TryGetContainer(manipulator, "density_core", out var manipulatorSlot), Is.True);
            Assert.That(_containers.Insert(secondCore, manipulatorSlot), Is.True);
        });
        await RunTicks(2);

        Assert.That(SEntMan.GetComponent<EntityStorageComponent>(crate).Capacity, Is.EqualTo(60));
        Assert.That(SEntMan.GetComponent<EntityStorageComponent>(manipulator).Capacity, Is.EqualTo(150));

        await Server.WaitPost(() =>
        {
            Assert.That(_containers.TryGetContainer(crate, "density_core", out var crateSlot), Is.True);
            Assert.That(_containers.Remove(firstCore, crateSlot), Is.True);
            Assert.That(_containers.TryGetContainer(manipulator, "density_core", out var manipulatorSlot), Is.True);
            Assert.That(_containers.Remove(secondCore, manipulatorSlot), Is.True);
        });
        await RunTicks(2);

        Assert.That(SEntMan.GetComponent<EntityStorageComponent>(crate).Capacity, Is.EqualTo(30));
        Assert.That(SEntMan.GetComponent<EntityStorageComponent>(manipulator).Capacity, Is.EqualTo(60));
    }

    [Test]
    public async Task LegionTrophyRaisesOnlyOneFaunaAndCannotRecycleIt()
    {
        var trophyNet = await Spawn("MegafaunaLegionTrophyTest");
        var firstFaunaNet = await Spawn("MegafaunaLegionTrophyFauna");
        var secondFaunaNet = await Spawn("MegafaunaLegionTrophyFauna");
        var trophy = ToServer(trophyNet);
        var firstFauna = ToServer(firstFaunaNet);
        var secondFauna = ToServer(secondFaunaNet);

        await Server.WaitPost(() =>
        {
            var marker = new AfterMarkerAttackedEvent(trophy, SPlayer, firstFauna, new DamageSpecifier());
            SEntMan.EventBus.RaiseLocalEvent(trophy, ref marker);
            _damage.TryChangeDamage(
                firstFauna,
                new DamageSpecifier { DamageDict = { ["Blunt"] = 30 } },
                ignoreResistances: true,
                origin: SPlayer);
        });
        await RunTicks(3);

        Assert.That(SEntMan.GetComponent<MobStateComponent>(firstFauna).CurrentState, Is.EqualTo(Content.Shared.Mobs.MobState.Alive));
        Assert.That(SEntMan.HasComponent<LegionTrophyRaisedAllyComponent>(firstFauna), Is.True);
        Assert.That(_damage.GetTotalDamage(firstFauna), Is.EqualTo(FixedPoint2.Zero));

        // A living ally occupies the trophy's single control channel.
        await Server.WaitPost(() =>
        {
            var marker = new AfterMarkerAttackedEvent(trophy, SPlayer, secondFauna, new DamageSpecifier());
            SEntMan.EventBus.RaiseLocalEvent(trophy, ref marker);
            _damage.TryChangeDamage(
                secondFauna,
                new DamageSpecifier { DamageDict = { ["Blunt"] = 30 } },
                ignoreResistances: true,
                origin: SPlayer);
        });
        await RunTicks(3);

        Assert.That(SEntMan.GetComponent<MobStateComponent>(secondFauna).CurrentState, Is.EqualTo(Content.Shared.Mobs.MobState.Dead));
        Assert.That(SEntMan.HasComponent<LegionTrophyRaisedAllyComponent>(secondFauna), Is.False);

        // Killing the first ally frees the channel, but its marker makes that corpse permanently ineligible.
        await Server.WaitPost(() => _damage.TryChangeDamage(
            firstFauna,
            new DamageSpecifier { DamageDict = { ["Blunt"] = 30 } },
            ignoreResistances: true,
            origin: SPlayer));
        await RunTicks(2);
        await Server.WaitPost(() =>
        {
            var recycle = new AfterMarkerAttackedEvent(trophy, SPlayer, firstFauna, new DamageSpecifier());
            SEntMan.EventBus.RaiseLocalEvent(trophy, ref recycle);
            var replacement = new AfterMarkerAttackedEvent(trophy, SPlayer, secondFauna, new DamageSpecifier());
            SEntMan.EventBus.RaiseLocalEvent(trophy, ref replacement);
        });
        await RunTicks(3);

        Assert.That(SEntMan.GetComponent<MobStateComponent>(firstFauna).CurrentState, Is.EqualTo(Content.Shared.Mobs.MobState.Dead));
        Assert.That(SEntMan.GetComponent<MobStateComponent>(secondFauna).CurrentState, Is.EqualTo(Content.Shared.Mobs.MobState.Alive));
        Assert.That(SEntMan.HasComponent<LegionTrophyRaisedAllyComponent>(secondFauna), Is.True);
    }

    [Test]
    public async Task BubblegumNormalDeathLeavesItsHarvestableCarcass()
    {
        var originMap = Transform.GetMapCoordinates(SPlayer).MapId;
        await SpawnTarget("LavalandBossBubblegum");
        var bubblegum = STarget!.Value;

        await Server.WaitPost(() => _damage.TryChangeDamage(
            bubblegum,
            new DamageSpecifier { DamageDict = { ["Blunt"] = 3000 } },
            ignoreResistances: true,
            origin: SPlayer));
        await RunTicks(5);

        Assert.That(SEntMan.Deleted(bubblegum), Is.False);
        Assert.That(SEntMan.GetComponent<MobStateComponent>(bubblegum).CurrentState,
            Is.EqualTo(Content.Shared.Mobs.MobState.Dead));
        Assert.That(SEntMan.HasComponent<MegafaunaHarvestableComponent>(bubblegum), Is.True);
        Assert.That(CountPrototype("LavalandBossBubblegumSecondLife"), Is.Zero);
        Assert.That(Transform.GetMapCoordinates(SPlayer).MapId, Is.EqualTo(originMap));
    }

    [Test]
    public async Task EveryMegafaunaItemConstructionCompletesAndReleasesTheCrafter()
    {
        foreach (var (recipe, expectedResult, ingredients) in MegafaunaItemConstructionCases)
        {
            var supplied = new List<EntityUid>();
            foreach (var (prototype, quantity) in ingredients)
                supplied.Add(await SpawnEntity((prototype, quantity), SEntMan.GetCoordinates(PlayerCoords)));

            await CraftItem(recipe);
            var result = await FindEntity(expectedResult);

            Assert.That(_containers.HasContainer(SPlayer, "item_construction", null), Is.False,
                $"{recipe} left the crafter locked by its hidden material container");
            Assert.That(ActiveDoAfters, Is.Empty, $"{recipe} left an active construction DoAfter");
            Assert.That(supplied.All(SEntMan.Deleted), Is.True, $"{recipe} did not consume all supplied ingredients");

            await Server.WaitPost(() => SEntMan.DeleteEntity(result));
            await RunTicks(2);
        }
    }

    [Test]
    public async Task CancellingEveryMegafaunaItemConstructionReturnsMaterialsAndReleasesTheCrafter()
    {
        foreach (var (recipe, _, ingredients) in MegafaunaItemConstructionCases)
        {
            var supplied = new List<EntityUid>();
            foreach (var (prototype, quantity) in ingredients)
                supplied.Add(await SpawnEntity((prototype, quantity), SEntMan.GetCoordinates(PlayerCoords)));

            Task<bool> constructionTask = default!;
#pragma warning disable CS4014 // The construction task completes after its awaited DoAfter is cancelled.
            await Server.WaitPost(() =>
            {
                constructionTask = SConstruction.TryStartItemConstruction(recipe, SEntMan.GetEntity(Player));
            });
#pragma warning restore CS4014
            await RunTicks(1);

            Assert.That(ActiveDoAfters.Count(), Is.EqualTo(1), $"{recipe} did not start its construction DoAfter");
            await CancelDoAfters();

            while (!constructionTask.IsCompleted)
                await RunTicks(1);

#pragma warning disable RA0004
            Assert.That(constructionTask.Result, Is.False, $"{recipe} reported success after cancellation");
#pragma warning restore RA0004
            await RunTicks(5);

            Assert.That(_containers.HasContainer(SPlayer, "item_construction", null), Is.False,
                $"{recipe} left the crafter locked after cancellation");
            Assert.That(ActiveDoAfters, Is.Empty, $"{recipe} left a cancelled DoAfter active");
            var returned = new List<EntityUid>();
            foreach (var (prototype, quantity) in ingredients)
            {
                var ingredient = await FindEntity((prototype, quantity));
                returned.Add(ingredient);
                Assert.That(_containers.IsEntityInContainer(ingredient), Is.False,
                    $"{recipe} left returned {prototype} hidden in a container");
            }

            await Server.WaitPost(() =>
            {
                foreach (var uid in returned)
                    SEntMan.DeleteEntity(uid);
            });
            await RunTicks(2);
        }
    }

    [Test]
    public async Task OrphanedMegafaunaConstructionContainerSelfHeals()
    {
        var remains = await SpawnEntity("DrakeRemains", SEntMan.GetCoordinates(PlayerCoords));
        var sinew = await SpawnEntity(("MegafaunaSinew", 2), SEntMan.GetCoordinates(PlayerCoords));

        await Server.WaitAssertion(() =>
        {
            var orphaned = _containers.EnsureContainer<Container>(SPlayer, "item_construction", out var existed);
            Assert.That(existed, Is.False);
            Assert.That(_containers.Insert(remains, orphaned), Is.True);
            Assert.That(_containers.Insert(sinew, orphaned), Is.True);
        });

        Assert.That(_containers.HasContainer(SPlayer, "item_construction", null), Is.True,
            "the test did not reproduce the stale construction lock");

        await CraftItem("DragonArmorConstruction");
        _ = await FindEntity("ClothingOuterArmorDragon");

        Assert.That(_containers.HasContainer(SPlayer, "item_construction", null), Is.False,
            "the stale material container was not recovered");
        Assert.That(ActiveDoAfters, Is.Empty, "the recovered construction left an active DoAfter");
        Assert.That(SEntMan.Deleted(remains), Is.True);
        Assert.That(SEntMan.Deleted(sinew), Is.True);
    }

    [Test]
    public async Task AdminObserverCanCraftDrakeArmor()
    {
        var admin = await SpawnEntity("AdminObserver", SEntMan.GetCoordinates(PlayerCoords));
        var adminCoordinates = SEntMan.GetComponent<TransformComponent>(admin).Coordinates;
        var remains = await SpawnEntity("DrakeRemains", adminCoordinates);
        var sinew = await SpawnEntity(("MegafaunaSinew", 2), adminCoordinates);

        Task<bool> constructionTask = default!;
#pragma warning disable CS4014 // The task is awaited after the server has started it.
        await Server.WaitPost(() =>
        {
            constructionTask = SConstruction.TryStartItemConstruction("DragonArmorConstruction", admin);
        });
#pragma warning restore CS4014

        while (!constructionTask.IsCompleted)
            await RunTicks(1);

#pragma warning disable RA0004
        Assert.That(constructionTask.Result, Is.True, "the admin observer failed to finish Drake armour");
#pragma warning restore RA0004
        await RunTicks(2);
        Assert.That(CountPrototype("ClothingOuterArmorDragon"), Is.EqualTo(1),
            "the admin observer did not produce Drake armour");
        Assert.That(_containers.HasContainer(admin, "item_construction", null), Is.False,
            "the admin observer was left with a hidden construction container");
        Assert.That(SEntMan.Deleted(remains), Is.True);
        Assert.That(SEntMan.Deleted(sinew), Is.True);
    }

    private int CountPrototype(string prototype)
    {
        var count = 0;
        var query = SEntMan.EntityQueryEnumerator<MetaDataComponent>();
        while (query.MoveNext(out _, out var metadata))
        {
            if (metadata.EntityPrototype?.ID == prototype)
                count++;
        }

        return count;
    }

    private int CountLivingLegionSplits()
    {
        var count = 0;
        var query = SEntMan.EntityQueryEnumerator<LegionSplitComponent, MobStateComponent>();
        while (query.MoveNext(out _, out _, out var mobState))
        {
            if (mobState.CurrentState != Content.Shared.Mobs.MobState.Dead)
                count++;
        }

        return count;
    }

    private EntityUid FirstLivingLegionSplit()
    {
        var query = SEntMan.EntityQueryEnumerator<LegionSplitComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out _, out var mobState))
        {
            if (mobState.CurrentState != Content.Shared.Mobs.MobState.Dead)
                return uid;
        }

        Assert.Fail("The Legion encounter has no living fragment.");
        return EntityUid.Invalid;
    }

    private async Task KillCurrentLegionGeneration()
    {
        var living = new List<EntityUid>();
        var query = SEntMan.EntityQueryEnumerator<LegionSplitComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out _, out var mobState))
        {
            if (mobState.CurrentState != Content.Shared.Mobs.MobState.Dead)
                living.Add(uid);
        }

        await Server.WaitPost(() =>
        {
            foreach (var split in living)
            {
                _damage.TryChangeDamage(
                    split,
                    new DamageSpecifier { DamageDict = { ["Blunt"] = 5000 } },
                    ignoreResistances: true,
                    origin: SPlayer);
            }
        });
        await RunTicks(5);
    }

    private void AssertLegionHasNoIntermediateLoot()
    {
        Assert.That(CountPrototype("LavalandCrateNecropolisFilled"), Is.Zero);
        Assert.That(CountPrototype("TrophyLavalandLegionSkull"), Is.Zero);
        Assert.That(CountPrototype("MaterialBones"), Is.Zero);
        Assert.That(CountPrototype("MaterialBones1"), Is.Zero);
        Assert.That(CountPrototype("LegionCore"), Is.Zero);
        Assert.That(CountPrototype("CrowbarRed"), Is.Zero);
    }
}
