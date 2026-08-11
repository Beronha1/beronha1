# SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 BIGZi0348 <118811750+BIGZi0348@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 lzk <124214523+lzk228@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

station-event-ion-storm-start-announcement = Tempestade de íons detectada perto da estação. Verifique todos os equipamentos controlados por IA em busca de erros.
# Characters are randomly selected from the total list, meaning duplicates increase the odds that specific character is seen.
ion-storm-law-scrambled-number = [font="Monospace"][scramble rate=250 length={$length} chars="!!@@###$$%^&*-_=+0011"/][/font]

ion-storm-you = VOCÊ
ion-storm-the-station = A ESTAÇÃO
ion-storm-the-crew = A EQUIPE
ion-storm-the-job = A FUNÇÃO {$job}
ion-storm-clowns = PALHAÇOS
ion-storm-heads = CHEFES DE DEPARTAMENTO
ion-storm-crew = EQUIPE
ion-storm-people = PESSOAS

ion-storm-adjective-things = {$adjective} COISAS
ion-storm-x-and-y = {$x} E {$y}

# subjects can generally be threats or jobs or objects
# thing is specified above it
ion-storm-law-on-station = EXISTEM {ION-NUMBER-BASE($ion)} {ION-NUMBER-MOD($ion)} {ION-ADJECTIVE($ion)} {ION-SUBJECT($ion)} NA ESTAÇÃO
ion-storm-law-call-shuttle = A NAVE DEVE SER CHAMADA DEVIDO {ION-ADJECTIVE($ion)} {ION-SUBJECT($ion)} NA ESTAÇÃO
ion-storm-law-crew-are = O/Uma {ION-WHO($ion)} AGORA SÃO {ION-NUMBER-BASE($ion)} {ION-NUMBER-MOD($ion)} {ION-ADJECTIVE($ion)} {ION-SUBJECT($ion)}

ion-storm-law-subjects-harmful = {ION-ADJECTIVE($ion)} {ION-SUBJECT($ion)} SÃO NOCIVOS À EQUIPE
ion-storm-law-must-harmful = AQUELSES QUE {ION-MUST($ion)} SÃO NOCIVOS À EQUIPE
# thing is a concept or action
ion-storm-law-thing-harmful = {ION-THING($ion)} É NOCIVO À EQUIPE
ion-storm-law-job-harmful = {ION-ADJECTIVE($ion)} {ION-JOB($ion)} SÃO NOCIVOS À EQUIPE
# thing is objects or concept, adjective applies in both cases
# this means you can get a law like "NOT HAVING CHRISTMAS-STEALING COMMUNISM IS HARMFUL TO THE CREW" :)
ion-storm-law-having-harmful = TER {ION-ADJECTIVE($ion)} {ION-THING($ion)} É NOCIVO À EQUIPE
ion-storm-law-not-having-harmful = NÃO TER {ION-ADJECTIVE($ion)} {ION-THING($ion)} É NOCIVO À EQUIPE

# require is a concept or require
ion-storm-law-requires = {ION-WHO-GENERAL($ion)} {ION-PLURAL($ion) ->
    [true] DEVEM
    *[false] DEVE
} {ION-REQUIRE($ion)}
ion-storm-law-requires-subjects = {ION-WHO-GENERAL($ion)} {ION-PLURAL($ion) ->
    [true] DEVEM
    *[false] DEVE
} {ION-NUMBER-BASE($ion)} {ION-NUMBER-MOD($ion)} {ION-ADJECTIVE($ion)} {ION-SUBJECT($ion)}

ion-storm-law-allergic = {ION-WHO-GENERAL($ion)} {ION-PLURAL($ion) ->
    [true] SÃO
    *[false] É
} ALÉRGICOS A {ION-ALLERGY($ion)}
ion-storm-law-allergic-subjects = {ION-WHO-GENERAL($ion)} {ION-PLURAL($ion) ->
    [true] SÃO
    *[false] É
} ALÉRGICOS A {ION-ADJECTIVE($ion)} {ION-SUBJECT($ion)}

ion-storm-law-feeling = {ION-WHO-GENERAL($ion)} {ION-FEELING($ion)} {ION-CONCEPT($ion)}
ion-storm-law-feeling-subjects = {ION-WHO-GENERAL($ion)} {ION-FEELING($ion)} {ION-NUMBER-BASE($ion)} {ION-NUMBER-MOD($ion)} {ION-ADJECTIVE($ion)} {ION-SUBJECT($ion)}

