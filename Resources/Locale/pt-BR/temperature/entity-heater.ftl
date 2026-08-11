-entity-heater-setting-name =
    { $setting ->
        [off] desligado
        [low] baixo
        [medium] médio
        [high] alto
       *[other] desconhecido
    }

entity-heater-examined = Está definido como { $setting ->
    [off] [color=gray]{ -entity-heater-setting-name(setting: "off") }[/color]
    [low] [color=yellow]{ -entity-heater-setting-name(setting: "low") }[/color]
    [medium] [color=orange]{ -entity-heater-setting-name(setting: "medium") }[/color]
    [high] [color=red]{ -entity-heater-setting-name(setting: "high") }[/color]
   *[other] [color=purple]{ -entity-heater-setting-name(setting: "other") }[/color]
}.
entity-heater-switch-setting = Mude para { -entity-heater-setting-name(setting: $setting) }
entity-heater-switched-setting = Mudou para { -entity-heater-setting-name(setting: $setting) }.

