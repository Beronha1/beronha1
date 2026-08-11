# SPDX-FileCopyrightText: 2022 LittleBuilderJane <63973502+LittleBuilderJane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Myctai <108953437+Myctai@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
# SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
# SPDX-FileCopyrightText: 2024 strO0pwafel <153459934+strO0pwafel@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Commands
## Delay shuttle round end
cmd-delayroundend-desc = Interrompe o timer que encerra a rodada quando a nave de emergência sai da hiperespaço.
cmd-delayroundend-help = Uso: delayroundend
emergency-shuttle-command-round-yes = Rodada atrasada.
emergency-shuttle-command-round-no = Não foi possível atrasar o fim da rodada.

## Dock emergency shuttle
cmd-dockemergencyshuttle-desc = Chama a nave de emergência e acopla-a à estação... se possível.
cmd-dockemergencyshuttle-help = Uso: dockemergencyshuttle

## Launch emergency shuttle
cmd-launchemergencyshuttle-desc = Lança a nave de emergência antecipadamente, se possível.
cmd-launchemergencyshuttle-help = Uso: launchemergencyshuttle

# Emergency shuttle
emergency-shuttle-left = A nave de emergência deixou a estação. Estimativa de {$transitTime} segundos até a nave chegar ao CentComm.
emergency-shuttle-launch-time = A nave de emergência vai lançar em {$consoleAccumulator} segundos.
emergency-shuttle-docked = A nave de emergência acoplou em {$direction} da estação, {$location}. Ela partirá em {$time} segundos.{$extended}
emergency-shuttle-good-luck = A nave de emergência não consegue encontrar uma estação. Boa sorte.
emergency-shuttle-nearby = A nave de emergência não consegue encontrar uma doca de acoplamento válida. Ela entrou em {$direction} da estação, {$location}. Ela partirá em {$time} segundos.{$extended}
emergency-shuttle-extended = {" "}O tempo de lançamento foi estendido por circunstâncias incômodas.

# Emergency shuttle console popup / announcement
emergency-shuttle-console-no-early-launches = O lançamento antecipado está desativado
emergency-shuttle-console-auth-left = {$remaining} autorizações necessárias para a nave ser lançada antecipadamente.
emergency-shuttle-console-auth-revoked = Autorização de lançamento antecipado revogada, {$remaining} autorizações necessárias.
emergency-shuttle-console-denied = Acesso negado

# UI
emergency-shuttle-console-window-title = Console da nave de emergência
emergency-shuttle-ui-engines = MOTORES:
emergency-shuttle-ui-idle = Ocioso
emergency-shuttle-ui-repeal-all = Revogar tudo
emergency-shuttle-ui-early-authorize = Autorização de lançamento antecipado
emergency-shuttle-ui-authorize = AUTORIZAR
emergency-shuttle-ui-repeal = REVOGAR
emergency-shuttle-ui-authorizations = Autorizações
emergency-shuttle-ui-remaining = Restante: {$remaining}

# Map Misc.
map-name-centcomm = Comando Central
map-name-terminal = Terminal de chegada

