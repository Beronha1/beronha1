eldritch-id-card-component-examine-inverted = O efeito atual é [color=yellow]invertido[/color]

eldritch-id-card-component-examine-message =
    Encantado pelo Mansus!
    Usar um ID nele ou usar esse ID em outro ID vai consumi-lo e permitir copiar seus acessos.
    Usá-lo na mão permite mudar sua aparência.
    Usá-lo em um par de portas permite vinculá-las. Entrar em uma delas te transporta para a outra, enquanto os hereges são teleportados para um ar-condicionado aleatório.
    Clicar com Alt no ID fará esse ID criar portais invertidos em vez disso, teleportando você para um airlock aleatório da estação, enquanto hereges são teleportados para o destino.

eldritch-id-card-component-on-invert =
    { $inverted ->
      [true] agora
      *[false] não está mais
    } criando fendas invertidas

eldritch-id-card-component-portal-inverted =
    portal { $inverted ->
             [true] invertido
             *[false] não invertido
           }

eldritch-id-card-component-invert = Inverter
eldritch-id-card-component-invert-message = Faça o ID criar portais invertidos, que te teleportam para um airlock aleatório da estação, enquanto hereges são teleportados para o destino ou vice-versa.

eldritch-id-card-component-link-one= ligação 1/2
eldritch-id-card-component-link-two= ligação 2/2

lock-portal-component-clear-portals = Limpar ambos os links

lock-portal-component-examine-inverted = [color=yellow]invertido[/color]
lock-portal-component-examine-not-inverted = [color=yellow]não invertido[/color]

lock-portal-component-examine-message =
    O portal está {$status}.
    Clique usando o ID eldritch para invertê-lo.
    Alt-clique com id eldritch para remover ambos os links.
