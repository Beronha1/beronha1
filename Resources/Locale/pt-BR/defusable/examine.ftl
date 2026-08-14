# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

defusable-examine-defused = {CAPITALIZE(THE($name))} está [color=lime]desativado[/color].
defusable-examine-live = {CAPITALIZE(THE($name))} está [color=red]marcando[/color] e tem [color=red]{$time}[/color] segundos restantes.
defusable-examine-live-display-off = {CAPITALIZE(THE($name))} está [color=red]marcando[/color] e o cronômetro parece estar desligado.
defusable-examine-inactive = {CAPITALIZE(THE($name))} está [color=lime]inativo[/color], mas ainda pode ser armado.
defusable-examine-bolts= Os parafusos estão {$down ->
[true] [color=red]para baixo[/color]
*[false] [color=green]acima[/color]
}.
