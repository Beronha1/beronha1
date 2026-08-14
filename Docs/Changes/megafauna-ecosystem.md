# Megafauna ecosystem expansion

This document is the implementation and administration map for Whiskey Station's
harvestable megafauna expansion. It covers Ash Drake, Colossus, Bubblegum,
Legion, Blood-Drunk/Demonic Frost Miner, Mercury Spider and Childish Oni.
Hierophant, Goliath and Watcher remain outside this harvesting feature.

Bitrunning domains, achievements and persistent round-to-round trophy storage
are intentionally outside scope.

## Port and licensing policy

- BYOND/DM source is a behavior reference only. Every behavior in this feature
  is implemented against Whiskey's current C# ECS APIs.
- Redistributable media keeps its upstream license, source and author in the
  nearest RSI `meta.json` or audio `attributions.yml`.
- Non-commercial Creative Commons media is permitted because Whiskey is a
  non-commercial project, while its attribution and ShareAlike terms still
  apply.
- Media without a traceable redistribution grant is not imported. The mechanic
  receives licensed replacement media or Whiskey-original art instead.
- Detailed decisions and exact upstream revisions are recorded in
  `Docs/Changes/megafauna-asset-licensing.md`.

The retired generated twenty-item atlas has been removed. Its active states were
replaced with already-shipped CC-BY-SA-3.0 or CC0 media recorded in each source
RSI's `meta.json`; chemical products now use normal solution-container visuals.

## Shared architecture

### Ordered carcass harvesting

`MegafaunaHarvestableComponent` defines an ordered list of stages, accepted tool
qualities, duration and an entity-table loot selector. `MegafaunaHarvestSystem`
requires a dead carcass, resolves held and worn harvesting tools, and starts a
server-authoritative DoAfter. Movement, damage, tool loss, target deletion and
invalid state cancel the operation. A stage index advances only after its loot
has spawned, preventing duplicate extraction.

Ash Drake, Bubblegum, Mercury Spider and Childish Oni define a completion
carcass. Finishing their final stage replaces the combat entity with an inert,
drawable solution reservoir. Syringes extract 240u Exothermic Blood, 300u
Demonic Blood, 180u Mercury Venom or 240u Oni Gastric Enzymes respectively.
These reservoirs are deliberately not injectable or refillable.

The current stages use Slicing for hide/tissue, Sawing for bone/organs and
Screwing/Pulsing for mechanical salvage. Wildhunter Knife, Nameless Sword,
Cursed Hands and the H.E.C.K. helmet participate through normal Tool or wearable
harvester components.

Primary code:

- `Content.Lavaland.Shared/Megafauna/Harvesting/`
- `Content.Lavaland.Server/Megafauna/Harvesting/`
- `Resources/Prototypes/_Lavaland/Actions/megafauna_harvest.yml`

### Crusher trophies

Every `BaseWeaponCrusher` descendant and portable PKA has eight dedicated
`trophy_slot_*` containers and a separate 100-point boss-trophy budget. Normal
PKA modkit capacity is not consumed. Duplicate trophy identities are rejected,
while distinct trophies combine in stable pipeline order. Insert/eject uses the
normal item-slot DoAfter, and Examine lists installed trophies, individual cost
and remaining trophy capacity.

A special boss reward is earned when Crushers and portable PKAs collectively
deal at least 60% of the encounter's effective post-resistance health damage.
It is cooperative and has no last-hit rule. Damage from other weapons neither
contributes nor erases qualifying progress. Legion fragments relay their
contribution into the single root encounter so the result cannot duplicate per
fragment.

Trophies and their effects:

| Trophy ID | Cost | Melee and ranged effect |
| --- | ---: | --- |
| `TrophyLavalandAshDrakeSpike` | 25 | Marker heat burst and temporary heat/lava protection; kinetic hits knock back and periodically create a bounded fire burst. |
| `TrophyLavalandColossusBlasterTubes` | 50 | Geometric structural shockwave; valid ranged hits shorten recharge to 40%. |
| `TrophyLavalandBubblegumDemonClaws` | 40 | Marker lifesteal; one shot becomes three full-damage projectiles across 45 degrees with bounded per-hit healing. |
| `TrophyLavalandLegionSkull` | 50 | Fire/attack-speed bonus and one raised fauna ally; every three ranged hits can create at most one active explosive skull. |
| `TrophyLavalandBDMEye` | 30 | Faster handling and short control immunity; ranged hits restore a bounded amount of health. |
| `TrophyDemonicFrostMinerIceTalisman` | 35 | Marker freeze; three ranged hits freeze the target with per-target cooldown. |
| `TrophySpiderMercuryAlloy` | 40 | Energy-reflection utility; kinetic projectiles ricochet exactly once. |
| `TrophyChildishOniHorn` | 35 | Oni melee effect; ranged impact releases a short density knockback wave. |

All trophies are finitely recyclable with the Wildhunter Knife. Raised Legion
fauna cannot be recycled, projectile fragmentation cannot recursively fragment,
ricochets are single-generation and entity-producing effects have hard active
limits.

### Proto-kinetic arsenal

