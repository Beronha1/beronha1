# SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Tunguso4ka <71643624+Tunguso4ka@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cmd-jobwhitelist-job-does-not-exist = O cargo {$job} nao existe.
cmd-jobwhitelist-player-not-found = Jogador {$player} nao encontrado.
cmd-jobwhitelist-hint-player= [jogador]
cmd-jobwhitelist-hint-job= [trabalho]

cmd-jobwhitelistadd-desc = Permite que um jogador jogue um cargo na whitelist.
cmd-jobwhitelistadd-help = Uso: jobwhitelistadd <username> <job>
cmd-jobwhitelistadd-already-whitelisted = {$player} ja esta na whitelist para jogar como {$jobId} .({$jobName}).
cmd-jobwhitelistadd-added = Adicionado {$player} na whitelist de {$jobId} ({$jobName}).

cmd-jobwhitelistget-desc = Exibe todos os cargos para os quais um jogador foi colocado na whitelist.
cmd-jobwhitelistget-help = Uso: jobwhitelistget <username>
cmd-jobwhitelistget-whitelisted-none = O jogador {$player} nao esta em whitelist para nenhum cargo.
cmd-jobwhitelistget-whitelisted-for = "O jogador {$player} esta na whitelist para:
{$jobs}"

cmd-jobwhitelistremove-desc = Remove a capacidade de um jogador jogar em um cargo na whitelist.
cmd-jobwhitelistremove-help = Uso: jobwhitelistremove <username> <job>
cmd-jobwhitelistremove-was-not-whitelisted = {$player} nao estava na whitelist para jogar como {$jobId} ({$jobName}).
cmd-jobwhitelistremove-removed = Removido {$player} da whitelist de {$jobId} ({$jobName}).
