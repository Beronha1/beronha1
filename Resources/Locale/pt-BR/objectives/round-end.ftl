objectives-round-end-result= {$count ->
    [one] Houve um {$agent}.
    *[other] Houve {$count} {MAKEPLURAL($agent)}.
}

objectives-round-end-result-in-custody = {$custody} de {$count} {MAKEPLURAL($agent)} estiveram em custodia.

objectives-player-user-named= [color=White]{$name}[/color] ([color=gray]{$user}[/color])
objectives-player-named= [color=White]{$name}[/color]

objectives-no-objectives = {$custody}{$title} foi um {$agent}.
objectives-with-objectives = {$custody}{$title} foi um {$agent} que tinha os seguintes objetivos:

objectives-objective-success = {$objective} | [color=green]Sucesso![/color] ({TOSTRING($progress, "P0")})
objectives-objective-partial-success = {$objective} | [color=yellow]Sucesso Parcial![/color] ({TOSTRING($progress, "P0")})
objectives-objective-partial-failure = {$objective} | [color=orange]Falha Parcial![/color] ({TOSTRING($progress, "P0")})
objectives-objective-fail = {$objective} | [color=red]Falha![/color] ({TOSTRING($progress, "P0")})

objectives-in-custody = [bold][color=red]| EM CUSTODIA | [/color][/bold]