The Paradise proto-kinetic railgun and shockwave are native ECS ports. The
railgun is a two-handed, planet-restricted long-range weapon whose bolt passes
through mobs but stops on structures. The shockwave fires eight short-range
concussive bolts in a ring. Their exact redistributable Paradise sound and
shockwave icon states are imported at commit
`f6b562e6b604f02596861117ea68a2d08e609c2a`; no generated art is used.

Common PKA progression adds Mining AoE, Offensive AoE, crew passthrough,
minebot/drone passthrough and an amber cosmetic tracer. Rare Necropolis-only
loot includes Hybrid AoE, Rapid Repeater, Resonator Blast and Death Syphon,
alongside the existing lifesteal crystal. Rapid Repeater heavily punishes misses
and accelerates valid hits; Resonator Blast maintains one bounded field chain;
Death Syphon learns capped prototype-specific bounties from assisted kills.

The existing `SalvageWeapons` and `KineticModifications` nodes are preserved:
the two weapons and common modkits are appended without creating a new research
dependency chain. Cargo prices follow the existing vendor scale (750–2,000
points); rare modules remain direct Necropolis rewards rather than purchasable
or infinitely reproducible technology.

### Organic organs

Boss organs are permanent physical organs and use the native anatomy and surgery
systems. They do not deteriorate and are not installed with disposable implanters.

- `OrganDivineVocalCords` occupies a dedicated cranial `DivineVocalCords` slot.
  It grants an active Colossus roar and supports spoken stop commands in English,
  Portuguese and Russian. It knocks down and disarms creatures in range on a
  shared cooldown.
- Raw `LegionCore` remains a single-use direct healing item. Stabilizing Serum
  converts it into `OrganStabilizedLegionCore`, which occupies the dedicated
  thoracic `RegenerativeCore` slot, restores its host from critical/dead state
  once and is consumed.
- `OrganCompressedLegionCore` combines a stabilized core with an Oni density
  core. It uses the same exclusive thoracic slot and grants Density Surge rather
  than another automatic resurrection.
- The Demonic Resurrection Crystal is a medical tool used on an intact corpse
  through an interruptible eight-second procedure. It preserves the original
  body, species, organs and inventory and is consumed only after a successful
  resurrection.

### Research Destructor

`ResearchDestructor` accepts only `ResearchArtifact` items, warns before an
irreplaceable item is consumed and requires a second interaction inside the
confirmation window. Destruction awards server points and directly unlocks the
listed hidden technologies. The unlock set prevents repeated technology grants.

Key nodes:

- `ColossusAnomalyApplications`
- `MegafaunaMaterialEngineering`
- `MegafaunaBiomechanics`
- `MercuryORTApplications`

## Boss chains

### Ash Drake

Harvest: Dragon Hide, Dragon Bone, Dragon Wing Membrane, Megafauna Sinew and
Exothermic Blood. Necropolis rewards include Spectral Blade, Lava Staff,
Dragon's Blood and the paired Sacred Flame spellbook/Fireball wand reward.

Departmental results include basic and industrial drake armour, synthetic
megafauna fuel and the Ash Drake crusher spike. Armour grants actual fire and
Lavaland lava immunity while equipped; the staff toggles basalt/lava through a
validated ranged DoAfter.

### Colossus

Harvest: Necro-Alloy, an anomalous crystal and viable Divine Vocal Cords.
Crystals have Ward, Repulsion, Stasis and Reprise modes and unlock the hidden
Colossus node in the Research Destructor. Cain & Abel, personal forcefields and
the reflection emitter complete the science/security chain.

### Bubblegum

Harvest: Demonic Blood, Demonic Chewing Gum and regenerative tissue. Rewards
include H.E.C.K. suit/helmet, Demon Claws, Mayhem in a Bottle, Blood Contract
and Soulscythe. The H.E.C.K. helmet is a wearable harvesting tool and its suit
supports corpse consumption and bounded hallucination effects.

The first boss death transfers participants to a private second-life arena. The
arena closes safely and returns participants after the second body dies. Blood
Contract use is confirmation-gated, logged and restricted to a single bounded
mark rather than silent permanent control.

### Legion

Harvest: Necrotic Legion Skull material, stabilized core and Staff of Storms.
Three skull units can be cultured into `LegionServitorCulture`, which grows up
to two faction-safe biological servitors.

Legion's apparent first death now starts one tracked fragmentation encounter:
the central, non-blocking carcass remains dormant while three large fragments
split into six medium and then twelve small fragments. Fragments disappear on
death and never emit bones, cores, crates or duplicate corpses. The final
fragment completes the encounter exactly once, exposes one Necropolis crate,
conditionally awards the crusher trophy when every phase met its weapon rule,
and unlocks the central carcass for its two ordered harvest stages.

The phrase "Golden Legion Skull" in the original planning inventory could not
be traced to a distinct item in current /tg/, Paradise, BeeStation or SPLURT.
Current /tg/ yields the Staff of Storms; Paradise names its crusher reward an
empowered Legion skull. Whiskey therefore represents this chain with the
necrotic skull material, servitor culture and empowered crusher trophy instead
of assigning false provenance to a fabricated upstream asset.

