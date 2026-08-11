# SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cmd-atvrange-desc = Define o alcance de debug de atmosfera (como dois floats, inicio [red] e fim [blue])
cmd-atvrange-help = Uso: {$command} <start> <end>
cmd-atvrange-error-start = Float START invalido
cmd-atvrange-error-end = Float END invalido
cmd-atvrange-error-zero = A escala nao pode ser zero, pois isso causaria uma divisao por zero no AtmosDebugOverlay.

cmd-atvmode-desc = Define o modo de debug da atmosfera. Isso reiniciara automaticamente a escala.
cmd-atvmode-help = Uso: {$command} <TotalMoles/GasMoles/Temperature> [<gas ID (para GasMoles)>]
cmd-atvmode-error-invalid = Modo invalido
cmd-atvmode-error-target-gas = Um gas alvo precisa ser fornecido para este modo.
cmd-atvmode-error-out-of-range = Gas ID nao pode ser interpretado ou esta fora do alcance.
cmd-atvmode-error-info = Nenhuma informacao adicional e necessaria para este modo.

cmd-atvcbm-desc = Altera de vermelho/verde/azul para escala de cinza
cmd-atvcbm-help = Uso: {$command} <true/false>
cmd-atvcbm-error = Flag invalida
