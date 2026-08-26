# SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

limited-charges-charges-remaining= {$charges ->
    [one] It has [color=fuchsia]{$charges}[/color] carga restante.
    *[other] It has [color=fuchsia]{$charges}[/color] cargas restantes.
}

limited-charges-no-charges = Não há cargas restantes!

limited-charges-max-charges = Está com cobranças [color=green]máximas[/color].
limited-charges-recharging= {$seconds ->
    [one] There is [color=yellow]{$seconds}[/color] segundo restante até a próxima carga.
    *[other] There are [color=yellow]{$seconds}[/color] segundos restantes até a próxima carga.
}
