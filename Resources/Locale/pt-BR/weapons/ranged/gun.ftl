# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 PixelTK <85175107+PixelTheKermit@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Errant <35878406+errant@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 MendaxxDev <153332064+MendaxxDev@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 TaralGit <76408146+TaralGit@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 and_a <and_a@DESKTOP-RJENGIR>
# SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later


gun-selected-mode-examine =O modo de disparo selecionado atualmente é [color={$color}]{$mode}[/color].
gun-fire-rate-examine = A taxa de tiro é [color={$color}]{$fireRate}[/color] por segundo.
gun-selector-verb = Alterar para {$mode}
gun-selected-mode = Selecionado {$mode}
gun-disabled = Você não pode usar armas!
gun-set-fire-mode-examine = Defina como [color=yellow]{$mode}[/color].
gun-set-fire-mode-popup = Alterado para {$mode}
gun-magazine-whitelist-fail = Isso não cabe na arma!
gun-magazine-fired-empty = Não sobrou munição!

# SelectiveFire
gun-SemiAuto = semi-automático
gun-Burst = estourar
gun-FullAuto = totalmente automático

# BallisticAmmoProvider
gun-ballistic-cycle = Ciclo
gun-ballistic-cycled = Ciclo
gun-ballistic-cycled-empty = Ciclo (vazio)
gun-ballistic-transfer-invalid= {CAPITALIZE(THE($ammoEntity))} não cabe dentro de {THE($targetEntity)}!
gun-ballistic-transfer-empty= {CAPITALIZE(THE($entity))} está vazio.
gun-ballistic-transfer-target-full= {CAPITALIZE(THE($entity))} já está totalmente carregado.

# CartridgeAmmo
gun-cartridge-spent = É [color=red]gasto[/color].
gun-cartridge-unspent = É [color=lime]não gasto[/color].

# BatteryAmmoProvider
gun-battery-examine = Tem carga suficiente para fotos [color={$color}]{$count}[/color].

# CartridgeAmmoProvider
gun-chamber-bolt-ammo = Arma não aparafusada
gun-chamber-bolt = O parafuso é [color={$color}]{$bolt}[/color].
gun-chamber-bolt-closed =Parafuso fechado
gun-chamber-bolt-opened = Parafuso aberto
gun-chamber-bolt-close = Fechar parafuso
gun-chamber-bolt-open = Parafuso aberto
gun-chamber-bolt-closed-state = aberto
gun-chamber-bolt-open-state = fechado
gun-chamber-rack = Prateleira

# MagazineAmmoProvider
gun-magazine-examine = Tem [color={$color}]{$count}[/color] fotos restantes.

# RevolverAmmoProvider
gun-revolver-empty = Revólver vazio
gun-revolver-full = Revólver cheio
gun-revolver-insert = Inserido
gun-revolver-spin = Revólver giratório
gun-revolver-spun = Fiado
gun-speedloader-empty = Speedloader vazio

# GunSpreadModifier
examine-gun-spread-modifier-reduction = O spread foi reduzido em [color=yellow]{$percentage}%[/color].
examine-gun-spread-modifier-increase = O spread foi aumentado em [color=yellow]{$percentage}%[/color].
