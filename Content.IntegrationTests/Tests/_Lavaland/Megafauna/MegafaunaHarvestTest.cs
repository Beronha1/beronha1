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
using Content.Lavaland.Shared.Research;
using Content.Lavaland.Shared.Weapons.Upgrades;
using Content.Lavaland.Server.Megafauna.Classic;
using Content.Lavaland.Server.Megafauna.Bubblegum;
using Content.Lavaland.Server.Mobs;
using Content.Server.Construction.Components;
using Content.Medical.Shared.Surgery.Tools;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
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
using Content.Shared.Research.Prototypes;
using Content.Shared.Stacks;
using Content.Shared.Storage.Components;
using Content.Shared.Timing;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;
using Content.Shared.Weapons.Reflect;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Localization;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

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
        "DivineVocalCordsImplanter",
        "DivineVocalCordsImplant",
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
        "StabilizedLegionCoreImplanter",
        "StabilizedLegionCoreImplant",
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
        "ReinforcedLegionOniSurvivalImplanter",
        "ReinforcedLegionOniSurvivalImplant",
        "DrakeRemains",
        "ResearchDestructorMachineCircuitboard",
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
        ("LegionOniSurvivalImplanterConstruction", "LegionOniSurvivalImplanterConstructionGraph", "implanter", "ReinforcedLegionOniSurvivalImplanter"),
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

            foreach (var id in Trophies)
            {
                var prototype = Prototype<EntityPrototype>(prototypes, id);
                Assert.That(prototype.TryComp<CrusherTrophyComponent>(out _, components), Is.True);
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
            Assert.That(resurrectionCrystal.TryComp<ResurrectionCrystalComponent>(out _, components), Is.True);
            var cursedBoots = Prototype<EntityPrototype>(prototypes, "ClothingShoesBootsCursedIce");
            Assert.That(cursedBoots.TryComp<CursedIceBootsComponent>(out _, components), Is.True);
            var godslayer = Prototype<EntityPrototype>(prototypes, "ClothingOuterArmorGodslayer");
            Assert.That(godslayer.TryComp<GodslayerArmorComponent>(out _, components), Is.True);

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
            Assert.That(crusher.TryComp<WeaponTrophySlotComponent>(out _, components), Is.True);

            var cords = Prototype<EntityPrototype>(prototypes, "DivineVocalCordsImplanter");
            Assert.That(cords.TryComp<PerishableBossOrganComponent>(out var perishable, components), Is.True);
            Assert.That(perishable!.FreshDuration, Is.EqualTo(TimeSpan.FromMinutes(4)));

            var legionImplanter = Prototype<EntityPrototype>(prototypes, "StabilizedLegionCoreImplanter");
            Assert.That(legionImplanter.TryComp<PerishableBossOrganComponent>(out var stabilized, components), Is.True);
            Assert.That(stabilized!.State, Is.EqualTo(PerishableBossOrganState.Stabilized));

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
            Assert.That(swordAction!.Actions, Does.Contain((EntProtoId) "ActionChildishOniBladeFlamingSlash"));

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
                         "ReinforcedLegionOniSurvivalImplanter",
                     })
            {
                var reward = Prototype<EntityPrototype>(prototypes, multibossReward);
                Assert.That(reward.TryComp<MegafaunaProcessedRewardComponent>(out _, components), Is.True);
                Assert.That(reward.TryComp<MegafaunaProvenanceComponent>(out var provenance, components), Is.True);
                Assert.That(provenance!.Grade, Is.EqualTo(MegafaunaProvenanceGrade.Processed));
            }

            var reinforcedImplant = Prototype<EntityPrototype>(prototypes, "ReinforcedLegionOniSurvivalImplant");
            Assert.That(reinforcedImplant.TryComp<StabilizedLegionCoreImplantComponent>(out var reinforcedCore, components), Is.True);
            Assert.That(reinforcedCore!.MaxActivations, Is.EqualTo(2));

            foreach (var construction in new[]
                     {
                         "DrakeMercuryAegisConstruction",
                         "ColossusMercuryShieldConstruction",
                         "LegionBubblegumRegeneratorConstruction",
                         "AshFrostThermalRegulatorConstruction",
                         "BloodDrunkMercuryPhaseCutterConstruction",
                         "LegionOniSurvivalImplanterConstruction",
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
            [("StabilizedLegionCoreImplanter", 1), ("ChemistryBottleDemonicBlood", 1)]),
        ("AshFrostThermalRegulatorConstruction", "ClothingBeltAshFrostThermalRegulator",
            [("IceEnergyCrystal", 1), ("DragonWingMembrane", 3), ("DragonBone", 3)]),
        ("BloodDrunkMercuryPhaseCutterConstruction", "WeaponBloodDrunkMercuryPhaseCutter",
            [("WeaponPlasmaCutterOverclocked", 1), ("SpiderMercuryAlloy", 4)]),
        ("LegionOniSurvivalImplanterConstruction", "ReinforcedLegionOniSurvivalImplanter",
            [("StabilizedLegionCoreImplanter", 1), ("OniDensityCore", 1)]),
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
  id: MegafaunaPerishableOrganTest
  components:
  - type: PerishableBossOrgan
    freshDuration: 20

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

        // A non-qualifying hit on any descendant invalidates the one trophy
        // owned by the root encounter. Reset it after proving the relay so this
        // scenario can also verify the positive reward path at completion.
        var crowbar = ToServer(await Spawn("Crowbar"));
        var firstSplit = FirstLivingLegionSplit();
        await Server.WaitPost(() =>
        {
            var coordinates = SEntMan.GetComponent<TransformComponent>(firstSplit).Coordinates;
            var attacked = new AttackedEvent(crowbar, SPlayer, coordinates);
            SEntMan.EventBus.RaiseLocalEvent(firstSplit, attacked);
        });
        var rootLoot = SEntMan.GetComponent<SpawnLootOnDeathComponent>(root);
        Assert.That(rootLoot.DoSpecialLoot, Is.False);
        await Server.WaitPost(() => rootLoot.DoSpecialLoot = true);

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
        Assert.That(CountPrototype("MaterialBones"), Is.Zero);
        Assert.That(CountPrototype("MaterialBones1"), Is.Zero);
        Assert.That(CountPrototype("LegionCore"), Is.Zero);
        Assert.That(CountPrototype("CrowbarRed"), Is.Zero);
    }

    [Test]
    public async Task BossOrganDecayPausesInAFreezerAndStabilizationIsPermanent()
    {
        var organNet = await Spawn("MegafaunaPerishableOrganTest");
        var freezerNet = await Spawn("CrateFreezer");
        var organ = ToServer(organNet);
        var freezer = ToServer(freezerNet);
        var component = SEntMan.GetComponent<PerishableBossOrganComponent>(organ);

        await Server.WaitPost(() =>
        {
            Assert.That(_containers.TryGetContainer(freezer, "entity_storage", out var container), Is.True);
            Assert.That(_containers.Insert(organ, container), Is.True);
        });
        await RunTicks(25);

        Assert.That(component.State, Is.EqualTo(PerishableBossOrganState.Fresh));
        Assert.That(component.PreservedBy, Is.EqualTo(freezer));

        await Server.WaitPost(() =>
        {
            Assert.That(_containers.TryGetContainer(freezer, "entity_storage", out var container), Is.True);
            Assert.That(_containers.Remove(organ, container), Is.True);
            _entityEffects.RaiseEffectEvent(organ, new StabilizeMegafaunaOrgan(), 1f, null, predicted: false);
        });
        await RunTicks(25);

        Assert.That(component.State, Is.EqualTo(PerishableBossOrganState.Stabilized));
        Assert.That(component.DecayAt, Is.Null);
    }

    [Test]
    public async Task CrusherAcceptsExactlyOneTrophyInItsDedicatedSlot()
    {
        await SpawnTarget("WeaponCrusher");
        var crusher = STarget!.Value;
        var slots = SEntMan.GetComponent<ItemSlotsComponent>(crusher);

        await InteractUsing("TrophyLavalandAshDrakeSpike");
        var installed = slots.Slots["trophy_slot"].Item;
        Assert.That(installed, Is.Not.Null);
        Assert.That(SEntMan.HasComponent<CrusherTrophyComponent>(installed!.Value), Is.True);

        // A second trophy cannot spill into the legacy blade/handle slots inherited by trophy prototypes.
        await InteractUsing("TrophyLavalandBubblegumDemonClaws", awaitDoAfters: false);
        Assert.That(ActiveDoAfters, Is.Empty);
        Assert.That(slots.Slots["trophy_slot"].Item, Is.EqualTo(installed));
        Assert.That(slots.Slots["upgrade_slot_blade"].Item, Is.Null);
        Assert.That(slots.Slots["upgrade_slot_handle"].Item, Is.Null);
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
