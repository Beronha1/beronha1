# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

point-scoreboard-winner =O vencedor foi [color=lime]{$player}![/color]
point-scoreboard-header = [bold]Placar[/bold]
point-scoreboard-list= {$place}. [bold][color=cyan]{$place}[/color][/bold] marcou [color=yellow]{$place ->
    [one] {$points} ponto
    *[other] {$points} pontos
}.[/color]
