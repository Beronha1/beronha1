# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 moonheart08 <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Morber <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
# SPDX-FileCopyrightText: 2023 crazybrain23 <44417085+crazybrain23@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### for technical and/or system messages

## General

shell-command-success = Comando executado com sucesso
shell-invalid-command = Comando inválido.
shell-invalid-command-specific = Comando inválido: {$commandName}
shell-can-only-run-from-pre-round-lobby = Você só pode executar este comando enquanto o jogo estiver Não lobby pré-round.
shell-can-only-run-while-round-is-active = Você só pode executar este comando enquanto o round estiver ativo.
shell-cannot-run-command-from-server = Você não pode executar este comando do servidor.
shell-only-players-can-run-this-command = Apenas jogadores podem executar este comando.
shell-must-be-attached-to-entity = Você precisa estar preso a uma entidade para executar este comando.
shell-must-have-body = Você precisa ter um corpo para executar este comando.

shell-unknown-error = Ocorreu um erro desconhecido.

## Arguments

shell-need-exactly-one-argument = Precisa de exatamente um argumento.
shell-wrong-arguments-number-need-specific = Precisam de {$properAmount} argumentos, foram recebidos {$currentAmount}.
shell-argument-must-be-number = O argumento deve ser um número.
shell-argument-must-be-boolean = O argumento deve ser um booleano.
shell-wrong-arguments-number = Número de argumentos incorreto.
shell-need-between-arguments = Precisa de {$lower} a {$upper} argumentos!
shell-need-minimum-arguments = Precisa de pelo menos {$minimum} argumentos!
shell-need-minimum-one-argument = Precisa de pelo menos um argumento!
shell-need-exactly-zero-arguments = Este comando não recebe argumentos.

shell-argument-uid= EntidadeUid

## Guards

shell-missing-required-permission = Você precisa de {$perm} para este comando!
shell-entity-is-not-mob = A entidade de destino não é um mob!
shell-invalid-entity-id = ID de entidade inválido.
shell-invalid-grid-id = ID de grade inválido.
shell-invalid-map-id = ID de mapa inválido.
shell-invalid-entity-uid = EntityUid não é um uid de entidade válido
shell-invalid-bool = Booleano inválido.
shell-invalid-bool-value = Booleano inválido: '{$value}'
shell-entity-uid-must-be-number = EntityUid deve ser um número.
shell-could-not-find-entity = Não foi possível encontrar a entidade {$entity}
shell-could-not-find-entity-with-uid = Não foi possível encontrar entidade com o uid {$uid}
shell-entity-with-uid-lacks-component = Entidade com uid {$uid} não tem o componente {INDEFINITE($componentName)} {$componentName}
shell-entity-target-lacks-component = Entidade de destino não tem o componente {INDEFINITE($componentName)} {$componentName}
shell-invalid-color-hex = Hex de cor inválido!
shell-target-player-does-not-exist = Jogador alvo não existe!
shell-target-entity-does-not-have-message = Entidade alvo não tem {INDEFINITE($missing)} {$missing}!
shell-timespan-minutes-must-be-correct = {$span} não é um intervalo de tempo em minutos válido.
shell-argument-must-be-prototype = O argumento {$index} deve ser um {LOC($prototypeName)}!
shell-argument-number-must-be-between = O argumento {$index} deve ser um número entre {$lower} e {$upper}!
shell-argument-station-id-invalid = O argumento {$index} deve ser um ID de estação válido!
shell-argument-map-id-invalid = O argumento {$index} deve ser um ID de mapa válido!
shell-argument-number-invalid = O argumento {$index} deve ser um número válido!
shell-argument-chat-invalid = O argumento {$index} deve ser um chat válido!

# Hints
shell-argument-username-hint = <username>
shell-argument-username-optional-hint= [nome de usuário]
