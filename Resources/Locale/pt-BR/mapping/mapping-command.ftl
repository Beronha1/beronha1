# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cmd-mapping-desc =Crie ou carregue um mapa e teletransporte você para ele.
cmd-mapping-help = Uso: mapeamento [MapID] [Caminho] [Grid]
cmd-mapping-server = Somente jogadores podem usar este comando.
cmd-mapping-error = Ocorreu um erro ao criar o novo mapa.
cmd-mapping-try-grid = Falha ao carregar o arquivo como mapa. Tentando carregar o arquivo como uma grade...
cmd-mapping-success-load = Mapa não inicializado criado do arquivo {$path} com id {$mapId}.
cmd-mapping-success-load-grid = Grade não inicializada carregada do arquivo {$path} em um novo mapa com id {$mapId}.
cmd-mapping-success = Mapa não inicializado criado com id {$mapId}.
cmd-mapping-warning = AVISO: O servidor está usando uma compilação de depuração. Você está arriscando perder suas alterações.


# duplicate text from engine load/save map commands.
# I CBF making this PR depend on that one.
cmd-mapping-failure-integer= {$arg} não é um número inteiro válido.
cmd-mapping-failure-float= {$arg} não é um ponto flutuante válido.
cmd-mapping-failure-bool= {$arg} não é um booleano válido.
cmd-mapping-nullspace = Você não pode carregar no mapa 0.
cmd-hint-mapping-id = [ID do mapa]
cmd-mapping-hint-grid = [Grade]
cmd-hint-mapping-path = [Caminho]
cmd-mapping-exists = O mapa {$mapId} já existe.
