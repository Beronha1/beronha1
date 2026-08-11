-protection = dano { $protect ->
    [true] reduzido
    *[false] [color=red]aumentado[/color]
} em [color=lightblue]{TOSTRING($value, "F1")}%[/color].

armor-coefficient-value-trauma= - [color=yellow]{$type}[/color] { -protection(proteger: $type, valor: $type) }

stamina-resistance-coefficient-value-trauma = - [color=lightyellow]Resistência[/color] { -protection(protect: $protect, value: $value) }

armor-damage-type-ballistic = Balístico
