# SPDX-FileCopyrightText: 2022 Aru Moon <anton17082003@gmail.com>
# SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 MishaUnity <81403616+MishaUnity@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Phill101 <28949487+Phill101@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Phill101 <holypics4@gmail.com>
# SPDX-FileCopyrightText: 2024 ArchRBX <5040911+ArchRBX@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Kot <1192090+koteq@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 lapatison <100279397+lapatison@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Эдуард <36124833+Ertanic@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

device-pda-slot-component-slot-name-cartridge = Cartucho

default-program-name = Programa
notekeeper-program-name = Notas
nano-task-program-name = NanoTask
news-read-program-name = Não�cias da esta��o

crew-manifest-program-name = Manifesto da tripula��o
crew-manifest-cartridge-loading = Carregando ...
crew-manifest-cartridge-loading-failed = Falha ao carregar manifesto da tripula��o!

net-probe-program-name = NetProbe
net-probe-scan = {$device} analisado!
net-probe-label-name = Nome
net-probe-label-address = Endere�o
net-probe-label-frequency = Frequ�ncia
net-probe-label-network = Rede

log-probe-program-name = LogProbe
log-probe-scan = Logs baixados de {$device}!
log-probe-label-time = Hor�rio
log-probe-label-accessor = Acessado por
log-probe-label-number = #
log-probe-print-button = Imprimir logs
log-probe-printout-device = Dispositivo escaneado: {$name}
log-probe-printout-header = Logs recentes:
log-probe-printout-entry = #{$number} / {$time} / {$accessor}

astro-nav-program-name = AstroNav

med-tek-program-name = MedTek

# NanoTask cartridge

nano-task-ui-heading-high-priority-tasks =
{ $amount ->
        [zero] Nenhuma tarefa de alta prioridade
        [one] 1 tarefa de alta prioridade
       *[other] {$amount} tarefas de alta prioridade
    }
nano-task-ui-heading-medium-priority-tasks =
{ $amount ->
        [zero] Nenhuma tarefa de m�dia prioridade
        [one] 1 tarefa de m�dia prioridade
       *[other] {$amount} tarefas de m�dia prioridade
    }
nano-task-ui-heading-low-priority-tasks =
{ $amount ->
        [zero] Nenhuma tarefa de baixa prioridade
        [one] 1 tarefa de baixa prioridade
       *[other] {$amount} tarefas de baixa prioridade
    }
nano-task-ui-done = Conclu�do
nano-task-ui-revert-done = Desfazer
nano-task-ui-priority-low = Baixa
nano-task-ui-priority-medium = M�dia
nano-task-ui-priority-high = Alta
nano-task-ui-cancel = Cancelar
nano-task-ui-print = Imprimir
nano-task-ui-delete = Excluir
nano-task-ui-save = Salvar
nano-task-ui-new-task = Nova tarefa
nano-task-ui-description-label = Descri��o:
nano-task-ui-description-placeholder = Pegue algo importante
nano-task-ui-requester-label = Solicitante:
nano-task-ui-requester-placeholder = John Nanotrasen
nano-task-ui-item-title = Editar tarefa
nano-task-printed-description = [bold]Descri��o[/bold]: {$description}
nano-task-printed-requester = [bold]Solicitante[/bold]: {$requester}
nano-task-printed-high-priority = [bold]Prioridade[/bold]: [color=red]Alta[/color]
nano-task-printed-medium-priority = [bold]Prioridade[/bold]: M�dia
nano-task-printed-low-priority = [bold]Prioridade[/bold]: Baixa

# Wanted list cartridge
wanted-list-program-name = Lista de procurados
wanted-list-label-no-records = Tudo certo, cowboy
wanted-list-search-placeholder = Buscar por nome e status

wanted-list-age-label = [color=darkgray]Idade:[/color] [color=white]{$age}[/color]
wanted-list-job-label = [color=darkgray]Cargo:[/color] [color=white]{$job}[/color]
wanted-list-species-label = [color=darkgray]Esp�cie:[/color] [color=white]{$species}[/color]
wanted-list-gender-label = [color=darkgray]G�nero:[/color] [color=white]{$gender}[/color]

wanted-list-reason-label = [color=darkgray]Motivo:[/color] [color=white]{$reason}[/color]
wanted-list-unknown-reason-label = motivo desconhecido

wanted-list-initiator-label = [color=darkgray]Iniciador:[/color] [color=white]{$initiator}[/color]
wanted-list-unknown-initiator-label = iniciador desconhecido

# Trauma - added demote-perma
wanted-list-status-label = [color=darkgray]status:[/color] {$status ->
        [demote] [color=red]rebaixar[/color]
        [brutalize] [color=orange]torturar[/color]
        [search] [color=#008080]procurar[/color]
        [perma] [color=#b18644]pris�o perp�tua[/color]
        [suspected] [color=yellow]suspeito[/color]
        [wanted] [color=red]procurado[/color]
        [detained] [color=#b18644]detido[/color]
        [paroled] [color=green]liberado condicional[/color]
        [discharged] [color=green]solto[/color]
        [hostile] [color=darkred]hostil[/color]
        [eliminated] [color=gray]eliminado[/color]
        *[other] nenhum
    }

wanted-list-history-table-time-col = Hor�rio
wanted-list-history-table-reason-col = Crime
wanted-list-history-table-initiator-col = Iniciador
