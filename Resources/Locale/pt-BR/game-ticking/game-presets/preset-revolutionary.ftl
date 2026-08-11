# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
# SPDX-FileCopyrightText: 2023 coolmankid12345 <55817627+coolmankid12345@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 coolmankid12345 <coolmankid12345@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Killerqu00 <47712032+Killerqu00@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

## Rev Head

roles-antag-rev-head-name = Líder revolucionário
roles-antag-rev-head-objective = Seu objetivo é tomar a estação convertendo pessoas para sua causa e eliminando todos os membros do Comando.

## Trauma - rewrote
head-rev-role-greeting =
    Você é um Líder Revolucionário.
    Você foi encarregado de remover todo o Comando da estação por morte, exílio ou prisão.
    Você preparou os componentes necessários para construir uma forja industrial e produzir as ferramentas que precisa.
    Crie panfletos de propaganda com a impressora para converter a tripulação.
    Cuidado, suas máquinas são muito barulhentas. Você terá que esconder e defender sua base operacional.
    Viva a revolução!

## Trauma - rewrote
head-rev-briefing =
    Construa estruturas industriais de revolução.
    Tire todos os chefes de comando para tomar a estação.

head-rev-break-mindshield = O implante de blindagem mental foi destruído!

## Rev

roles-antag-rev-name = Revolucionário
roles-antag-rev-objective = Seu objetivo é garantir a segurança e seguir as ordens dos líderes revolucionários, ajudando-os a tomar a estação eliminando todos os membros do Comando.

rev-break-control = {$name} lembrou de sua verdadeira lealdade!

rev-role-greeting =
    Você é um revolucionário. Sua tarefa é proteger os líderes revolucionários e ajudá-los a tomar a estação.
    A revolução precisa trabalhar em conjunto para matar, conter ou converter todos os membros do Comando.
    Viva a revolução!

rev-briefing = Ajude os líderes revolucionários a matar, conter ou converter todos os membros do Comando para tomar a estação.

## General

rev-title = Revolucionários
rev-description = Revolucionários escondidos entre a tripulação estão tentando converter outros para sua causa e derrubar o Comando.

rev-not-enough-ready-players = Jogadores prontos insuficientes para iniciar o jogo. Havia {$readyPlayersCount} jogadores prontos de {$minimumPlayers} necessários. Não foi possível iniciar Revolucionários.
rev-no-one-ready = Nenhum jogador pronto! Não foi possível iniciar Revolucionários.
rev-no-heads = Não havia líderes revolucionários para serem selecionados. Não foi possível iniciar Revolucionários.

rev-won = Os líderes revolucionários sobreviveram e assumiram com sucesso o controle da estação.

rev-lost = Todos os líderes revolucionários morreram e o Comando sobreviveu.

rev-stalemate = Comando e líderes revolucionários morreram todos. Empate.

rev-reverse-stalemate = Comando e líderes revolucionários sobreviveram.

rev-headrev-count = {$initialCount ->
    [one] Havia um líder revolucionário:
    *[other] Havia {$initialCount} líderes revolucionários:
}

rev-headrev-name-user = [color=#5e9cff]{$name}[/color] ([color=gray]{$username}[/color]) converteu {$count} {$count ->
    [one] pessoa
    *[other] pessoas
}

rev-headrev-name = [color=#5e9cff]{$name}[/color] converteu {$count} {$count ->
    [one] pessoa
    *[other] pessoas
}

## Deconverted window

rev-deconverted-title = Desconvertido!
rev-deconverted-text =
    Como o último líder revolucionário morreu, a revolução acabou.

    Você não é mais revolucionário, então seja legal.
rev-deconverted-confirm = Confirmar

