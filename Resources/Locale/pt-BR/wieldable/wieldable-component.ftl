# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 RiceMar1244 <138547931+RiceMar1244@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Locale for wielding items; i.e. two-handing them

wieldable-verb-text-wield =Empunhar
wieldable-verb-text-unwield = Desenrolar

wieldable-component-successful-wield = Você empunha { THE($item) }.
wieldable-component-failed-wield = Você desempunha { THE($item) }.
wieldable-component-successful-wield-other= { CAPITALIZE(THE($user)) } empunha { THE($item) }.
wieldable-component-failed-wield-other= { CAPITALIZE(THE($user)) } desdobra { THE($item) }.
wieldable-component-blocked-wield= { CAPITALIZE(THE($blocker)) } impede você de empunhar { THE($item) }.

wieldable-component-no-hands = Você não tem mãos suficientes!
wieldable-component-not-enough-free-hands= {$number ->
    [one] Você precisa de mão livre para manejar { THE($item) }.
    *[other] Você precisa de { $number } mãos livres para empunhar { THE($item) }.
}
wieldable-component-not-in-hands= { CAPITALIZE(THE($item)) } não está em suas mãos!

wieldable-component-requires= { CAPITALIZE(THE($item))} deve ser empunhado!

gunwieldbonus-component-examine = Esta arma melhorou a precisão quando empunhada.

gunrequireswield-component-examine = Esta arma só pode ser disparada quando empunhada.
