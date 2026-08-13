# Megafauna asset licensing ledger

This ledger tracks licensing decisions made while expanding the seven
harvestable megafauna. It supplements the per-RSI `meta.json` and per-audio
`attributions.yml`; those files remain authoritative for individual assets.

## Project policy

- CC-BY, CC-BY-SA, CC-BY-NC and CC-BY-NC-SA assets are eligible. Whiskey
  Station is a non-commercial project.
- BYOND/DM behavior is used as a design reference and reimplemented in C# ECS.
- An asset without a traceable redistribution license is replaced with a
  licensed or original equivalent. Missing art never removes its mechanic.
- Modified assets retain source, author, license and a modification notice.

## Decisions already applied

| Asset | Origin | Decision |
| --- | --- | --- |
| `_Lavaland/Effects/160x160.rsi` | Goonstation `318ff1e` | Corrected to CC-BY-NC-SA-3.0. |
| `_Lavaland/Effects/64x128.rsi` | Goonstation `318ff1e` | Corrected to CC-BY-NC-SA-3.0. |
| `_Lavaland/Effects/64x64.rsi` | Goonstation `318ff1e`, plus `safe_zone` by OnsenCapy | Entire combined RSI conservatively marked CC-BY-NC-SA-3.0. |
| `_Lavaland/Structures/Specific/bridge.rsi` | Goonstation `c2b2171` | Corrected to CC-BY-NC-SA-3.0. |
| Mercury `Finale.ogg` | Hitz.me attribution did not establish a redistribution license | Replaced in prototypes by licensed `Xibalba.ogg`; binary removed. |
| `legion_spawn.ogg` | Added in historical Lavaland commit without source/license metadata | Replaced by traced `/tg/` `invoke_general.ogg`; binary removed. |
| Mercury and Oni sounds sourced from Pixabay | Individual Pixabay author/source URLs | Retained under the Pixabay Content License; the local `Custom` label refers to that license. |
| Cleaving Saw, Wildhunter knife and Demonic Jackhammer item/in-hand frames | `/tg/station` `5b2389c`; the exact DMI paths are recorded in `miner_rewards.rsi/meta.json` | Imported under CC-BY-SA-3.0 and converted without redrawing pixels. |
| Resurrection crystal, Ice Energy Crystal and animated Ice-block Talisman | `/tg/station` `5b2389c`; `icons/obj/mining.dmi` and `icons/obj/mining_zones/artefacts.dmi` | Imported under CC-BY-SA-3.0; DMI animation timing retained in RSI metadata. |
| Demonic Frost Miner phases | `/tg/station` `5b2389c`; `icons/mob/simple/icemoon/icemoon_monsters.dmi` | All four directions for both phases imported under CC-BY-SA-3.0. |
| Godslayer armour and helm | `/tg/station` `5b2389c`; object and worn clothing DMIs listed in each RSI | Item and four-direction worn frames imported under CC-BY-SA-3.0. |
| Cursed ice hiking boots | `/tg/station` `5b2389c`; `icons/obj/clothing/shoes.dmi` and `icons/mob/clothing/feet.dmi` | Imported under CC-BY-SA-3.0 after tracing the cursed subtype's inherited `iceboots` state. |
| H.E.C.K. corpse consumption and Blood Contract behavior | Paradise `127bd7f`; `bubblegum_loot.dm` | No DM copied. Mechanics were reimplemented in C# ECS under Whiskey's AGPL-compatible source tree; the contract reuses an already-attributed Goobstation scroll RSI instead of importing duplicate media. |
| Mercury Ether Drinker and Paradox Canceller sprites | Goobstation PR #6542, head `6d0a4d66`; `_Lavaland/Objects/Devices/mercury.rsi` by OnsenCapy | Existing CC-BY-SA-3.0 RSI retained; the previously unused licensed states are now backed by a native Whiskey ECS implementation. No upstream C# copied. |
| Radiant Shield sprite family | Goobstation PR #6542, head `6d0a4d66`; Citadel e-shield hueshift and tweaks by OnsenCapy | Imported exact RSI and metadata under CC-BY-SA-3.0. |
| Radiant MODsuit control, helmet, gauntlet, chest and boot sprite families | Goobstation PR #6542, head `6d0a4d66`; original art by OnsenCapy | Imported six exact RSI directories and their metadata under CC-BY-SA-3.0. |
| Mercury reward behavior, construction and TechWeb chain | Goobstation PR #6542, head `6d0a4d66`, used as the design reference | Ether Drinker, Paradox Canceller, Radiant Shield and MODsuit behaviors were independently implemented/adapted against Whiskey's current ECS, Trauma body and MODsuit APIs. Construction and research prototypes were rewritten for Whiskey's material economy. |
| Childish Oni boss, Nameless Sword and zukin sprites | Goobstation PR #6734, head `4dea43a96c`; original art by OnsenCapy / @NamelessName338 | Existing exact CC-BY-SA-3.0 media retained. The closed, unmerged state of the PR does not supersede its per-file redistribution license. |
| Childish Oni dress, hands, shoes and obi sprite families | Goobstation PR #6734, head `4dea43a96c`; original art by OnsenCapy / @NamelessName338 | Imported exact RSI directories and metadata under CC-BY-SA-3.0. |
| Childish Oni animated magma firewall | `/tg/station` `fb4faf1477de8c7ed5561097116763d6bb089036`, carried by Goobstation PR #6734 | Imported exact animation and metadata under CC-BY-SA-3.0; Whiskey uses a native temporary-field prototype. |
| Childish Oni soundtrack `childishoni.ogg` | Sonican, [Pixabay source](https://pixabay.com/music/upbeat-action-fight-239712/) | Existing audio retained under the Pixabay Content License recorded by its `attributions.yml`. |
| Childish Oni attacks, phase dialogue, harvest and reward behavior | Goobstation PR #6734, head `4dea43a96c`, used as the design reference | No upstream C# was copied. Attacks and dialogue scheduling are native Whiskey ECS implementations; item and encounter prototypes were rewritten for Whiskey's current APIs and economy. |
| Retired Whiskey harvest item atlas | Generated specifically for Whiskey Station with OpenAI image generation on 2026-08-12 | Removed. Active states now use attributed media already shipped by Whiskey: Goobstation bloody membrane; `/tg/` dragon bone, adamantine ingot, bubblegum, vocal cords and Legion core; SS14 module and silk families; Trauma reflectplate; Shitmed CC0 tissue sample; and the CC0 gravity anomaly core. Reagents use standard dynamic chemistry containers and carcass reservoirs. |
| Dimensional cargo manipulator and trophy vault | Whiskey-native prototypes using crate artwork already shipped by the project | No external media was imported. Capacity manipulation, component whitelisting and access control use the current SS14 ECS APIs, with no cross-round persistence. |
| Sacred Flame spellbook and action | `/tg/station` Ash Drake reward used only as a mechanics reference; book/action icon layers already shipped by Whiskey/Wega | BYOND/DM was not copied. The area ignition is a native ECS action, and the composed book art introduces no new external media. |

## Upstream licensing routes

| Upstream | Media rule used by this expansion |
| --- | --- |
| /tg/station, BeeStation, SPLURT | Import per-file media under CC-BY-SA-3.0 unless a narrower notice overrides it. |
| Paradise | Default media is CC-BY-SA-3.0; `icons/goonstation`, `sound/goonstation` and TGMC exceptions retain their NC licenses. |
| Goonstation | CC-BY-NC-SA-3.0 US. |
| Goob PR 6542 / 6734 | Use each RSI/audio metadata from the PR; most original OnsenCapy art is CC-BY or CC-BY-SA. |
| CEV Eris | Import only after the target file is traced to an explicit repository, directory or per-file license. |

The inventory will grow with each imported reward, environment, sound and
equipment family.
