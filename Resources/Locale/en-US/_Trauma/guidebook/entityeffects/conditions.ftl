entity-condition-guidebook-has-mob-name = the target's name contains { $name }
entity-condition-guidebook-inventory-nested = the target is wearing anything where {$condition}
entity-condition-guidebook-inside-area = the target is inside an area
entity-condition-guidebook-has-marking = the target has a {$marking} marking
entity-condition-guidebook-is-species = the target is a {$species}

entity-condition-guidebook-pressure-protection = the target is protected from pressure
entity-condition-guidebook-cosmic-cultist = the target is a cosmic cultist
entity-condition-shadowling-or-thrall = target is a shadowling or thrall
entity-condition-not-shadowling-or-thrall = target is not a shadowling or thrall
entity-condition-guidebook-is-humanoid = target is humanoid
entity-condition-guidebook-hypoport-target = target can receive a hypoport

entity-condition-guidebook-cybernetics-blacklist = Is not a cybernetic limb

entity-condition-guidebook-is-awake = the target is awake

entity-condition-guidebook-use-delay = the target has no active {$id} use delay

entity-condition-guidebook-organ-slot = the target's {$part} {$inverted ->
    [true] has no
    *[false] has a
} {$slot} slot

entity-condition-guidebook-dna-unstable = the target's DNA is unstable
entity-condition-guidebook-has-organ = the target { $invert ->
    [true] does not have
    *[false] has
} a {$organ} organ
entity-condition-guidebook-holding-item = the target is holding an item
entity-condition-guidebook-in-container = the target is inside a container
entity-condition-guidebook-standing = the target is standing
entity-condition-guidebook-vital-damage = { $max ->
    [2147483647] the target has at least {NATURALFIXED($min, 2)} vital damage
    *[other] { $min ->
        [0] the target has at most {NATURALFIXED($max, 2)} vital damage
        *[other] the target has between {NATURALFIXED($min, 2)} and {NATURALFIXED($max, 2)} vital damage
    }
}
