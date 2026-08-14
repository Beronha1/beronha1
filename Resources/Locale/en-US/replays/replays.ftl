# SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Loading Screen

replay-loading = Loading ({$cur}/{$total})
replay-loading-reading = Reading files
replay-loading-processing = Processing files
replay-loading-spawning = Spawning entities
replay-loading-initializing = Initializing entities
replay-loading-starting = Starting entities
replay-loading-failed = Failed to load replay. Error:
                        {$reason}
replay-loading-retry = Try loading with more exception tolerance — may cause bugs!
replay-loading-cancel = Cancel

# Main Menu

replay-menu-subtext = Replay Client
replay-menu-load = Load selected replay
replay-menu-select = Select a replay
replay-menu-open = Open replay folder
replay-menu-none = No replays found.

# Main Menu Info Box

replay-info-title = Replay Information
replay-info-none-selected = No replay selected
replay-info-invalid = [color=red]Invalid replay selected[/color]
replay-info-info = {"["}color=gray]Selected:[/color]  {$name} ({$file})
                   {"["}color=gray]Time:[/color]   {$time}
                   {"["}color=gray]Round ID:[/color]   {$roundId}
                   {"["}color=gray]Duration:[/color]   {$duration}
                   {"["}color=gray]Fork ID:[/color]   {$forkId}
                   {"["}color=gray]Version:[/color]   {$version}
                   {"["}color=gray]Engine:[/color]   {$engVersion}
                   {"["}color=gray]Type hash:[/color]   {$hash}
                   {"["}color=gray]Component hash:[/color]   {$compHash}

# Replay selection window

replay-menu-select-title = Select Replay

# Replay related verbs

replay-verb-spectate = Spectate

# Commands

cmd-replay-spectate-help = replay_spectate [optional entity]
cmd-replay-spectate-desc = Attaches or detaches the local player to a given entity UID.
cmd-replay-spectate-hint = Optional EntityUid
