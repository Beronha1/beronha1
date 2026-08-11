# SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
# SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Secret stash component. Stuff like potted plants, comfy chair cushions, etc...

comp-secret-stash-action-hide-success =Você esconde { THE($item) } no {$stashname}.
comp-secret-stash-action-hide-container-not-empty = Já tem alguma coisa aqui!?
comp-secret-stash-action-hide-item-too-big= { CAPITALIZE(THE($item)) } é muito grande para caber no {$stashname}.
comp-secret-stash-action-get-item-found-something = Havia algo dentro do {$stashname}!
comp-secret-stash-on-examine-found-hidden-item = Há algo escondido dentro do {$stashname}!
comp-secret-stash-on-destroyed-popup = Algo cai do {$stashname}!

### Verbs
comp-secret-stash-verb-insert-into-stash = Item escondido
comp-secret-stash-verb-insert-message-item-already-inside = Já existe um item dentro de {$stashname}.
comp-secret-stash-verb-insert-message-no-item = Oculte { THE($item) } no {$stashname}.
comp-secret-stash-verb-take-out-item = Pegar item
comp-secret-stash-verb-take-out-message-something = Retire o conteúdo de {$stashname}.
comp-secret-stash-verb-take-out-message-nothing = Não há nada dentro de {$stashname}.

comp-secret-stash-verb-close = Fechar
comp-secret-stash-verb-cant-close = Você não pode fechar o {$stashname} com isso.
comp-secret-stash-verb-open = Abrir

### Stash names
secret-stash-plant = plantar
secret-stash-toilet = cisterna de banheiro
secret-stash-plushie = pelúcia
secret-stash-cake = bolo
