# SPDX-FileCopyrightText: 2023 Guillaume E <262623+quatre@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Vasilis <vascreeper@yahoo.com>
# SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Hannah Giovanna Dawson <karakkaraz@gmail.com>
# SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

analysis-console-menu-title =Console de análise Mark 3 de amplo espectro
analysis-console-server-list-button = Servidor
analysis-console-extract-button = Extrair pontos

analysis-console-info-no-scanner = Nenhum analisador conectado! Conecte um usando uma multiferramenta.
analysis-console-info-no-artifact = Nenhum artefato presente! Coloque um no bloco para visualizar as informações do nó.
analysis-console-info-ready = Sistemas operacionais. Pronto para digitalizar.

analysis-console-no-node = Selecione o nó para visualizar
analysis-console-info-id = [font="Monospace" size=11]ID:[/font]
analysis-console-info-id-value = [font="Monospace" size=11][color=yellow]{$id}[/color][/font]
analysis-console-info-class = [font="Monospace" size=11]Classe:[/font]
analysis-console-info-class-value = [font="Monospace" size=11]{$class}[/font]
analysis-console-info-locked = [font="Monospace" size=11]Status:[/font]
analysis-console-info-locked-value= [font="Monospace" size=11][color={ $state ->
    [0] red]Bloqueado
    [1] lime] Desbloqueado
    *[2] plum]Ativo
}[/color][/font]
analysis-console-info-durability = [font="Monospace" size=11]Durabilidade:[/font]
analysis-console-info-durability-value = [font="Monospace" size=11][color={$color}]{$current}/{$max}[/color][/font]
analysis-console-info-effect = [font="Monospace" size=11]Efeito:[/font]
analysis-console-info-effect-value= [font="Monospace" size=11][color=gray]{ $state ->
    [true] {$info}
    *[false] Desbloqueie nós para obter informações
}[/color][/font]
analysis-console-info-trigger = [font="Monospace" size=11]Acionadores:[/font]
analysis-console-info-triggered-value = [font="Monospace" size=11][color=gray]{$triggers}[/color][/font]
analysis-console-info-scanner = Digitalizando...
analysis-console-info-scanner-paused = Pausado.
analysis-console-progress-text= {$seconds ->
    [one] T-{$seconds} segundo
    *[other] T-{$seconds} segundos
}

analysis-console-extract-value = [font="Monospace" size=11][color=orange]Nó {$id} (+{$value})[/color][/font]
analysis-console-extract-none = [font="Monospace" size=11][color=orange] Nenhum nó desbloqueado tem pontos restantes para extrair [/color][/font]
analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Pesquisa total: {$value}[/color][/font]

analyzer-artifact-extract-popup = A energia brilha na superfície do artefato!
