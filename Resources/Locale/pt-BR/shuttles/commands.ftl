# SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# FTLdiskburner
cmd-ftldisk-desc = Cria um disco de coordenadas FTL para navegar até o mapa em que a EntityID informada está
cmd-ftldisk-help= ftldisk [ID da entidade]

cmd-ftldisk-no-transform = A entidade {$destination} não possui componente Transform!
cmd-ftldisk-no-map = A entidade {$destination} não possui mapa!
cmd-ftldisk-no-map-comp = A entidade {$destination} está de alguma forma no mapa {$map} sem componente de mapa.
cmd-ftldisk-map-not-init = A entidade {$destination} está no mapa {$map}, que não está inicializado! Verifique se é seguro inicializar e depois inicialize o mapa primeiro, ou os jogadores ficarão presos no lugar!
cmd-ftldisk-map-paused = A entidade {$desintation} está no mapa {$map} que está pausado! Por favor, despausar o mapa primeiro ou os jogadores ficarão presos no lugar.
cmd-ftldisk-planet = A entidade {$desintation} está no mapa do planeta {$map} e precisará de um ponto FTL. Ele pode já existir.
cmd-ftldisk-already-dest-not-enabled = A entidade {$destination} está no mapa {$map} que já tem um FTLDestinationComponent, mas não está habilitado! Configure isso manualmente por segurança.
cmd-ftldisk-requires-ftl-point = A entidade {$destination} está no mapa {$map}, que exige um ponto FTL para viajar! Ele pode já existir.

cmd-ftldisk-hint = ID de rede do mapa