### Blood-Drunk and Demonic Frost Miners

Blood-Drunk Miner supports Guidance, Hunter and Doom variants plus Cleaving Saw,
Custom Kinetic Accelerator, Wildhunter Knife and Miner's Eye rewards. Demonic
Frost Miner has two combat phases and supplies the Resurrection Crystal, Ice
Energy Crystal, Cursed Ice Boots, Demonic Jackhammer and Ice-block Talisman.

Whiskey salvage stages add Tier-4 Servos, High-Density Circuits, Titanium Alloy
and the Overclocked Plasma Cutter. Those are explicitly presented as recovered
equipment, not historical upstream boss drops, and feed alternative Gygax
fabrication recipes.

### Mercury Spider

The original Goob PR chain is implemented as Crystallized Keratin to ORT Alloy,
Mercury Core, Ether Drinker, Paradox Canceller, Radiant Shield, Radiant MODsuit
and Mercury Railgun. The encounter includes crystal fissures, safe zones,
chasms, escape rope, directional patterns, pulsing lights and radiological arena
effects.

The parallel interdepartmental harvest yields Mercury Silk, Mirrored Chitin and
Mercury Venom. These become superconducting cable, reflective armour, elemental
mercury and neurotoxin products.

### Childish Oni

The four-phase encounter includes phase dialogue, projectile flurry, spiral and
directional patterns, rampage movement, temporary firewalls and controlled boss
music. Rewards include Nameless Sword, Flowery Dress, Cursed Hands/Shoes, Zukin,
Obi, an infinitely regenerating Oni Gourd, Density-Compression Core and Oni
Gastric Enzymes.

The core adds 30 storage slots to ordinary compatible crates and 90 to
`CrateCargoDensityManipulator`; removing it restores the exact base capacity.
The gourd regenerates high-purity Oni Sake, and the enzymes synthesize Universal
Antitoxin Serum.

## Departmental endpoints

| Department | Implemented endpoints |
| --- | --- |
| Science | Research Destructor, direct hidden-node unlocks, anomalous crystals, Colossus/Mercury devices. |
| Engineering | Drake armour, Lava Staff, synthetic thermal fuel, superconducting cable and Mercury energy tools. |
| Medical/Chemistry | Permanent surgical boss organs, Legion-core stabilization, corpse restoration, regeneration serum, antitoxin and venom refinement. |
| Robotics | Tier-4 components and alternative Gygax armour/central/targeting recipes. |
| Cargo | Component-based raw/intact/processed bounties, provenance examine text, density manipulator and trophy vault. |
| Service | Demonic gum, Oni Gourd and Oni Sake with bounded healing/speed effects. |
| Security | Mirrored-chitin suit, Radiant Shield, reflection emitter and Colossus equipment with probabilistic reflection. |

## Multi-boss progression

Construction graphs require material from multiple encounters and high crafting
knowledge. They include:

- `GodslayerArmorConstruction`
- `DrakeMercuryAegisConstruction`
- `ColossusMercuryShieldConstruction`
- `LegionBubblegumRegeneratorConstruction`
- `AshFrostThermalRegulatorConstruction`
- `BloodDrunkMercuryPhaseCutterConstruction`
- `CompressedLegionCoreConstruction`

## Cargo and administration

Cargo products:

- `CargoDensityManipulator` (12,000 credits)
- `CargoMegafaunaTrophyVault` (8,000 credits)

Bounties:

- `BountyMegafaunaRawSamples`
- `BountyMegafaunaIntactSpecimen`
- `BountyMegafaunaProcessedEquipment`

Useful spawn IDs for a live acceptance round:

- Bosses: `LavalandBossAshDrake`, `LavalandBossColossus`,
  `LavalandBossBubblegum`, `LavalandBossMegaLegion`, `MobBloodDrunkMiner`,
  `LavalandBossDemonicFrostMiner`, `MobSpiderMercuryUltimate`,
  `MobChildishOni`.
- Infrastructure: `ResearchDestructor`, `CrateCargoDensityManipulator`,
  `CrateMegafaunaTrophyVault`, `WeaponCrusher`.

Recommended acceptance sequence is: defeat by a crusher-capable character,
inspect the retained carcass, harvest both stages with the correct tools, test
the trophy on every crusher family, process one R&D artifact, synthesize each
boss reagent, complete one departmental recipe and export one component-based
bounty. Then repeat with an interrupted DoAfter and verify no duplicated output.

## Verification

`Content.IntegrationTests/Tests/_Lavaland/Megafauna/MegafaunaHarvestTest.cs`
covers prototype wiring, protected pre-existing mobs, ordered/cancelled/exactly
once harvesting, organ deterioration and stabilization, trophy insertion,
research destruction, departmental recipes, density-core capacity restoration,
Legion ally limits, Bubblegum arena cleanup, Oni phases and Mercury relics.

Before merging, run:

```text
dotnet build Content.Lavaland.Server/Content.Lavaland.Server.csproj --no-restore
dotnet test Content.IntegrationTests/Content.IntegrationTests.csproj --no-restore --filter FullyQualifiedName~MegafaunaHarvest
git diff --check
```
