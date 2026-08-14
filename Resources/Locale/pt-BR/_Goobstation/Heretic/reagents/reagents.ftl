reagent-name-eldritch = essência eldritch
reagent-desc-eldritch = Um líquido estranho que desafia as leis da física. Ele reenergiza e cura quem consegue ver além dessa realidade frágil, mas é incrivelmente prejudicial para mentes fechadas.

reagent-name-crucible-soul = alma do cadinho
reagent-desc-crucible-soul = Um líquido laranja brilhante e translúcido. Permite que você caminhe por paredes. Após expirar, você é teleportado para sua posição original.

reagent-name-clarity = crepúsculo e aurora
reagent-desc-clarity = Um líquido amarelo opaco. Parece desaparecer e retornar com regularidade. Permite ver através de paredes e objetos.

reagent-name-marshal = soldado ferido
reagent-desc-marshal = Um líquido escuro e sem cor. Aumenta sua força física, tornando seus ataques mais ferozes e fazendo você levar menos dano quanto mais ferido estiver. Seus ataques corpo a corpo curam sua saúde e estamina, mas causam dano ao longo do tempo - quanto mais saudável você está, mais dano contínuo você recebe.

reagent-name-ether = éter do recém-nascido
reagent-desc-ether = Líquido espesso e verde que causa náusea. Restaura completamente seu corpo e depois te coloca em sono aprimorado por um minuto inteiro.

entity-condition-guidebook-heretic-or-ghoul = o alvo é um herege ou ghoul
entity-condition-guidebook-not-heretic-or-ghoul = o alvo não é um herege ou ghoul

entity-condition-guidebook-environment-temperature = a temperatura do ambiente está
    { $invert ->
        [true] at least
        *[false] at most
    } {$threshold} graus

entity-condition-guidebook-has-body-part = o alvo
    { $invert ->
        [true] has no
        *[false] has
    } {$part}

entity-condition-guidebook-on-fire = o alvo está
    { $invert ->
        [false] sem fogo
        *[true] em chamas
    }

reagent-effect-guidebook-has-status-effect =
    { $invert ->
        [true] has no
        *[false] has
    } o efeito de status {$effect}

entity-condition-guidebook-nullrod-protected = o alvo é protegido por um bastão nulo
entity-condition-guidebook-nullrod-not-protected = o alvo não é protegido por um bastão nulo

reagent-effect-guidebook-deconvert-ghoul = converte de volta uma entidade ghoulificada

reagent-physical-desc-eldritch= sobrenatural
reagent-physical-desc-crucible-soul = de outro mundo
reagent-physical-desc-clarity = claro
reagent-physical-desc-marshal = agonizante
reagent-physical-desc-ether = anestésico
reagent-physical-desc-raw = cru

flavor-complex-eldritch = Ag'hsj'saje'sh
flavor-complex-crucible-soul = como algo entre as planícies
flavor-complex-clarity = como olhos
flavor-complex-marshal = doloroso
flavor-complex-ether = refrescante

crucible-soul-effect-examine-message =
    {"["}color=#fb793a]{ CAPITALIZE(SUBJECT($ent)) } { GENDER($ent) ->
        [epicene] do
       *[other] faz
    } não parece estar todo aí.[/color]

wounded-solider-effect-examine-message = [color=#5e718e]{ CAPITALIZE(SUBJECT($ent)) } { CONJUGATE-BE($ent) } em um estado de fúria eterna.[/color]
