# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
# SPDX-FileCopyrightText: 2026 punkzebub <punkzebub@gmail.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Whiskey Station - branded server currency
server-currency-name-singular= Moeda de uísque
server-currency-name-plural= Moedas de uísque

## Commands

server-currency-gift-command = gift
server-currency-gift-command-description = Dá parte do seu saldo como presente para outro jogador.
server-currency-gift-command-help = Uso: presente <jogador> <valor>
server-currency-gift-command-error-1 = Você não pode presentear a si mesmo!
server-currency-gift-command-error-2 = Você não pode se dar a esse presente! Seu saldo é de {$balance}.
server-currency-gift-command-giver = You gave {$player} {$amount}.
server-currency-gift-command-reciever = {$player} gave you {$amount}.

server-currency-balance-command = balance
server-currency-balance-command-description = Retorna seu saldo.
server-currency-balance-command-help = Uso: saldo
server-currency-balance-command-return = Você tem {$balance}.

server-currency-add-command = balance:add
server-currency-add-command-description = Adiciona moeda ao saldo de um jogador.
server-currency-add-command-help = Uso: saldo:Adicionar <jogador> <valor>

server-currency-remove-command = balance:rem
server-currency-remove-command-description = Remover moeda do saldo de um jogador.
server-currency-remove-command-help = Uso: saldo:rem <jogador> <valor>

server-currency-set-command = balance:set
server-currency-set-command-description = Define o saldo de um jogador.
server-currency-set-command-help = Uso: saldo:Definir <jogador> <valor>

server-currency-get-command = balance:get
server-currency-get-command-description = Obtém o saldo de um jogador.
server-currency-get-command-help = Uso: saldo:get <jogador>

server-currency-command-completion-1 = Nome de usuário
server-currency-command-completion-2 = Valor
server-currency-command-error-1 = Não foi possível encontrar um jogador com esse nome.
server-currency-command-error-2 = O valor deve ser inteiro.
server-currency-command-return = {$player} has {$balance}.

# 65% Update

gs-balanceui-title = Loja
gs-balanceui-confirm = Confirmar

gs-balanceui-gift-label = Transferência:
gs-balanceui-gift-player = Jogador
gs-balanceui-gift-player-tooltip = Insira o nome do jogador para quem quer enviar dinheiro
gs-balanceui-gift-value = Valor
gs-balanceui-gift-value-tooltip = Quantidade de dinheiro para transferir

gs-balanceui-shop-label = Loja de Tokens
gs-balanceui-shop-empty = Fora de estoque!
gs-balanceui-shop-buy = Comprar
gs-balanceui-shop-footer = ⚠ Ahelp para usar seu token. Apenas 1 uso por dia.

gs-balanceui-shop-token-label= Fichas
gs-balanceui-shop-tittle-label = Títulos

gs-balanceui-admin-add-label = Adicionar (ou subtrair) dinheiro:
gs-balanceui-admin-add-player = Nome do jogador
gs-balanceui-admin-add-value = Valor

gs-balanceui-shop-click-confirm = Clique novamente para confirmar
gs-balanceui-shop-purchased = Comprado {$item}
