## Survivor

roles-antag-survivor-name = Sobrevivente
# It's a Halo reference
roles-antag-survivor-objective = Objetivo atual: Sobreviver

survivor-role-greeting =
    Você é um Sobrevivente. Acima de tudo, precisa voltar ao Comando Central vivo.
    Colete tanta força de fogo quanto necessário para garantir sua sobrevivência.
    Não confie em ninguém.

survivor-round-end-dead-count =
{
    $deadCount ->
        [one] [color=red]{$deadCount}[/color] sobrevivente morreu.
        *[other] [color=red]{$deadCount}[/color] sobreviventes morreram.
}

survivor-round-end-alive-count =
{
    $aliveCount ->
        [one] [color=yellow]{$aliveCount}[/color] sobrevivente ficou isolado na estação.
        *[other] [color=yellow]{$aliveCount}[/color] sobreviventes ficaram isolados na estação.
}

survivor-round-end-alive-on-shuttle-count =
{
    $aliveCount ->
        [one] [color=green]{$aliveCount}[/color] sobrevivente saiu vivo.
        *[other] [color=green]{$aliveCount}[/color] sobreviventes saíram vivos.
}

## Wizard

objective-issuer-swf = [color=turquoise]A Federação de Wizards Espaciais[/color]

wizard-title = Mago
wizard-description = Há um mago na estação! Você nunca sabe o que eles podem fazer.

roles-antag-wizard-name = Mago
roles-antag-wizard-objective = Dê-lhes uma lição que nunca vão esquecer.

wizard-role-greeting =
    É hora de magia, bola de fogo!
    Houve tensões entre a Federação de Magos Espaciais e a Nanotrasen. Você foi selecionado pela Federação de Magos Espaciais para visitar a estação e "lembrá-los" de por que conjuradores não devem ser provocados.
    Cause caos e destruição! O que você faz depende de você, mas lembre-se: a Federação de Magos quer que você saia vivo.

wizard-round-end-name = mago

## TODO: Wizard Apprentice (Coming sometime post-wizard release)


