armor-examine-cancel-delayed-knockdown = - [color=green]Cancela completamente[/color] o atraso de derrubada prolongada do cassetete de choque.

armor-examine-modify-delayed-knockdown-delay =
    - { $deltasign ->
          [1] [color=green]Aumenta[/color]
          *[-1] [color=red]Diminui[/color]
      } o atraso de derrubada prolongada do cassetete de choque em [color=lightblue]{NATURALFIXED($amount, 2)} { $amount ->
          [1] segundo
          *[other] segundos
      }[/color].

armor-examine-modify-delayed-knockdown-time =
    - { $deltasign ->
          [1] [color=red]Aumenta[/color]
          *[-1] [color=green]Diminui[/color]
      } o tempo de derrubada prolongada do cassetete de choque em [color=lightblue]{NATURALFIXED($amount, 2)} { $amount ->
          [1] segundo
          *[other] segundos
      }[/color].
