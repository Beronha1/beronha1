# SPDX-FileCopyrightText: 2024 Celene <4323352+CuteMoonGod@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Hmeister <nathan.springfredfoxbon4@gmail.com>
# SPDX-FileCopyrightText: 2024 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Veritius <veritiusgaming@gmail.com>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

execution-verb-name = Executar
execution-verb-message = Use sua arma para executar alguém.

# All the below localisation strings have access to the following variables
# attacker (the person committing the execution)
# victim (the person being executed)
# weapon (the weapon used for the execution)

execution-popup-melee-initial-internal = Você aponta {THE($weapon)} para a garganta de {THE($victim)}.
execution-popup-melee-initial-external = { CAPITALIZE(THE($attacker)) } aponta {POSS-ADJ($attacker)} {$weapon} para a garganta de {THE($victim)}.
execution-popup-melee-complete-internal = Você corta a garganta de {THE($victim)}!
execution-popup-melee-complete-external = { CAPITALIZE(THE($attacker)) } corta a garganta de {THE($victim)}!

execution-popup-self-initial-internal = Você aponta {THE($weapon)} para a sua própria garganta.
execution-popup-self-initial-external = { CAPITALIZE(THE($attacker)) } aponta {POSS-ADJ($attacker)} {$weapon} para sua própria garganta.
execution-popup-self-complete-internal = Você corta a própria garganta!
execution-popup-self-complete-external = { CAPITALIZE(THE($attacker)) } corta a própria garganta!

