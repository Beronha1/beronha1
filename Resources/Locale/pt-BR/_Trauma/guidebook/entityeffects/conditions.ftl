entity-condition-guidebook-has-mob-name = O nome do alvo contém { $name }
entity-condition-guidebook-inventory-nested = O alvo está vestindo qualquer coisa onde {$condition}
entity-condition-guidebook-inside-area = the target is inside an area
entity-condition-guidebook-has-marking = O alvo tem uma marcação {$marking}
entity-condition-guidebook-is-species = the target is a {$species}

entity-condition-guidebook-pressure-protection = O alvo está protegido contra pressão
entity-condition-guidebook-cosmic-cultist = the target is a cosmic cultist
entity-condition-shadowling-or-thrall = target is a shadowling or thrall
entity-condition-not-shadowling-or-thrall = target is not a shadowling or thrall
entity-condition-guidebook-is-humanoid = target is humanoid
entity-condition-guidebook-hypoport-target = O alvo pode receber uma hipoporta

entity-condition-guidebook-cybernetics-blacklist = Is not a cybernetic limb

entity-condition-guidebook-is-awake = O alvo está acordado

entity-condition-guidebook-use-delay = O alvo não tem atraso de uso ativo de {$id}

entity-condition-guidebook-organ-slot = the target's {$part} {$inverted ->
    [true] has no
    *[false] has a
} {$slot} slot

entity-condition-guidebook-dna-unstable = O DNA do alvo está instável
entity-condition-guidebook-has-organ = O alvo { $invert ->
    [true] não possui
    *[false] possui
} um órgão da categoria {$organ}
entity-condition-guidebook-holding-item = O alvo está segurando um item
entity-condition-guidebook-in-container = O alvo está dentro de um contêiner
entity-condition-guidebook-standing = O alvo está em pé
entity-condition-guidebook-vital-damage = { $max ->
    [2147483647] O alvo possui pelo menos {NATURALFIXED($min, 2)} de dano vital
    *[other] { $min ->
        [0] O alvo possui no máximo {NATURALFIXED($max, 2)} de dano vital
        *[other] O alvo possui entre {NATURALFIXED($min, 2)} e {NATURALFIXED($max, 2)} de dano vital
    }
}
