# Battery Status
battery-status-charge =Cobrança: [color=#5E7C16]{$percent}[/color]%
battery-status-switchable-state= {$state ->
        [on] [color=green]Ativado[/color]
        [off] [color=red]Desligado[/color]
        *[other] Desconhecido
}
battery-status-state = Estado: {$state}

# Charge Status
charge-status-count = Cargas: [color=fuchsia]{$current}/{$max}[/color]
charge-status-recharge = Recarga: [color=yellow]{$seconds}s[/color]

# Tank Pressure Status
tank-pressure-status = Pressione.: [color=orange]{$pressure} kPa[/color]
tank-status-switchable-state= {$state ->
        [open] [color=red]Aberto[/color]
        [closed] [color=green]Fechado[/color]
        *[other] Desconhecido
}
tank-status-state = Estado: {$state}

# Magazine Status
magazine-status-rounds = Rodadas: [color=yellow]{$current}/{$max}[/color]

# Guardian Status
guardian-status-used = [color=red]Usado[/color]
guardian-status-ready = [color=green]Pronto[/color]

# Anomaly Status
anomaly-status-infinite = [color=gold]Cargas infinitas[/color]
anomaly-status-charges = [color=orange]{$charges} cobranças[/color]

# Timer Trigger Status
timer-trigger-status-delay = Definir atraso: [color=white]{$delay}s[/color]
