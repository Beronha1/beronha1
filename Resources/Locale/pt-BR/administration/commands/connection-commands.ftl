# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## Strings for the "grant_connect_bypass" command.

cmd-grant_connect_bypass-desc = Permite temporariamente que um usuario ignore as verificacoes de conexao normais.
cmd-grant_connect_bypass-help = Uso: grant_connect_bypass <usuario> [duracao minutos]
    Permite temporariamente que um usuario ignore as restricoes de conexao.
    A ignorancia so funciona neste servidor e expira em (por padrao) 1 hora.
    Eles poderao entrar independentemente de whitelist, panic bunker ou limite de jogadores.

cmd-grant_connect_bypass-arg-user = <usuario>
cmd-grant_connect_bypass-arg-duration = [duracao minutos]

cmd-grant_connect_bypass-invalid-args = Esperado 1 ou 2 argumentos
cmd-grant_connect_bypass-unknown-user = Nao foi possivel encontrar o usuario '{$user}'
cmd-grant_connect_bypass-invalid-duration = Duracao invalida '{$duration}'

cmd-grant_connect_bypass-success = Ignorar verificacao adicionado com sucesso para '{$user}'
