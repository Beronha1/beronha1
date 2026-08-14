entity-effect-guidebook-delete-entity = { $chance ->
    [1] exclui
    *[other] excluir
} o alvo
entity-effect-guidebook-force-equip-clothing = force {$chance ->
    [1] equips
    *[other] equip
} {A($name)} to the target's {$slot}

entity-effect-guidebook-part-add-slot = {$chance ->
    [1] adds
    *[other] add
} a {$slot} slot to the target part

entity-effect-guidebook-insert-new-organ = { $chance ->
    [1] insere
    *[other] inserir
} um(a) {$organ} na parte do alvo

entity-effect-guidebook-add-to-chemicals = { $chance ->
    [1] { $deltasign ->
            [1] Adds
            *[-1] Removes
        }
    *[other]
        { $deltasign ->
            [1] add
            *[-1] remove
        }
} {NATURALFIXED($amount, 2)}u of {$reagent} { $deltasign ->
    [1] to
    *[-1] from
} the solution

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

entity-effect-guidebook-speak = Causes involuntary speech

entity-effect-guidebook-scale-entity = Altera o tamanho do alvo em ({$x}, {$y})

entity-effect-guidebook-attack-self = { $chance ->
    [1] faz
    *[other] fazer
} o alvo {$canUse ->
    [true] atacar
    *[false] socar
} a si mesmo
entity-effect-guidebook-attack-others = {$chance ->
    [1] makes
    *[other] make
} the target attack a random nearby thing

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

entity-effect-guidebook-scramble-dna = {$chance ->
    [1] scrambles
    *[other] scramble
} the target's mutations

entity-effect-guidebook-move-organ = { $chance ->
    [1] move
    *[other] mover
} o {$organ} do alvo para {$dest}

entity-effect-guidebook-heal-bone-damage = { $chance ->
     [1] heals
     *[other] heal
} {NATURALFIXED($amount, 2)} bone damage

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

