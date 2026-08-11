# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

generator-clogged = {CAPITALIZE(THE($generator))} desliga abruptamente!

portable-generator-verb-start = Iniciar gerador
portable-generator-verb-start-msg-unreliable = Iniciar o gerador. Isso pode levar algumas tentativas.
portable-generator-verb-start-msg-reliable = Iniciar o gerador.
portable-generator-verb-start-msg-unanchored = O gerador precisa estar ancorado primeiro!
portable-generator-verb-stop = Parar gerador
portable-generator-start-fail = Você puxa o cabo, mas não ligou.
portable-generator-start-success = Você puxa o cabo, e ele entra em funcionamento.

portable-generator-ui-title = Gerador portátil
portable-generator-ui-status-stopped = Parado:
portable-generator-ui-status-starting = Iniciando:
portable-generator-ui-status-running = Em operação:
portable-generator-ui-start = Iniciar
portable-generator-ui-stop = Parar
portable-generator-ui-target-power-label = Potência alvo (kW):
portable-generator-ui-efficiency-label = Eficiência:
portable-generator-ui-fuel-use-label = Consumo de combustível:
portable-generator-ui-fuel-left-label = Combustível restante:
portable-generator-ui-clogged = Contaminantes detectados no tanque de combustível!
portable-generator-ui-eject = Ejetar
portable-generator-ui-eta= (~{ $minutes } min)
portable-generator-ui-unanchored = Não ancorado
portable-generator-ui-current-output = Saída atual: {$voltage}
portable-generator-ui-network-stats = Rede:
portable-generator-ui-network-stats-value= { POWERWATTS($supply) } / { POWERWATTS($load) }
portable-generator-ui-network-stats-not-connected = Não conectado

power-switchable-generator-examine = A saída de energia está definida para {$voltage}.
power-switchable-generator-switched = Saída alternada para {$voltage}!

power-switchable-voltage= { $voltage ->
    [HV] [color=orange]HV[/color]
    [MV] [color=yellow]MV[/color]
    *[LV] [color=green]LV[/color]
}
power-switchable-switch-voltage = Trocar para {$voltage}

fuel-generator-verb-disable-on = Desligue o gerador primeiro!
