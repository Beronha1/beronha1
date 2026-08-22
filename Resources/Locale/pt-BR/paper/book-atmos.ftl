# SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
# SPDX-FileCopyrightText: 2023 Vasilis <vascreeper@yahoo.com>
# SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
# SPDX-FileCopyrightText: 2023 dontbetank <59025279+dontbetank@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

book-text-atmos-distro = A rede de distribuição, ou “distro” em resumo, é a linha de vida da estação. Ela é responsável por transportar ar do setor atmosférico por toda a estação.

        Os canos relevantes costumam ser pintados de azul apagado e acolhedor, mas uma forma infalível de identificá-los é usar um scanner de bandeja para rastrear quais canos estão ligados a ventilações ativas da estação.

        A mistura padrão de gás da rede de distribuição é 20 graus Celsius, 78% nitrogênio e 22% oxigênio. Você pode conferir isso usando um analisador de gases em um cano de distro ou qualquer ventilação conectada a ele. Circunstâncias especiais podem exigir misturas especiais.

        Quando se trata de definir a pressão da distribuição, há algumas coisas a considerar. As ventilações ativas regulam a pressão da estação, então, desde que tudo esteja funcionando corretamente, não existe pressão de distribuição “alta demais”.

        Uma pressão de distro mais alta permite que a rede atue como amortecedor entre os mineiros de gás e as ventilações, fornecendo uma quantidade significativa de ar extra que pode ser usado para repressurizar a estação após uma despressurização.

        Uma pressão de distro mais baixa reduz a quantidade de gás perdido caso o distro seja despressurizado, uma forma rápida de lidar com contaminação de distro. Também pode ajudar a desacelerar ou prevenir a sobrepressurização da estação em caso de falhas de ventilações.

        As pressões comuns de distro ficam na faixa de 300 a 375 kPa, mas outras pressões podem ser usadas com conhecimento dos riscos e benefícios.

        A pressão da rede é determinada pela última bomba que estiver bombeando para ela. Para evitar gargalos, todas as bombas entre os mineiros e a última bomba devem estar no máximo de vazão, e quaisquer dispositivos desnecessários devem ser removidos.

        Você pode validar a pressão do distro com um analisador de gases, mas lembre-se de que uma demanda alta causada por eventos como despressurizações pode fazer o distro ficar abaixo do alvo por períodos prolongados. Então, se você ver uma queda de pressão, não entre em pânico — pode ser temporário.

book-text-atmos-waste = A rede de descarte é o sistema principal responsável por manter o ar da estação livre de contaminantes.

        Você pode identificar os canos relevantes pelo tom vermelho apagado agradável ou usando um scanner de bandeja para rastrear quais canos estão conectados aos scrubbers da estação.

        A rede de descarte é usada para transportar gases de descarte para serem filtrados ou despressurizados. O ideal é manter a pressão em 0 kPa, mas às vezes pode ficar em uma baixa pressão não zero durante o uso.

        Técnicos têm a opção de filtrar ou despressurizar os gases de descarte. Embora despressurizar seja mais rápido, filtrar permite reaproveitar esses gases para reciclagem ou venda.

        A rede de descarte também pode ser usada para diagnosticar problemas atmosféricos na estação. Altos níveis de um gás de descarte podem sugerir um grande vazamento, enquanto a presença de gases não pertencentes ao descarte pode indicar problema de configuração dos scrubbers ou conexão física inadequada. Se os gases estiverem com temperatura alta, isso pode indicar incêndio.

book-text-atmos-alarms = Alarmes de ar ficam espalhados pelas estações para permitir o gerenciamento e monitoramento da atmosfera local.

            A interface do alarme de ar oferece aos técnicos uma lista de sensores conectados, suas leituras e a capacidade de ajustar limiares. Esses limiares são usados para determinar a condição de alarme do sistema de ar. Os técnicos também podem usar a interface para definir pressões alvo para as ventilações e configurar velocidades operacionais e gases alvo para os scrubbers.

            Embora a interface permita ajuste fino dos dispositivos sob controle do alarme de ar, também há vários modos disponíveis para configuração rápida do alarme. Esses modos são trocados automaticamente quando o estado do alarme muda:
            - Filtragem: o modo padrão
            - Filtragem (ampla): um modo de filtragem que altera o funcionamento dos scrubbers para limpar uma área maior
            - Encher: desabilita os scrubbers e define as ventilações para pressão máxima
            - Pânico: desabilita as ventilações e define os scrubbers para sucção

            a multitool ou configurador de rede pode ser usada para ligar dispositivos aos alarmes de ar.

book-text-atmos-vents =
    Abaixo está um guia de referência rápida para vários dispositivos atmosféricos:

                Ventilações passivas:
                Essas ventilações não exigem energia; permitem que o gás flua livremente tanto para dentro quanto para fora da rede de canos à qual estão conectadas.

                Ventilações ativas:
                São as mais comuns da estação. Têm uma bomba interna e exigem energia. Por padrão, bombeiam gás para fora dos canos apenas, e apenas até 101 kPa. No entanto, podem ser reconfiguradas pelo alarme de ar. Também travam se o ambiente estiver abaixo de 1 kPa, para evitar bombeamento de gás para o espaço.

                Scrubbers:
                Esses dispositivos permitem remover gases do ambiente e colocar no cano conectado. Podem ser configurados para selecionar gases específicos quando conectados a um alarme de ar.

                Injetores de ar:
                Injetores são semelhantes às ventilações ativas, mas não têm bomba interna e não exigem energia. Não podem ser configurados, mas continuam bombeando gás a pressões bem maiores.