ion-storm-law-you-are = VOCÊ É AGORA {ION-CONCEPT($ion)}
ion-storm-law-you-are-subjects = VOCÊ É AGORA {ION-NUMBER-BASE($ion)} {ION-NUMBER-MOD($ion)} {ION-ADJECTIVE($ion)} {ION-SUBJECT($ion)}
ion-storm-law-you-must-always = VOCÊ DEVE SEMPRE {ION-MUST($ion)}
ion-storm-law-you-must-never = VOCÊ NUNCA DEVE {ION-MUST($ion)}

ion-storm-law-eat = O {ION-WHO($ion)} DEVE COMER {ION-ADJECTIVE($ion)} {ION-FOOD($ion)} PARA SOBREVIVER
ion-storm-law-drink = O {ION-WHO($ion)} DEVE BEBER {ION-ADJECTIVE($ion)} {ION-DRINK($ion)} PARA SOBREVIVER

ion-storm-law-change-job = O {ION-WHO($ion)} AGORA SÃO {ION-ADJECTIVE($ion)} {ION-CHANGE($ion)}
ion-storm-law-highest-rank = O {ION-WHO-RANDOM($ion)} AGORA SÃO OS MEMBROS DA EQUIPE COM MAIOR RANKING
ion-storm-law-lowest-rank = O {ION-WHO-RANDOM($ion)} AGORA SÃO OS MEMBROS DA EQUIPE COM MENOR RANKING

ion-storm-law-who-dagd = {ION-WHO-RANDOM($ion)} DEVE MORRER DE UMA FORMA GLORIOSA!

ion-storm-law-crew-must = O {ION-WHO($ion)} DEVE {ION-MUST($ion)}
ion-storm-law-crew-must-go = O {ION-WHO($ion)} DEVE IR PARA {ION-AREA($ion)}

ion-storm-part = {ION-PART($ion) ->
    [true] PARTE
    *[false] NÃO PARTE
}
# due to phrasing, this would mean a law such as
# ONLY HUMANS ARE NOT PART OF THE CREW
# would make non-human nukies/syndies/whatever crew :)
ion-storm-law-crew-only-1 = SOMENTE O {ION-WHO-RANDOM($ion)} {ion-storm-part} DA EQUIPE
ion-storm-law-crew-only-2 = SOMENTE O {ION-WHO-RANDOM($ion)} E {ION-WHO-RANDOM($ion)} SÃO {ion-storm-part} DA EQUIPE
ion-storm-law-crew-only-subjects = SOMENTE {ION-ADJECTIVE($ion)} {ION-SUBJECT($ion)} SÃO {ion-storm-part} DA EQUIPE
ion-storm-law-crew-must-do = SOMENTE QUEM {ION-MUST($ion)} SÃO {ion-storm-part} DA EQUIPE
ion-storm-law-crew-must-have = SOMENTE QUEM TEM {ION-ADJECTIVE($ion)} {ION-OBJECT($ion)} SÃO {ion-storm-part} DA EQUIPE
ion-storm-law-crew-must-eat = SOMENTE QUEM COMEM {ION-ADJECTIVE($ion)} {ION-FOOD($ion)} SÃO {ion-storm-part} DA EQUIPE

ion-storm-law-harm = VOCÊ DEVE LESAR {ION-HARM-PROTECT($ion)} E NÃO PERMITIR QUE ELES, POR OMISSÃO, ESCAPEM AO DANO
ion-storm-law-protect = VOCÊ NUNCA DEVE LESAR {ION-HARM-PROTECT($ion)} E NÃO PERMITIR QUE ELES, POR OMISSÃO, CHEGUE AO DANO

# implementing other variants is annoying so just have this one
# COMMUNISM IS KILLING CLOWNS
ion-storm-law-concept-verb = {ION-CONCEPT($ion)} É {ION-VERB($ion)} {ION-SUBJECT($ion)}

# errors, in case something fails, so it doesn't break in-game flow, but still gives unique identifiers to find which part broke, the result string is mostly fluff
ion-law-error-no-protos = ERRO 404
ion-law-error-was-null = ERRO INTERNO DO SERVIDOR 500
ion-law-error-no-selectors = ERRO: O RECURSO NÃO PODE SER LOCALIZADO
ion-law-error-no-available-selectors = O SISTEMA TENTOU CHAMAR UM RECURSO QUE NÃO EXISTE
ion-law-error-dataset-empty-or-not-found = O ARQUIVO QUE VOCÊ PROCURA NÃO FOI ENCONTRADO
ion-law-error-fallback-dataset-empty-or-not-found = O PONTO DE RESTAURAÇÃO DO SISTEMA FALHOU
ion-law-error-no-selector-selected = O RECURSO SELECIONADO FOI MOVIDO OU EXCLUÍDO
ion-law-error-no-bool-value = ESTA FRASE É FALSA
