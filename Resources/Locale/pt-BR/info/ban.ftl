# ban
cmd-ban-desc = Bane alguém
# Trauma - added severity and webhook reason
cmd-ban-help = Uso: ban <nome ou ID do usuário> <motivo> [duração em minutos, omita ou 0 para ban permanente] [severidade] [sobrescrita do motivo do webhook]
cmd-ban-player = Não foi possível encontrar um jogador com esse nome.
cmd-ban-invalid-minutes = {$minutes} não é uma quantidade válida de minutos!
cmd-ban-invalid-severity = {$severity} não é uma severidade válida!
cmd-ban-invalid-arguments = Quantidade inválida de argumentos
cmd-ban-hint = <nome/ID do usuário>
cmd-ban-hint-reason = <motivo>
cmd-ban-hint-duration = [duração]
cmd-ban-hint-severity = [severidade]

cmd-ban-hint-duration-1 = Permanente
cmd-ban-hint-duration-2 = 1 dia
cmd-ban-hint-duration-3 = 3 dias
cmd-ban-hint-duration-4 = 1 semana
cmd-ban-hint-duration-5 = 2 semanas
cmd-ban-hint-duration-6 = 1 mês

# ban panel
cmd-banpanel-desc = Abre o painel de banimentos
cmd-banpanel-help = Uso: banpanel [nome ou user guid]
cmd-banpanel-server = Isso não pode ser usado pelo console do servidor
cmd-banpanel-player-err = O jogador especificado não foi encontrado

# listbans
cmd-banlist-desc = Lista os banimentos ativos de um usuário.
cmd-banlist-help = Uso: banlist <nome ou ID do usuário>
cmd-banlist-empty = Nenhum banimento ativo encontrado para {$user}
cmd-banlist-hint = <nome/ID do usuário>

cmd-ban_exemption_update-desc = Define uma isenção para um tipo de banimento de um jogador.
cmd-ban_exemption_update-help = Usage: ban_exemption_update <player> <flag> [<flag> [...]]
    Specify multiple flags to give a player multiple ban exemption flags.
    To remove all exemptions, run this command and give "None" as only flag.

cmd-ban_exemption_update-nargs = Esperado pelo menos 2 argumentos
cmd-ban_exemption_update-locate = Não foi possível localizar o jogador '{$player}'.
cmd-ban_exemption_update-invalid-flag = Flag inválida: '{$flag}'.
cmd-ban_exemption_update-success = As flags de isenção de banimento de '{$player}' ({$uid}) foram atualizadas.
cmd-ban_exemption_update-arg-player = <jogador>
cmd-ban_exemption_update-arg-flag = <flag>

cmd-ban_exemption_get-desc = Mostra as isenções de banimento de um jogador.
cmd-ban_exemption_get-help = Uso: ban_exemption_get <jogador>

cmd-ban_exemption_get-nargs = Esperado exatamente 1 argumento
cmd-ban_exemption_get-none = O usuário não possui isenção de nenhum banimento.
cmd-ban_exemption_get-show = O usuário possui isenção das seguintes flags de banimento: {$flags}.
cmd-ban_exemption_get-arg-player = <jogador>

# Painel de banimento
ban-panel-title = Painel de banimento
ban-panel-player = Jogador
ban-panel-ip= PI
ban-panel-hwid = HWID
ban-panel-reason = Motivo
ban-panel-last-conn = Usar o IP e o HWID da última conexão?
ban-panel-submit = Banir
ban-panel-confirm = Are you sure?
ban-panel-tabs-basic = Informações básicas
ban-panel-tabs-reason = Motivo
ban-panel-tabs-players = Lista de jogadores
ban-panel-tabs-role = Informações do banimento de função
ban-panel-no-data = Você deve fornecer um usuário, IP ou HWID para banir
ban-panel-invalid-ip = Não foi possível interpretar o endereço IP. Tente novamente.
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
ban-panel-ip-hwid-tooltip = Deixe vazio e marque a opção abaixo para usar os dados da última conexão
ban-panel-severity = Severidade:
ban-panel-erase = Apagar mensagens de chat e jogador da rodada
ban-panel-expiry-error = A duração informada é inválida

# Texto de ban
server-ban-string = {$admin} criou um banimento de severidade {$severity} do servidor que expira em {$expires} para [{$name}, {$ip}, {$hwid}], com motivo: {$reason}
server-ban-string-no-pii = {$admin} criou um banimento de severidade {$severity} do servidor que expira em {$expires} para {$name} com motivo: {$reason}
server-ban-string-never = nunca

# Kick no ban
ban-kick-reason = Você foi banido
