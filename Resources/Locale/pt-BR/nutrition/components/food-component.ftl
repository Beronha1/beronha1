# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 FoLoKe <36813380+FoLoKe@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2021 Remie Richards <remierichards@gmail.com>
# SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2024 Eris <erisfiregamer1@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later


### Interaction Messages

# When trying to eat food without the required utensil... but you gotta hold it
food-you-need-to-hold-utensil =Você precisa segurar {INDEFINITE($utensil)} {$utensil} para comer isso!

food-nom = Nome. {$flavors}
food-swallow = Você engole { THE($food) }. {$flavors}

food-has-used-storage = Você não pode comer { THE($food) } com um item armazenado dentro.

food-system-remove-mask = Você precisa tirar o {$entity} primeiro.

## System

food-system-you-cannot-eat-any-more = Você não pode mais comer!
food-system-you-cannot-eat-any-more-other= {CAPITALIZE(SUBJECT($target))} não posso comer mais!
food-system-try-use-food-is-empty= {CAPITALIZE(THE($entity))} está vazio!
food-system-wrong-utensil = Você não pode comer {THE($food)} com {INDEFINITE($utensil)} {$utensil}.
food-system-cant-digest = Você não consegue digerir {THE($entity)}!
food-system-cant-digest-other= {CAPITALIZE(SUBJECT($target))} não consegue digerir {THE($entity)}!

food-system-verb-eat = Coma

## Force feeding

food-system-force-feed= {CAPITALIZE(THE($user))} está tentando lhe dar algo!
food-system-force-feed-success= {CAPITALIZE(THE($user))} forçou você a comer alguma coisa! {$flavors}
food-system-force-feed-success-user = Você alimentou {THE($target)} com sucesso
