# SPDX-License-Identifier: AGPL-3.0-or-later

# Tela de carregamento

replay-loading = Carregando ({$cur}/{$total})
replay-loading-reading = Lendo arquivos
replay-loading-processing = Processando arquivos
replay-loading-spawning = Criando entidades
replay-loading-initializing = Inicializando entidades
replay-loading-starting = Iniciando entidades
replay-loading-failed = Falha ao carregar o replay. Erro:
                        {$reason}
replay-loading-retry = Tentar carregar tolerando mais exceções — pode causar bugs!
replay-loading-cancel = Cancelar

# Menu principal

replay-menu-subtext = Cliente de replays
replay-menu-load = Carregar replay selecionado
replay-menu-select = Selecionar um replay
replay-menu-open = Abrir pasta de replays
replay-menu-none = Nenhum replay encontrado.

# Informações do replay

replay-info-title = Informações do replay
replay-info-none-selected = Nenhum replay selecionado
replay-info-invalid = [color=red]O replay selecionado é inválido[/color]
replay-info-info = {"["}color=gray]Selecionado:[/color]  {$name} ({$file})
                   {"["}color=gray]Data:[/color]   {$time}
                   {"["}color=gray]ID da rodada:[/color]   {$roundId}
                   {"["}color=gray]Duração:[/color]   {$duration}
                   {"["}color=gray]ID do fork:[/color]   {$forkId}
                   {"["}color=gray]Versão:[/color]   {$version}
                   {"["}color=gray]Engine:[/color]   {$engVersion}
                   {"["}color=gray]Hash de tipos:[/color]   {$hash}
                   {"["}color=gray]Hash de componentes:[/color]   {$compHash}

# Janela de seleção

replay-menu-select-title = Selecionar replay

# Verbos

replay-verb-spectate = Observar

# Comandos

cmd-replay-spectate-help = replay_spectate [entidade opcional]
cmd-replay-spectate-desc = Anexa ou desanexa o jogador local de uma entidade pelo UID.
cmd-replay-spectate-hint = EntityUid opcional
