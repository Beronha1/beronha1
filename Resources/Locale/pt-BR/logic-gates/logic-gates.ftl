# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

logic-gate-examine =Atualmente é o portão {INDEFINITE($gate)} {$gate}.

logic-gate-cycle = Mudou para portão {INDEFINITE($gate)} {$gate}

power-sensor-examine = Atualmente está verificando o {$output ->
    [true] saída
    *[false] entrada
} bateria.
power-sensor-voltage-examine = Está verificando a rede de energia {$voltage}.

power-sensor-switch = Mudou para verificar o {$output ->
    [true] saída
    *[false] entrada
} bateria.
power-sensor-voltage-switch = Rede trocada para {$voltage}!


