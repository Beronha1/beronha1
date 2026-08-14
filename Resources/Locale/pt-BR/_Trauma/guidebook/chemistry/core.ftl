## no reagent stuff and no trailing "." at the end
guidebook-nested-effect-description =
    {$chance ->
        [1] { $effect }
        *[other] Tem { NATURALPERCENT($chance, 2) } chance de { $effect }
    }{ $conditionCount ->
        [0] {""}
        *[other] {" "}quando { $conditions }
    }
