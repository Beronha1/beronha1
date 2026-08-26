es-objective-summary-fmt = {$name}: {$success ->
    [true] [color=limegreen]Sucesso[/color]
    *[false] [color=red]Falha[/color]
} {$percent ->
    [0] {""}
    [100] {""}
    *[other] ([color=gray]{$percent}%[/color])
}

es-objective-text-organization = Equipe
es-objective-tooltip-organization = Este é um [bold]objetivo compartilhado da organização[/bold].

    Todos os integrantes da sua organização compartilham este objetivo e precisam trabalhar em conjunto. O progresso é compartilhado entre todas as pessoas que receberam o objetivo.

es-objective-text-secret-identity = Individual
es-objective-tooltip-secret-identity = Este é um [bold]objetivo pessoal de identidade secreta[/bold].

    Este é um objetivo exclusivo baseado na identidade secreta que você recebeu. Somente você pode vê-lo. Outros integrantes da sua organização podem ter objetivos pessoais diferentes.

es-fruit-vendor-objective-title = Alimente {$count} pessoas com frutas invocadas
es-arms-dealer-objective-title = Faça a tripulação manter {$count} armas invocadas até o fim do turno
es-guzzle-objective-title = Beba {$count} unidades de líquido
es-imbibe-reagent-guzzler-desc = Beba o máximo de {$reagent} que conseguir.
es-guzzle-specific-objective-title = Beba {$count} unidades de {$reagent}
es-guzzle-unique-reagents-objective-title = Beba {$count} reagentes diferentes
es-daredevil-objective-title = Sofra {$count} de dano do tipo {$damagetype}
es-daredevil-desc = Sofra o máximo de dano do tipo {$damagetype} que conseguir.
es-daredevil-total-objective-title = Sofra {$count} de dano total
es-daredevil-total-objective-desc = Coloque-se em perigo e acumule dano de qualquer fonte.
es-eat-unique-foods-objective-title = Coma {$count} alimentos diferentes
es-eat-food-objective-title = Coma {$name}
es-sacrifice-objective-title = Cure {$count} pessoas ao se sacrificar
es-sacrifice-popup-heroic-sacrifice = {$name} realizou um sacrifício heroico!

es-daredevil-source-objective-title-ESGrille = Sofra {$count} de dano de uma grade
es-daredevil-source-objective-desc-ESGrille = Deixe uma grade eletrificada ferir você.
es-daredevil-source-objective-title-ESEmitter = Sofra {$count} de dano de um emissor
es-daredevil-source-objective-desc-ESEmitter = Coloque-se no caminho do feixe de um emissor.
es-daredevil-source-objective-title-ESAncestor = Sofra {$count} de dano de um ancestral
es-daredevil-source-objective-desc-ESAncestor = Provoque um ancestral e aguente seus ataques.

ent-ESObjectiveResearchTelescience = Pesquise a teleciência
    .desc = Trabalhe com a equipe de ciências para teleportar a estação para longe da tempestade de radiação que se aproxima.
ent-ESObjectiveEatSummonedFruit = Distribua frutas invocadas
    .desc = Use sua habilidade para manifestar frutas deliciosas e alimentar as pessoas da estação.
ent-ESObjectiveHoldSummonedGuns = Arme a tripulação
    .desc = Invoque armas de fogo e garanta que integrantes da tripulação fiquem com elas até o fim do turno.
ent-ESObjectiveKillNonCrew = Elimine uma ameaça externa
    .desc = Use sua arma de confiança para matar alguém a bordo que não faça parte da tripulação.
ent-ESObjectiveGuzzle = Beba tudo
    .desc = Consuma o máximo de líquido possível, não importa de onde venha!
ent-ESObjectiveGuzzleSpecialInterest = Beba seu reagente favorito
    .desc = Consuma o máximo possível do reagente que despertou seu interesse especial.
ent-ESObjectiveGuzzleUniqueReagents = Experimente líquidos diferentes
    .desc = Consuma a maior variedade de líquidos que conseguir.
ent-ESObjectiveTakeDamage = Sofra dano
    .desc = Sofra o máximo possível de um determinado tipo de dano.
ent-ESObjectiveTakeDamageFromSource = Sofra dano de uma fonte
    .desc = Sofra o máximo possível de dano de uma fonte específica.
ent-ESObjectiveTakeTotalDamage = Sofra dano total
    .desc = Acumule dano de qualquer fonte.
ent-ESObjectiveEatUniqueFoods = Coma alimentos diferentes
    .desc = Seu estômago deseja o desconhecido. Procure todos os sabores diferentes do mundo!
ent-ESObjectiveEatFood = Satisfaça um desejo exótico
    .desc = Encontre e coma o alimento específico que você deseja.
ent-ESObjectiveSurvive = Sobreviva
    .desc = Sobreviva até o fim do turno a qualquer custo.
ent-ESObjectiveSacrificeHeal = Sacrifique-se
    .desc = Cure outras pessoas morrendo perto delas.
