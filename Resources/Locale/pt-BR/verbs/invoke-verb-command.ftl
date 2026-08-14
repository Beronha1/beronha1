# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Localization used for the invoke verb command.
# Mostly help + error messages.

invoke-verb-command-description =Invoca um verbo com o nome dado em uma entidade, com a entidade do jogador
invoke-verb-command-help = invocarverbo <playerUid | "self"> <targetUid> <verbName | "interaction" | "activation" | "alternative">

invoke-verb-command-invalid-args = invocarverb leva 2 argumentos.

invoke-verb-command-invalid-player-uid = O uid do jogador não pôde ser analisado ou "self" não foi passado.
invoke-verb-command-invalid-target-uid = O UID de destino não pôde ser analisado.

invoke-verb-command-invalid-player-entity = O uid do jogador fornecido não corresponde a uma entidade válida.
invoke-verb-command-invalid-target-entity = O UID de destino fornecido não corresponde a uma entidade válida.

invoke-verb-command-success = Verbo invocado '{ $verb }' em { $target } com { $player } como usuário.

invoke-verb-command-verb-not-found = Não foi possível encontrar o verbo { $verb } em { $target }.

