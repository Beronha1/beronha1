reagent-effect-guidebook-deal-stamina-damage =
    { $chance ->
        [1] { $deltasign ->
                [1] Causa
                *[-1] Cura
            }
        *[other]
            { $deltasign ->
                [1] causar
                *[-1] curar
            }
    } { $amount } dano de estamina { $immediate ->
                    [true] imediato
                    *[false] ao longo do tempo
                  }

reagent-effect-guidebook-stealth-entities = Camufla mobs vivos próximos.

reagent-effect-guidebook-change-faction = Muda a facção do mob para {$faction}.

reagent-effect-guidebook-mutate-plants-nearby = Muta plantas próximas aleatoriamente.

reagent-effect-guidebook-dnascramble = Embaralha o DNA da pessoa.

reagent-effect-guidebook-change-species = Transforma o alvo em um {$species}.

reagent-effect-guidebook-change-species-random = Transforma o alvo em uma espécie completamente aleatória.

reagent-effect-guidebook-immunity-modifier =
    { $chance ->
        [1] Modifica
        *[other] modifica
    } a taxa de ganho de imunidade em {NATURALFIXED($gainrate, 5)}, força em {NATURALFIXED($strength, 5)} por pelo menos {NATURALFIXED($time, 3)} { $time ->
        [1] segundo
        *[other] segundos
    }

reagent-effect-guidebook-disease-progress-change =
    { $chance ->
        [1] Modifica
        *[other] modifica
    } o progresso de doenças do tipo {$type} em {NATURALFIXED($amount, 5)}

reagent-effect-guidebook-disease-mutate = Muta doenças em {NATURALFIXED($amount, 4)}

