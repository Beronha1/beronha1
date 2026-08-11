# ban
cmd-ban-desc = Bane alguém
# Trauma - added severity and webhook reason
cmd-ban-help = Uso: ban <nome ou ID do usuário> <motivo> [duração em minutos, omita ou 0 para ban permanente] [severidade] [sobrescrita do motivo do webhook]
cmd-ban-player = N?o foi poss?vel encontrar um jogador com esse nome.
cmd-ban-invalid-minutes = {$minutes} não ? uma quantidade v?lida de minutos!
cmd-ban-invalid-severity = {$severity} não ? uma severidade v?lida!
cmd-ban-invalid-arguments = Quantidade de argumentos inv?lida
cmd-ban-hint = <nome/ID do usuário>
cmd-ban-hint-reason = <motivo>
cmd-ban-hint-duration = [duração]
cmd-ban-hint-severity = [severidade]

cmd-ban-hint-duration-1 = Permanente
cmd-ban-hint-duration-2 = 1 dia
cmd-ban-hint-duration-3 = 3 dias
cmd-ban-hint-duration-4 = 1 semana
cmd-ban-hint-duration-5 = 2 semanas
cmd-ban-hint-duration-6 = 1 m?s

# ban panel
cmd-banpanel-desc = Abre o painel de banimentos
cmd-banpanel-help = Uso: banpanel [nome ou user guid]
cmd-banpanel-server = Isso não pode ser usado pelo console do servidor
cmd-banpanel-player-err = N?o foi poss?vel encontrar o jogador especificado

# listbans
cmd-banlist-desc = Lista os banimentos ativos de um usuário.
cmd-banlist-help = Uso: banlist <nome ou ID do usuário>
cmd-banlist-empty = Nenhum banimento ativo encontrado para {$user}
cmd-banlist-hint = <nome/ID do usuário>

cmd-ban_exemption_update-desc = Define uma isen??o para um tipo de banimento em um jogador.
cmd-ban_exemption_update-help = Uso: ban_exemption_update <jogador> <flag> [<flag> [...]]
    Especifique v?rias flags para dar ao jogador m?ltiplas isen??es de banimento.
    Para remover todas as isen??es, execute este comando e passe "None" como ?nica flag.

cmd-ban_exemption_update-nargs = Esperado pelo menos 2 argumentos
cmd-ban_exemption_update-locate = N?o foi poss?vel localizar o jogador '{$player}'.
cmd-ban_exemption_update-invalid-flag = Flag '{$flag}' inv?lida.
cmd-ban_exemption_update-success = Flags de isen??o de banimento atualizadas para '{$player}' ({$player}).
cmd-ban_exemption_update-arg-player = <jogador>
cmd-ban_exemption_update-arg-flag = <flag>

cmd-ban_exemption_get-desc = Exibe as isen??es de banimento para um jogador espec?fico.
cmd-ban_exemption_get-help = Uso: ban_exemption_get <jogador>

cmd-ban_exemption_get-nargs = Esperado exatamente 1 argumento
cmd-ban_exemption_get-none = O usuário não ? isento de nenhum banimento.
cmd-ban_exemption_get-show = O usuário ? isento das seguintes flags de banimento: {$flags}.
cmd-ban_exemption_get-arg-player = <jogador>

# Painel de banimento
ban-panel-title = Painel de banimento
ban-panel-player = Jogador
ban-panel-ip= PI
ban-panel-hwid = HWID
ban-panel-reason = Motivo
ban-panel-last-conn = Usar IP e HWID da ?ltima conex?o?
ban-panel-submit = Banir
ban-panel-confirm = Tem certeza?
ban-panel-tabs-basic = Informa??es b?sicas
ban-panel-tabs-reason = Motivo
ban-panel-tabs-players = Lista de jogadores
ban-panel-tabs-role = Informa??es de banimento por função
ban-panel-no-data = Você deve fornecer um usuário, IP ou HWID para banir
ban-panel-invalid-ip = O endere?o de IP não p?de ser analisado. Tente novamente
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
ban-panel-ip-hwid-tooltip = Deixe em branco e marque a caixa abaixo para usar os detalhes da ?ltima conex?o
ban-panel-severity = Severidade:
ban-panel-erase = Apagar mensagens de chat e jogador da rodada
ban-panel-expiry-error= errar

# Texto de ban
server-ban-string = {$admin} criou um banimento de severidade {$severity} do servidor que expira em {$expires} para [{$name}, {$ip}, {$hwid}], com motivo: {$reason}
server-ban-string-no-pii = {$admin} criou um banimento de severidade {$severity} do servidor que expira em {$expires} para {$name} com motivo: {$reason}
server-ban-string-never = nunca

# Kick no ban
ban-kick-reason = Você foi banido
