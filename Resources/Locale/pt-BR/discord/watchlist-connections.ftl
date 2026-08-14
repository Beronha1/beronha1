# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Palladinium <patrick.chieppe@hotmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

discord-watchlist-connection-header =
    { $players ->
        [one] {$players} jogador em uma lista de observação tem
        *[other] {$players} jogadores em uma lista de observação têm
    } conectado a {$serverName}

discord-watchlist-connection-entry = - {$playerName} com mensagem "{$message}"{ $expiry ->
        [0] {""}
        *[other] {" "}(expira em <t:__TOK_0__:R>)
    }{ $otherWatchlists ->
        [0] {""}
        [one] {" "}e {$otherWatchlists} outra lista de observação
        *[other] {" "}e {$otherWatchlists} outras listas de observação
    }

