# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
# SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Mervill <mervills.email@gmail.com>
# SPDX-FileCopyrightText: 2023 alexkar598 <25136265+alexkar598@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Repo <47093363+Titian3@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Displayed as initiator of vote when no user creates the vote
ui-vote-initiator-server =O servidor

## Default.Votes

ui-vote-restart-title = Reiniciar rodada
ui-vote-restart-succeeded = O reinício da votação foi bem-sucedido.
ui-vote-restart-failed = Falha ao reiniciar a votação (é necessário {TOSTRING($ratio, "P0") }).
ui-vote-restart-fail-not-enough-ghost-players = Falha na votação de reinicialização: é necessário um mínimo de { $ghostPlayerRequirement }% de jogadores fantasmas para iniciar uma votação de reinicialização. Atualmente, não há jogadores fantasmas suficientes.
ui-vote-restart-yes = Sim
ui-vote-restart-no = Não
ui-vote-restart-abstain = Abster-se

ui-vote-gamemode-title = Próximo modo de jogo
ui-vote-gamemode-tie = Empate para votação no modo de jogo! Escolhendo... { $picked }
ui-vote-gamemode-win= { $winner } ganhou a votação do modo de jogo!

ui-vote-map-title = Próximo mapa
ui-vote-map-tie = Empate para votação no mapa! Escolhendo... { $picked }
ui-vote-map-win= { $winner } ganhou a votação do mapa!
ui-vote-map-notlobby = A votação em mapas só é válida no lobby pré-rodada!
ui-vote-map-notlobby-time = A votação em mapas só é válida no lobby pré-rodada com { $time } restantes!
ui-vote-map-invalid= {$winner} tornou-se inválido após a votação do mapa! Não será selecionado!

# Votekick votes
ui-vote-votekick-unknown-initiator = Um jogador
ui-vote-votekick-unknown-target = Jogador desconhecido
ui-vote-votekick-title= { $initiator } chamou um votekick para o usuário: { $targetEntity }. Motivo: { $reason}
ui-vote-votekick-yes = Sim
ui-vote-votekick-no = Não
ui-vote-votekick-abstain = Abster-se
ui-vote-votekick-success = Votekick para { $target } foi bem-sucedido. Motivo da votação: { $reason}
ui-vote-votekick-failure =A votação para { $target } falhou. Motivo da votação: { $reason}
ui-vote-votekick-not-enough-eligible = Não há eleitores qualificados on-line suficientes para iniciar uma votação: { $voters }/{ $requirement }
ui-vote-votekick-server-cancelled = Votekick para { $target } foi cancelado pelo servidor.

