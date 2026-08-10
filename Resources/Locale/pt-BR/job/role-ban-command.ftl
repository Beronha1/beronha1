# SPDX-FileCopyrightText: 2026 AkkadianMerchant <https://github.com/AkkadianMerchant>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

cmd-roleban-desc = Bane um jogador de um cargo

cmd-roleban-help = Uso: roleban <name or user ID> <job> <reason> [duração em minutos, ignore ou deixe 0 para um ban permanente]

cmd-roleban-hint-4 = [duração em minutos, ignore ou deixe 0 para um ban permanente]

cmd-roleban-hint-duration-1 = Permanente

cmd-roleban-hint-duration-2 = 1 dia

cmd-roleban-hint-duration-3 = 3 dias

cmd-roleban-hint-duration-4 = 1 semana

cmd-roleban-hint-duration-5 = 2 semanas

cmd-roleban-hint-duration-6 = 1 mês

cmd-roleunban-desc = Remove o banimento de certo cargo de um jogador

cmd-roleunban-help = Uso: roleunban <role ban id>

cmd-rolebanlist-desc = Lista os banimentos de cargos dos jogadores

cmd-rolebanlist-help = Usage: <name or user ID> [incluir desbanidos]

cmd-rolebanlist-hint-2 = [incluir desbanidos]

cmd-roleban-minutes-parse = {$time} não é uma quantidade válida de minutos.\n{$help}

cmd-roleban-severity-parse = ${severity} não é uma gravidade válida\n{$help}.

cmd-roleban-arg-count = Quantidade inválida de argumentos.

cmd-roleban-job-parse = Trabalho {$job} não existe.

cmd-roleban-name-parse = Não foi possível encontrar um jogador com esse nome.

cmd-roleban-success =  {$target} foi banido para {$role} como a {$reason} {$length}.

cmd-roleban-inf = permanentemente

cmd-roleban-until =  até {$expires}

cmd-departmentban-desc = Bane um jogador das funções que compõem um departamento

cmd-departmentban-help = Uso: departmentban <name or user ID> <department> <reason> [duração em minutos, deixe de fora ou 0 para banimento permanente]
