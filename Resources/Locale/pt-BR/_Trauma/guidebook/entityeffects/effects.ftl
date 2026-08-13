entity-effect-guidebook-delete-entity = { $chance ->
    [1] exclui
    *[other] excluir
} o alvo
entity-effect-guidebook-force-equip-clothing = { $chance ->
    [1] for?a
    *[other] for?ar
} {A($name)} para o {$slot} do alvo

entity-effect-guidebook-part-add-slot = { $chance ->
    [1] adiciona
    *[other] adicionar
} uma slot {$slot} ? parte do alvo

entity-effect-guidebook-insert-new-organ = { $chance ->
    [1] insere
    *[other] inserir
} um(a) {$organ} na parte do alvo

entity-effect-guidebook-add-to-chemicals = { $chance ->
    [1] { $deltasign ->
            [1] Adiciona
            *[-1] Remove
        }
    *[other]
        { $deltasign ->
            [1] adicionar
            *[-1] remover
        }
} {NATURALFIXED($amount, 2)}u de {$reagent} { $deltasign ->
    [1] para
    *[-1] de
} a solu??o

entity-effect-guidebook-make-traitor = { $chance ->
    [1] transforma
    *[other] transformar
} o alvo em um traidor

entity-effect-guidebook-infect-disease = { $chance ->
    [1] infecta
    *[other] infectar
} o alvo com {$disease}

entity-effect-guidebook-add-marking = { $chance ->
    [1] adiciona
    *[other] adicionar
} {$marking} ao alvo
entity-effect-guidebook-remove-marking = { $chance ->
    [1] remove
    *[other] remover
} {$marking} do alvo

entity-effect-guidebook-speak = Causa fala involunt?ria

entity-effect-guidebook-scale-entity = Altera o tamanho do alvo em ({$x}, {$y})

entity-effect-guidebook-attack-self = { $chance ->
    [1] faz
    *[other] fazer
} o alvo {$canUse ->
    [true] atacar
    *[false] socar
} a si mesmo
entity-effect-guidebook-attack-others = { $chance ->
    [1] faz
    *[other] fazer
} o alvo atacar algo aleat?rio por perto

entity-effect-guidebook-start-use-delay = { $chance ->
    [1] inicia
    *[other] iniciar
} o atraso de uso de {$id} no alvo

entity-effect-guidebook-part-remove-slot = { $chance ->
    [1] remove
    *[other] remover
} uma slot {$slot} da parte do alvo

entity-effect-guidebook-remove-part = { $chance ->
    [1] destaca
    *[other] destacar
} o membro do corpo

entity-effect-guidebook-set-standing = { $chance ->
    [1] faz
    *[other] fazer
} o alvo {$standing ->
    [true] levantar
    *[other] derrubar
}

entity-effect-guidebook-relay-random-part = para uma parte aleatéria, {$effect}

entity-effect-guidebook-nothing = nada acontece { $chance ->
    [1] nunca
    *[other] nunca
}

entity-effect-guidebook-scramble-dna = { $chance ->
    [1] embaralha
    *[other] embaralhar
} as muta??es do alvo

entity-effect-guidebook-move-organ = { $chance ->
    [1] move
    *[other] mover
} o {$organ} do alvo para {$dest}

entity-effect-guidebook-heal-bone-damage = { $chance ->
     [1] cura
     *[other] curar
} {NATURALFIXED($amount, 2)} de dano ?sseo

entity-effect-guidebook-detach-part = { $chance ->
    [1] Destaca
    *[other] destacar
} o alvo da parte corporal à qual está ligado
entity-effect-guidebook-emp-reaction-effect = { $chance ->
    [1] Cria
    *[other] criar
} um pulso eletromagnético
entity-effect-guidebook-flash-reaction-effect = { $chance ->
    [1] Cria
    *[other] criar
} um clarão cegante
entity-effect-guidebook-gib = { $chance ->
    [1] Esquarteja
    *[other] esquartejar
} o alvo
entity-effect-guidebook-random-polymorph = { $chance ->
    [1] Transforma
    *[other] transformar
} o alvo em uma forma aleatória
entity-effect-guidebook-regenerate-part = { $chance ->
    [1] Regenera
    *[other] regenerar
} o órgão {$slot} do alvo
entity-effect-guidebook-relay-mutated = { $chance ->
    [1] Transmite
    *[other] transmitir
} {$effect} ao alvo da mutação
entity-effect-guidebook-relay-puller = { $chance ->
    [1] Transmite
    *[other] transmitir
} {$effect} à entidade que está puxando o alvo
entity-effect-guidebook-remove-snares = { $chance ->
    [1] Remove
    *[other] remover
} as amarras do alvo
entity-effect-guidebook-revert-polymorph = { $chance ->
    [1] Reverte
    *[other] reverter
} a transformação do alvo
entity-effect-paint-target-guidebook-text = { $chance ->
    [1] Pinta
    *[other] pintar
} o alvo



