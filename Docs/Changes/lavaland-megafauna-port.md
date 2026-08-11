# Lavaland megafauna port

This file is the working inventory, provenance matrix, and checkpoint log for
the Whiskey Station megafauna port. Source pull requests are references, not
changes to cherry-pick wholesale.

## Source matrix

| Source | Exact reference | Selected scope | Explicit exclusions / cautions |
| --- | --- | --- | --- |
| Goob Station | [PR #3269](https://github.com/Goob-Station/Goob-Station/pull/3269), merge `1428badeaa` | Megafauna selectors and conditions, EntityShapes, anger and phase support, boss music, reset/rejuvenation, map optimization, tests | Already present in the modular `Content.Lavaland.*` projects; harden in place instead of reapplying |
| Goob Station | [PR #5270](https://github.com/Goob-Station/Goob-Station/pull/5270), merge `7a11657807` | Lavaland loot/progression, Legion, PKA and crusher upgrades, useful ruin changes | Do not import unrelated station maps, roles, GPS UI, or broad item-slot changes unless required |
| Adventure Time Station | [PR #2987](https://github.com/AdventureTimeSS14/space_station_ADT/pull/2987) | Procedural generation, megafauna placement, safe-zone exclusion, fauna and environmental QA ideas | ADT-specific namespaces, economy, GPS, weather, and weapon balance need separate review |
| Adventure Time Station | [PR #3020](https://github.com/AdventureTimeSS14/space_station_ADT/pull/3020) | Population pass, ruin overlap fixes, arena/lair placement fixes, spawn validation | Port algorithms and invariants, not the whole ADT subsystem |
| RedStar | [PR #111](https://github.com/red-star-server/RedStar-14/pull/111) | Bubblegum, Ash Drake, Colossus, Mega Legion, Blood Drunk Miner; combat systems, arenas, effects, audio, and trophies | Code and assets retain `_Wega` provenance and must be adapted to the Whiskey modules |
| RedStar | [PR #117](https://github.com/red-star-server/RedStar-14/pull/117) | Boss AI/range fixes, ruin spawning fixes, trophy effects, PKA/crusher upgrade integration | Selective follow-up to #111; avoid unrelated balance and store changes |
| Wega | [`wega-team/ss14-wega`](https://github.com/wega-team/ss14-wega) | Original implementation/provenance for `_Wega` code and assets | Wega PR #111/#117 are unrelated; those numbers refer to RedStar |
| Goob Station | [PR #6734](https://github.com/Goob-Station/Goob-Station/pull/6734) | Childish Oni arena, phases, attacks, generic phase presentation, and loot | Closed without merge; experimental and conflict-prone, so behavior must be revalidated |
| Goob Station | [PR #6542](https://github.com/Goob-Station/Goob-Station/pull/6542) | Spider of Mercury biome/arena, phases, attacks, radiation systems, loot and crafting | Open/unmerged; fix known console spam, missing alloy recipe, railgun size, and conflicts |

## Whiskey baseline

- Branch baseline: `9f6570fa96` on `Lavaland-e-traduções`.
- Goob #3269 and #5270 are already ancestors of the current branch.
- Lavaland code is split into `Content.Lavaland.Common`, `.Shared`, `.Server`,
  and `.Client`; new runtime code belongs there.
- The reusable megafauna framework, Hierophant encounter, weapon upgrade
  framework, Lavaland resources, and integration-test shell already exist.
- Bubblegum, Ash Drake, Colossus, and Mega Legion currently have simplified
  entity prototypes and media, but no dedicated encounter systems or arenas in
  the active ruin pool. Blood Drunk Miner, Childish Oni, and Spider of Mercury
  are absent.
- The active megafauna marker only spawns Hierophant, and the ruin pool only
  registers the Hierophant arena.
- Locales supported by Whiskey are `en-US` and `pt-BR`.

## Dependency order

1. Verify and test the existing modular core.
2. Establish deterministic arena/spawn contracts.
3. Complete Bubblegum as the vertical reference encounter.
4. Complete Ash Drake and Colossus.
5. Complete Mega Legion and Blood Drunk Miner.
6. Connect trophies, loot, recipes, and weapon progression.
7. Add the configurable Whiskey Megafauna Director.
8. Adapt Childish Oni.
9. Adapt Spider of Mercury.
10. Finish localization, attribution, map/resource validation, build, tests,
    and headless smoke testing where available.

## Validation contract

Each encounter must have an reachable arena/spawn path, working combat loop,
death/reset behavior, reward path, en-US and pt-BR text, and a prototype smoke
test. Core and spawning changes require focused tests before the full solution
build and integration suite. Imported assets keep their SPDX and attribution
metadata.

## Oni-derived failure checklist

The Childish Oni playtests exposed recurring integration boundaries rather than
one isolated boss bug. Every ported megafauna is audited against this list.

| Process / code boundary | Observed failure | Root cause and enforced contract |
| --- | --- | --- |
| Client sandbox loading | Client rejected `CollectionsMarshal.SetCount` before reaching the menu | A .NET 10 collection initializer in `BubblegumTripleDashActionEvent` generated forbidden IL. Shared action-event defaults use sandbox-safe construction and client assembly loading remains part of validation. |
| Phase state replication | First damage/phase change could terminate PVS serialization | Oni appearance keys and values were plain enums. Every value written through `AppearanceSystem.SetData` in this port is now `Serializable` and `NetSerializable`. Mercury and classic visual keys were checked against the same rule. |
| Action selection and targets | Oni claw reached `PerformActionSelector` with an out-of-range target and hit its fatal assertion | Action validation now records failed entity/world validation; the claw retains its source entity-target range contract. Every classic, Oni, Hierophant, and Mercury action prototype was checked for matching instant/entity/world target components. |
| Component-query mutation | An Oni spiral spawned another `TimedSpawner` while that component store was being enumerated | Timed spawners are fired from a UID snapshot. The wider audit also moved completed `MegafaunaActiveBlink` removals after enumeration, preventing the same multi-boss failure pattern. |
| Entity/component ownership | Restored `NavSmash` could call melee with the miner UID paired to the held weapon's component | Melee always receives the entity UID that owns its component. This fixes Blood-drunk Miner and protects the same shared steering path used by the other smashing bosses. |
| Delayed spawn/despawn ordering | A transition could disappear on the same tick that it should spawn the next stage | Timed cinematic spawns must precede despawn by an explicit margin. Oni effects already satisfy this; Mercury forming and UFO-to-Xibalba chains now do too. |
| Arena geometry and source resources | Placeholder arenas were too small, misplaced, or missing presentation data | Oni and Mercury use the original 764/757-entity arena maps and original media. All seven arenas load in tests; classic arenas retain source tile geometry and have non-invasive navigation beacons. |
| Participant/health scaling | Abandoned admin bodies could multiply boss health and make Oni/Hierophant appear nearly invulnerable | Only unique, currently attached player sessions count. The Director is bounded and always recalculates from immutable source health/phase thresholds. |
| Cancelled chasm presentation | Mercury fissure teleport restored a player to sprite scale `(0, 0)`, then the light tree asserted on an invalid AABB | `ChasmFallingComponent.OriginalScale` is a client-only snapshot, cancellation restores a validated non-zero scale, and the shrink animation uses bounded linear interpolation. |

## Checkpoint log

- 2026-08-10 — Source repositories and exact PR ownership verified. Baseline
  inventory completed; no source PR has been applied wholesale.
- 2026-08-10 — Baseline `Content.Lavaland.Server` build passed. Hardened the
  megafauna scheduler against duplicate startup/timestamp collisions, clear
  schedules on death, and preserve immutable phase-threshold baselines.
  Extended the core contracts for selector conditions/order, stateless entity
  shapes, target acquisition/forgiveness, idempotent aggression, target reset,
  mixed entity/world action targeting, and boss-music stream lifecycle.
  `Content.IntegrationTests` builds with zero errors; focused tests pass 3/3.
- 2026-08-10 — Rebuilt Lavaland ruin placement around deterministic seeded
  candidates, exact bounded attempts, per-ruin clearance, and a configurable
  outpost safe zone. Ported the RedStar/Wega Bubblegum arena and verified it
  loads through the current map serializer.
- 2026-08-10 — Ported the complete Bubblegum encounter from RedStar/Wega #111:
  weighted NPC actions, rage, blood dive/pools/hands, triple dash, illusion,
  pentagram, and chaotic phase attacks. Adapted it to current Whiskey APIs while
  retaining the original encounter media,
  hardened zero-distance/blocked-tile dash completion, verified arena spawning
  and the 50% phase transition, then enabled `BubblegumArena` in the active
  ruin pool. Focused megafauna tests pass 3/3.
- 2026-08-10 — Ported Ash Drake, Colossus, Mega Legion, and Blood Drunk Miner
  from RedStar/Wega #111, including weighted NPC actions, projectiles, arena
  attacks, Legion state/split behavior, miner dash/loadout, and all four source
  arenas. All five classic arenas load on isolated maps and use their original
  conditional spawn markers; enabled them in the active ruin pool. Focused
  tests pass and `Content.Lavaland.Server` builds with zero errors.
- 2026-08-10 — Integrated RedStar #117's crusher trophies and marker hooks for
  all five classic bosses. PKA and crusher damage now qualify for trophies,
  non-qualifying damage permanently invalidates special loot for that kill,
  and normal Necropolis progression remains guaranteed. The original Wega
  trophy sprites, boss sprites, attack effects, projectile art, and boss music
  are imported with their source metadata and attribution files.
- 2026-08-10 — Added the Whiskey Megafauna Director. Encounter health and
  action cadence scale monotonically with peak party size and completed bosses
  on the same map plus bounded elapsed-round intervals, with configurable caps
  and round/map cleanup. The opt-in component is globally controlled by
  `lavaland.megafauna_director_enabled`; disabling it restores the immutable
  health/cadence baselines.
- 2026-08-10 — Adapted Childish Oni from experimental Goob #6734 with all nine
  source attack families across four combat states (three phase escalations), a
  dedicated arena, unique weapon/trophy loot, and active ruin-pool placement.
  Imported and connected the original boss phases, action icon, skull/hand/slash
  effects, nameless sword, oni trophy icon, and `childishoni.ogg`. RSI metadata
  credits OnsenCapy/NamelessName338; audio attribution credits Sonican and keeps
  the original Pixabay source link.
- 2026-08-10 — Revalidated Childish Oni against Goob #6734 after its first live
  playtest. Restored the source timing and geometry for flurry, side hands,
  barrage, spiral, orbiting rings, claw, and landing attacks; imported the
  original machete-hit sound and attribution; corrected instant/world-targeted
  action wiring; and made the phase appearance enums network-serializable. A
  focused regression test now damages the boss, forces phase two, and verifies
  that the resulting appearance state can be synchronized to the client.
- 2026-08-10 — Adapted Spider of Mercury from experimental Goob #6542 as a
  three-form endgame encounter. Ground, UFO, and ultimate forms preserve the
  source attack families and transition in-place; only the final form advances
  Director progression and drops the unique Mercury rewards. Its reachable
  crafting chain converts crystallized keratin plus diamond into alloy, then
  combines alloy with the Mercury core into a `Ginormous` 4x2 railgun. This
  fixes the source PR's disconnected alloy recipe and inconsistent railgun
  footprint. Imported and connected the original ground/UFO/ultimate sprites,
  transition and attack effects, core/material/railgun art, footstep pack,
  encounter ambience, attack sounds, `Finale.ogg`, and `Xibalba.ogg`.
- 2026-08-10 — Finished the production pass with matching `en-US` and `pt-BR`
  locale keys, shared client/server action-event contracts, and explicit
  projectile sprite layers. Both Lavaland server and integration-test projects
  build with zero errors. The focused integration suite passes 3/3 and validates
  the core scheduler/phase contracts, all seven arenas, each encounter's initial
  action roster, Bubblegum's phase transition, all three Spider forms and final
  loot wiring, both Mercury crafting paths, the railgun footprint, Director
  party scaling from 2600 to 3380 health, one bounded elapsed-time step to 3640,
  and CVar disable/restore/re-enable.
- 2026-08-10 — Revalidated every ported megafauna against its exact source PR.
  Restored Ash Drake and Colossus action weights, original factions/HTN and
  Wega presentation assets; restored Mega Legion's 800-health baseline,
  split-family identity and last-fragment-only reward; and restored Blood Drunk
  Miner's SSD-indicator cleanup and navigation behavior. Replaced the simplified
  Spider of Mercury attack approximations with the source encounter systems:
  stamina drain, vicinity spawning, expanding cosmic rays, moving resonance
  walls, charged solar storm, paradigm damage conversion, full temporary
  reflection, orbiting shell, safe-zone convergence, phase selector/sprite swap,
  and phase-dependent transport/dash. Current-engine serialization and action
  inheritance adaptations were tested rather than bypassed. Client, server, and
  integration-test projects build with zero errors; focused megafauna tests pass
  3/3.
- 2026-08-10 — Fixed a fatal `AssertOwner` failure in `NPCSteeringSystem` exposed
  by Blood Drunk Miner's restored `NavSmash`: steering now passes the held
  weapon's actual entity UID together with its `MeleeWeaponComponent`, rather
  than pairing that component with the NPC UID. The held-weapon bayonet relay
  now also raises its lookup event on the held entity instead of an uninitialized
  UID. Added a regression contract for the miner's held weapon.
- 2026-08-10 — Replaced the six-entity placeholder Oni arena with the original
  764-entity temple arena from Goob #6734, including its wooden temple, chasm,
  bridge, roof, torch, rope, gourd, skull, beacon, and original visual assets.
  Fixed a fatal nested-spawner crash by snapshotting due `TimedSpawner` entities
  before firing them; an Oni spiral marker can now spawn a second timed spawner
  without invalidating the active component query. Added a direct nested-spawner
  regression test.

- 2026-08-10 — Audited every arena against its exact source revision. The five
  RedStar/Wega maps already had byte-equivalent tile geometry; restored their
  original conditional spawn-marker entities as well. Their serialized entity
  counts are Bubblegum 11, Ash Drake 252, Colossus 6, Mega Legion 6, and Blood
  Drunk Miner 56; the low counts in three maps are original because most arena
  geometry is stored as tile chunks rather than individual entities.
- 2026-08-10 — Replaced the six-entity Whiskey Spider of Mercury placeholder
  with Goob #6542's original 1,747-entity radioactive fissure and 757-entity
  Sea of Fantasy Trees arena. Imported the original fairy/crystal/hazard tiles,
  walls, trees, crystals, spikes, pit, rope, beacon, sprites, and the chasm
  teleport flow. Restored the dormant exoskeleton, forming animation, and
  500-health pre-fight form before the ported combat boss. Current-engine
  compatibility retains the source behavior while dropping only unavailable,
  unrelated RMC/CombatPower metadata and obsolete self-scheduling `Timer`
  declarations. The six focused megafauna integration tests pass.
- 2026-08-10 — Fixed Childish Oni's out-of-range claw fatal without weakening
  the selector assertion. Invalid entity/world action targets now mark action
  validation as failed, and the claw again carries Goob #6734's original
  entity-target contract, so `ActionAvailableCondition` rejects targets beyond
  its two-tile range before `PerformActionSelector` runs. The arena sake gourd
  now keeps its original three-state DV RSI (`icon` plus both in-hand states)
  while disabling `FlaskBase`'s incompatible inherited `icon_open` visualizer;
  its sake reservoir also uses the current `Solution` component. Focused Oni
  claw and gourd regressions pass 2/2, and the integration project builds with
  zero errors.

- 2026-08-10 — Rechecked Hierophant and Childish Oni health against Goob
  #3269/#6734. Their 2500/2600 solo thresholds and phase breakpoints already
  matched the source, so the baselines remain intact. Fixed inflated health
  after admin ghost/body changes by counting unique, currently attached player
  sessions instead of abandoned aggressor bodies. Recalibrated Whiskey's
  optional Director to the source's 20% additional-player target, halved its
  victory/time health increments, shortened elapsed scaling to four intervals,
  and capped health at 1.75x while preserving monotonic encounter difficulty.
- 2026-08-10 — Completed the radioactive fissure's source Mercury wake-up
  path. The dormant core now participates in Trauma's `Injurable` damage path;
  the serialized Sea of Fantasy Trees map is explicitly unpaused when loaded;
  and the 6.5-second forming animation gets one tick of despawn margin so its
  source `TimedSpawner` cannot lose a same-tick race. A regression now destroys
  the 50-damage dormant core, waits for the animation, destroys the 500-health
  pulsing shell, and observes the first Spider of Mercury combat form.
- 2026-08-10 — Added invisible navigation beacons to the five otherwise
  source-identical Wega boss grids. Bubblegum, Ash Drake, Colossus, Mega Legion,
  and Blood-drunk Miner now appear beside Oni and Mercury in Ghost Warp; this
  changes each serialized entity count by exactly one without changing arena
  tiles or combat geometry. The focused megafauna suite passes 8/8 with zero
  skipped tests, and both server and integration-test projects build with zero
  errors.

- 2026-08-10 — Applied the Oni-derived failure checklist to the full roster.
  Fixed same-query removal in concurrent megafauna blinks, bounded Mercury
  pulsing-light geometry, and gave the UFO-to-Xibalba timed spawn an explicit
  despawn margin. Fixed the Mercury fissure client fatal by keeping the chasm
  fall's original sprite scale client-local, validating restoration, and using
  non-overshooting interpolation. The source Sea of Fantasy Trees floor remains
  byte-identical; its separate map now receives the source cosmic-cult ambient
  color so players and terrain render instead of appearing black. A live chain
  regression also found and imported Goob #6542's missing original
  `futuristic-teleport.ogg` plus its Pixabay attribution. Port-scoped resource
  audit reports 64/64 audio and 120/120 sprite references present.

- 2026-08-10 — Restored Goob #6542's original `ThunderStrike` sound collection
  for Mercury's timed thunder effect, using the already imported and attributed
  `thunder_clap.ogg` and `thunderstrike.ogg` assets. Also made client-side
  aggression cleanup tolerate the valid replication order where the reverse
  `AggressorComponent` has already been removed; the authoritative server still
  emits an error for a genuinely broken aggression pair. The complete
  port-scoped collection audit resolves 13/13 references, and focused
  server/client regressions pass 2/2. The full megafauna integration suite
  passes 13/13 with zero skipped tests, and the integration project builds
  with zero errors.

- 2026-08-10 — Restored the original RedStar/Wega reward paths for Bubblegum,
  Colossus, Ash Drake, and Mega Legion without changing their source arenas.
  Boss-specific Necropolis crates, Dragon Blood and its lesser-drake form, Lava
  Staff, Spectral Blade, Spellblade, Divine Vocal Cords, H.E.C.K. equipment,
  Legion Core, and stabilizing serum now use their original sprites and embedded
  attribution metadata. Mega Legion again uses the source's 20% Necropolis-crate
  and 10% Legion-Core chances, while its final fragment still awards the red
  crowbar. Focused reward and client-sprite regressions pass 2/2; the complete
  megafauna suite passes 15/15, and the Lavaland server and integration-test
  projects build with zero errors. Existing warnings were not suppressed or
  changed.

## Experimental-source divergences and residual risks

- Goob #6734 exposes four combat states (three threshold escalations), despite
  being described informally as a three-phase encounter. Whiskey preserves all
  nine attack families. The source's generic movement/spawn helpers were adapted
  into Oni-local server components so the original spiral, trail, orbit, side
  hand, and target-spawn behavior remains intact without coupling Lavaland to
  unrelated experimental subsystems.
- Goob #6542's original fissure biome and separate arena are imported and are
  connected by their source pit/beacon/escape-rope flow. Ether Drinker, Paradox
  Canceller, shield, and Radiant MODsuit remain outside this arena/combat port;
  the encounter's combat radiation, three forms, final rewards, alloy recipe,
  core, and railgun progression are self-contained. Omitting Ether Drinker also
  leaves out the source's known server-console spam path.
- The Mercury combat helpers are local to `Content.Lavaland.*` instead of being
  copied into unrelated shared modules. Obsolete source `Timer` components were
  removed because the current `TimedSpawner` is self-scheduling, directional
  vectors use the current serializer format, and instant actions deliberately do
  not inherit `TargetAction`; the latter prevents invalid target-attempt events.
- The classic roster keeps its source-specific reward rules alongside Whiskey's
  Director integration. Bubblegum, Colossus, and Ash Drake use their dedicated
  reward crates; Mega Legion uses the source's conditional generic-crate and
  Legion-Core chances, keeps its split encounter, and awards the red crowbar on
  the final fragment. The Legion Core and stabilizing-serum subsystem is active.
- Childish Oni and Spider now use the original Goob #6734/#6542 media for every
  presentation path implemented by this port. Each RSI keeps its source
  `meta.json`; each audio directory keeps its source `attributions.yml`. The
  omitted Mercury subsystems above have no runtime media path here. The classic
  roster likewise uses the original RedStar/Wega media and embedded attribution
  metadata instead of substitute Whiskey sprites or sounds.
- Automated QA covers serialization, maps, actions, phases, progression,
  crafting graphs, Director scaling, and client/server startup. Multiplayer
  balance and encounter readability still need a live playtest before release.
