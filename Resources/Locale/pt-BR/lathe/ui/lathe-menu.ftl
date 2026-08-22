lathe-menu-title = menu de torno
lathe-menu-queue = Fila
lathe-menu-server-list = Lista de servidores
lathe-menu-sync = Sincronizar
lathe-menu-search-designs = Buscar projetos
lathe-menu-category-all = Todos
lathe-menu-search-filter = Filtro:
lathe-menu-amount = Quantidade:
lathe-menu-recipe-count = { $count ->
    [1] {$count} Receita
    *[other] {$count} Receitas
}
lathe-menu-reagent-slot-examine = Ele tem um slot para um erlenmeyer na lateral.
lathe-reagent-dispense-no-container = Líquido escorre de {THE($name)} pelo chão!
lathe-menu-result-reagent-display = {$reagent} ({$amount}u)
lathe-menu-material-display = {$material} ({$amount})
lathe-menu-tooltip-display = {$amount} de {$material}
lathe-menu-description-display = [italic]{$description}[/italic]
lathe-menu-material-amount = { $amount ->
    [1] {NATURALFIXED($amount, 2)} {$unit}
    *[other] {NATURALFIXED($amount, 2)} {MAKEPLURAL($unit)}
}
lathe-menu-material-amount-missing = { $amount ->
    [1] {NATURALFIXED($amount, 2)} {$unit} de {$material} ([color=red]{NATURALFIXED($missingAmount, 2)} {$unit} em falta[/color])
    *[other] {NATURALFIXED($amount, 2)} {MAKEPLURAL($unit)} de {$material} ([color=red]{NATURALFIXED($missingAmount, 2)} {MAKEPLURAL($unit)} em falta[/color])
}
lathe-menu-no-materials-message = No materials loaded.
lathe-menu-silo-linked-message = Silo conectado
lathe-menu-fabricating-message = Fabricando...
lathe-menu-materials-title = Materiais
lathe-menu-queue-title = Fila de produção

# Mono
lathe-menu-delete-fabricating-tooltip = Cancelar Uma impressão do item atual.
lathe-menu-delete-item-tooltip = Cancelar Uma impressão deste lote.
lathe-menu-move-up-tooltip = Mover este lote para Uma frente na fila.
lathe-menu-move-down-tooltip = Mover este lote para trás na fila.
lathe-menu-item-single = {$index}. {$name}
lathe-menu-item-batch = {$index}. {$name} ({$printed}/{$total})
lathe-menu-loop = Repetir
lathe-menu-loop-tooltip = Recoloca cada lote no fim da fila depois de concluído, mantendo a produção até os materiais acabarem.
lathe-menu-skip = Pular se insuficiente
lathe-menu-skip-tooltip = Descarta um lote repetido sem materiais suficientes em vez de bloquear a fila até o reabastecimento.
