# TODO: Make this a fluent function in RT
photograph-name-text =Esta é uma fotografia de { PROPER($entity) ->
    *[false] { INDEFINIDO($entidade) } { $entidade }
     [true] { $entidade}
    }.
photograph-name-text-empty = Esta é uma fotografia.
photograph-name-text-photograph = Esta é uma fotografia de outra fotografia.

