# ban
cmd-ban-desc = Bane alguém
# Trauma - added severity and webhook reason
cmd-ban-help = Uso: ban <nome ou ID do usuário> <motivo> [duração em minutos, omita ou 0 para ban permanente] [severidade] [sobrescrita do motivo do webhook]
cmd-ban-player = Unable to find a player with that name.
cmd-ban-invalid-minutes = {$minutes} is not a valid amount of minutes!
cmd-ban-invalid-severity = {$severity} is not a valid severity!
cmd-ban-invalid-arguments = Invalid amount of arguments
cmd-ban-hint = <nome/ID do usuário>
cmd-ban-hint-reason = <motivo>
cmd-ban-hint-duration = [duração]
cmd-ban-hint-severity = [severidade]

cmd-ban-hint-duration-1 = Permanente
cmd-ban-hint-duration-2 = 1 dia
cmd-ban-hint-duration-3 = 3 dias
cmd-ban-hint-duration-4 = 1 semana
cmd-ban-hint-duration-5 = 2 semanas
cmd-ban-hint-duration-6 = 1 month

# ban panel
cmd-banpanel-desc = Abre o painel de banimentos
cmd-banpanel-help = Uso: banpanel [nome ou user guid]
cmd-banpanel-server = Isso não pode ser usado pelo console do servidor
cmd-banpanel-player-err = The specified player could not be found

# listbans
cmd-banlist-desc = Lista os banimentos ativos de um usuário.
cmd-banlist-help = Uso: banlist <nome ou ID do usuário>
cmd-banlist-empty = Nenhum banimento ativo encontrado para {$user}
cmd-banlist-hint = <nome/ID do usuário>

cmd-ban_exemption_update-desc = Set an exemption to a type of ban on a player.
cmd-ban_exemption_update-help = Usage: ban_exemption_update <player> <flag> [<flag> [...]]
    Specify multiple flags to give a player multiple ban exemption flags.
    To remove all exemptions, run this command and give "None" as only flag.

cmd-ban_exemption_update-nargs = Esperado pelo menos 2 argumentos
cmd-ban_exemption_update-locate = Unable to locate player '{$player}'.
cmd-ban_exemption_update-invalid-flag = Invalid flag '{$flag}'.
cmd-ban_exemption_update-success = Updated ban exemption flags for '{$player}' ({$uid}).
cmd-ban_exemption_update-arg-player = <jogador>
cmd-ban_exemption_update-arg-flag = <flag>

cmd-ban_exemption_get-desc = Show ban exemptions for a certain player.
cmd-ban_exemption_get-help = Uso: ban_exemption_get <jogador>

cmd-ban_exemption_get-nargs = Esperado exatamente 1 argumento
cmd-ban_exemption_get-none = User is not exempt from any bans.
cmd-ban_exemption_get-show = User is exempt from the following ban flags: {$flags}.
cmd-ban_exemption_get-arg-player = <jogador>

# Painel de banimento
ban-panel-title = Painel de banimento
ban-panel-player = Jogador
ban-panel-ip= PI
ban-panel-hwid = HWID
ban-panel-reason = Motivo
ban-panel-last-conn = Use IP and HWID from last connection?
ban-panel-submit = Banir
ban-panel-confirm = Are you sure?
ban-panel-tabs-basic = Basic info
ban-panel-tabs-reason = Motivo
ban-panel-tabs-players = Lista de jogadores
ban-panel-tabs-role = Role ban info
ban-panel-no-data = Você deve fornecer um usuário, IP ou HWID para banir
ban-panel-invalid-ip = The IP address could not be parsed. Please try again
ban-panel-select = Selecionar tipo
ban-panel-server = Banimento do servidor
ban-panel-role = Banimento de função
ban-panel-minutes = Minutos
ban-panel-hours = Horas
ban-panel-days = Dias
ban-panel-weeks = Semanas
ban-panel-months = Meses
ban-panel-years = Anos
ban-panel-permanent = Permanente
ban-panel-ip-hwid-tooltip = Leave empty and check the checkbox below to use last connection's details
ban-panel-severity = Severidade:
ban-panel-erase = Apagar mensagens de chat e jogador da rodada
ban-panel-expiry-error= errar

# Texto de ban
server-ban-string = {$admin} criou um banimento de severidade {$severity} do servidor que expira em {$expires} para [{$name}, {$ip}, {$hwid}], com motivo: {$reason}
server-ban-string-no-pii = {$admin} criou um banimento de severidade {$severity} do servidor que expira em {$expires} para {$name} com motivo: {$reason}
server-ban-string-never = nunca

# Kick no ban
ban-kick-reason = Você foi banido
