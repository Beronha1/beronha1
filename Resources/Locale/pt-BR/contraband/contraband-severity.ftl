contraband-examine-text-Minor =
    {$tipo ->
        *[item] [color={$color}]Este item é considerado contrabando menor.[/color]
        [reagent] [color={$color}]Este reagente é considerado contrabando menor.[/color]
    }

contraband-examine-text-Restricted =
    {$tipo ->
        *[item] [color={$color}]Este item é restrito ao departamento.[/color]
        [reagent] [color={$color}]Este reagente é restrito ao departamento.[/color]
    }

contraband-examine-text-Restricted-department =
    {$tipo ->
        *[item] [color={$color}]Este item é restrito a {$departments} e pode ser considerado contrabando.[/color]
        [reagent] [color={$color}]Este reagente é restrito a {$departments} e pode ser considerado contrabando.[/color]
    }

contraband-examine-text-Major =
    {$tipo ->
        *[item] [color={$color}]Este item é considerado contrabando grave.[/color]
        [reagent] [color={$color}]Este reagente é considerado contrabando grave.[/color]
    }

contraband-examine-text-GrandTheft =
    {$tipo ->
        *[item] [color={$color}]Este item é um alvo altamente valioso para agentes do Sindicato![/color]
        [reagent] [color={$color}]Este reagente é um alvo altamente valioso para agentes do Syndicate![/color]
    }

contraband-examine-text-Highly-Illegal =
    {$tipo ->
        *[item] [color={$color}]Este item é contrabando altamente ilegal![/color]
        [reagent] [color={$color}]Este reagente é contrabando altamente ilegal![/color]
    }

contraband-examine-text-Syndicate =
    {$tipo ->
        *[item] [color={$color}]Este item é contrabando altamente ilegal do Sindicato![/color]
        [reagent] [color={$color}]Este reagente é contrabando altamente ilegal do Sindicato![/color]
    }

contraband-examine-text-Magical =
    {$tipo ->
        *[item] [color={$color}]Este item é contrabando mágico altamente ilegal![/color]
        [reagent] [color={$color}]Este reagente é contrabando mágico altamente ilegal![/color]
    }

contraband-examine-text-avoid-carrying-around= [color=red][italic]Você provavelmente deseja evitar carregar isso visivelmente sem um bom motivo.[/italic][/color]
contraband-examine-text-in-the-clear= [color=green][italic]Você deve estar livre para carregar isso visivelmente.[/italic][/color]

contraband-examinable-verb-text= Legalidade
contraband-examinable-verb-message= Verifique a legalidade deste item.

contraband-department-plural = {$department}
contraband-job-plural= {MAKEPLURAL($job)}
