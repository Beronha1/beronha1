# SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### Loc for the pneumatic cannon.

pneumatic-cannon-component-itemslot-name= Tanque de gás

## Shown when trying to fire, but no gas

pneumatic-cannon-component-fire-no-gas = { CAPITALIZE(THE($cannon)) } clica, mas não sai gás.

## Shown when changing power.

pneumatic-cannon-component-change-power = {$power ->
    [High] Você define o limitador para potência máxima. Parece um pouco poderoso demais...
    [Medium] Você define o limitador para potência média.
    *[Low] Você definiu o limitador para baixa potência.
}

## Shown when being stunned by having the power too high.

pneumatic-cannon-component-power-stun= A força pura de { THE($cannon) } te derruba!
