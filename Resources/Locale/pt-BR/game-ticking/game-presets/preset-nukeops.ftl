nukeops-title = Operativos Nucleares
nukeops-description = Operativos nucleares visaram a estação. Tente impedir que armem e detonem a bomba nuclear protegendo o disco nuclear!

nukeops-welcome =
    Você é um operador nuclear. Seu objetivo é explodir {$station} e garantir que fique apenas uma pilha de escombros. Seus chefes, a Sindicatos, forneceram as ferramentas para essa tarefa.
    Operação {$name} foi autorizada! Morte à Nanotrasen!
nukeops-briefing = Seus objetivos são simples. Entregue a carga e saia antes da detonação da carga. Inicie a missão.

nukeops-opsmajor = [color=crimson]Vitória maior dos syndicais![/color]
nukeops-opsminor = [color=crimson]Vitória menor dos syndicais![/color]
nukeops-neutral = [color=yellow]Resultado neutro![/color]
nukeops-crewminor = [color=green]Vitória menor da tripulação![/color]
nukeops-crewmajor = [color=green]Vitória maior da tripulação![/color]

nukeops-cond-nukeexplodedoncorrectstation = Os operativos nucleares conseguiram explodir a estação.
nukeops-cond-nukeexplodedonnukieoutpost = O posto avançado dos operativos nucleares foi destruído por uma explosão nuclear!
nukeops-cond-nukeexplodedonincorrectlocation = A bomba nuclear detonou fora da estação.
nukeops-cond-nukeactiveinstation = A bomba nuclear ficou armada na estação.
nukeops-cond-nukeactiveatcentcom = A bomba nuclear foi armada e entregue ao Comando Central!
nukeops-cond-nukediskoncentcom = A tripulação escapou com o disco de autenticação nuclear.
nukeops-cond-nukedisknotoncentcom = A tripulação deixou para trás o disco de autenticação nuclear.
nukeops-cond-nukiesabandoned = Os operativos nucleares foram abandonados.
nukeops-cond-allnukiesdead = Todos os operativos nucleares morreram.
nukeops-cond-somenukiesalive = Alguns operativos nucleares morreram.
nukeops-cond-allnukiesalive = Nenhum operativo nuclear morreu.

nukeops-disk-location-title = Localização final do disco:
nukeops-disk-carried-by = {" "}carregado por [color=White]{$name}[/color], [color=orange]{$job}[/color], {$location} { $user ->
    [unknown] { "" }
    *[other] ([color=gray]{$user}[/color])
}

storage-hierarchy-list= { $items-left ->
  [0] { $existing-text } { $item },
  *[other] { $existing-text } { $item }, em
}

nukeops-list-start = Os operativos nucleares foram:
nukeops-list-name= - [color=White]{$name}[/color]
nukeops-list-name-user= - [color=White]{$name}[/color] ([color=gray]{$user}[/color])
nukeops-not-enough-ready-players = Jogadores prontos insuficientes! Há {$readyPlayersCount} jogadores prontos de {$minimumPlayers} necessários. Não é possível iniciar Nukeops.
nukeops-no-one-ready = Nenhum jogador pronto! Não é possível iniciar Nukeops.

nukeops-role-commander = Comandante
nukeops-role-agent = Soldado
nukeops-role-operator = Operador

nukeops-roundend-name = operativo nuclear
