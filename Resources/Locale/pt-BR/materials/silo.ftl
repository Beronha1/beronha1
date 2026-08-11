ore-silo-ui-title = Silos de material
ore-silo-ui-label-clients = Maquinas
ore-silo-ui-label-mats = Materiais
ore-silo-ui-itemlist-entry= {$linked ->
    [true] {"[Ligado] "}
    *[False] {""}
} {$name} ({$beacon}) {$inRange ->
    [true] {""}
    *[false] (Fora de alcance)
}
