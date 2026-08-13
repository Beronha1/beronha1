# TODO: Make this a fluent function in RT
photograph-name-text = Esta é uma fotografia de { PROPER($entity) ->
    *[false] { INDEFINITE($entity) } { $entity }
     [true] { $entity }
    }.
photograph-name-text-empty = Esta é uma fotografia.
photograph-name-text-photograph = Esta é uma fotografia de outra fotografia.

