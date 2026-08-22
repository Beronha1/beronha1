# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
# SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# UI
admin-notes-title = Notas para {$player}
admin-notes-new-note = Nova nota
admin-notes-show-more = Mostrar mais
admin-notes-for = Nota para: {$player}
admin-notes-id= Identidade: {$id}
admin-notes-type = Tipo: {$type}
admin-notes-severity = Severidade: {$severity}
admin-notes-secret = Secreto
admin-notes-notsecret = Não secreto
admin-notes-expires = Expira em: {$expires}
admin-notes-expires-never = Não expira
admin-notes-edited-never = Nunca
admin-notes-round-id = Rodada Id: {$id}
admin-notes-round-id-unknown = Rodada Id: Desconhecido
admin-notes-created-by = Criado por: {$author}
admin-notes-created-at = Criado em: {$date}
admin-notes-last-edited-by = Última edição por: {$author}
admin-notes-last-edited-at = Última edição em: {$date}
admin-notes-edit = Editar
admin-notes-delete = Excluir
admin-notes-hide = Ocultar
admin-notes-delete-confirm = Confirmar exclusão
admin-notes-edited = Última edição por {$author} em {$date}
admin-notes-unbanned = Unban por {$admin} em {$date}
admin-notes-message-desc = [color=white]Você recebeu { $count ->
    [1] uma mensagem administrativa
    *[other] mensagens administrativas
} desde a ultima vez que jogou neste servidor.[/color]
admin-notes-message-admin = De [bold]{ $admin }[/bold], escrito em { TOSTRING($date, "f") }:
admin-notes-message-wait = O botão de aceitar será ativado em {$time} segundos.
admin-notes-message-accept = Encerrar permanentemente
admin-notes-message-dismiss = Dispensar por agora
admin-notes-message-seen = Visto
admin-notes-banned-from = Banido de
admin-notes-the-server = servidor
admin-notes-permanently = permanentemente
admin-notes-days = {$days} dias
admin-notes-hours = {$hours} horas
admin-notes-minutes = {$minutes} minutos

# Note editor UI
admin-note-editor-title-new = Criando uma nova nota para {$player}
admin-note-editor-title-existing = Editando nota {$id} em {$player} por {$author}
admin-note-editor-pop-out = Abrir fora
admin-note-editor-secret = Secret?
admin-note-editor-secret-tooltip = Marcando isso, a nota nao sera visivel para o jogador
admin-note-editor-type-note = Nota
admin-note-editor-type-message = Mensagem
admin-note-editor-type-watchlist= Lista de observação
admin-note-editor-type-server-ban = Ban de servidor
admin-note-editor-type-role-ban = Ban de cargo
admin-note-editor-severity-select = Selecionar
admin-note-editor-severity-none = Nenhum
admin-note-editor-severity-low = Baixa
admin-note-editor-severity-medium = Média
admin-note-editor-severity-high = Alta
admin-note-editor-expiry-checkbox = Permanent?
admin-note-editor-expiry-checkbox-tooltip = Marque para fazer expirar
admin-note-editor-expiry-label = Expira em:
admin-note-editor-expiry-label-params = Expira em: {$date} (em {$expiresIn})
admin-note-editor-expiry-label-expired = Expirado
admin-note-editor-expiry-placeholder = Informe o tempo de expiracao (inteiro).
admin-note-editor-submit = Enviar
admin-note-editor-submit-confirm = Are you sure?

# Time
admin-note-button-minutes = Minutos
admin-note-button-hours = Horas
admin-note-button-days = Dias
admin-note-button-weeks = Semanas
admin-note-button-months = Meses
admin-note-button-years = Anos
admin-note-button-centuries= Séculos


# Verb
admin-notes-verb-text = Abrir Notas administração

# Watchlist and message login
admin-notes-watchlist = Watchlist para {$player}: {$message}
admin-notes-new-message = Voce recebeu uma mensagem de administrador de {$admin}: {$message}
admin-notes-fallback-admin-name = [System]

# Admin remarks
admin-remarks-command-description = Abre a pagina de observacoes administração
admin-remarks-command-error = Observacoes administração foram desabilitadas
admin-remarks-title = Observacoes administração

# Misc
system-user = [System]
