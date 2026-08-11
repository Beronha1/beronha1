ent-SpawnPointGhostBlob = Gerador de Blob
    .suffix = DEBUG, Spawner de Papel Fantasma
    .desc = { ent-MarkerBase.desc }
ent-MobBlobPod = Queda de Blob
    .desc = Um combatente blob comum.
ent-MobBlobBlobbernaut = Blobbernaut
    .desc = Um combatente blob de elite.
ent-BaseBlob = blob básico.
    .desc = { "" }
ent-NormalBlobTile = Tile Blob Regular
    .desc = Uma parte comum do blob necessária para Uma construção de blocos mais avançados.
ent-CoreBlobTile = Núcleo do Blob
    .desc = O órgão mais importante do blob. Ao destruir o núcleo, Uma infecção cessará.
ent-FactoryBlobTile = Fábrica de Blob
    .desc = Gera Drops de Blob e Blobbernauts com o tempo.
ent-ResourceBlobTile = Blob de Recurso
    .desc = Produz recursos para o blob.
ent-NodeBlobTile = Nó de Blob
    .desc = Uma mini versão do núcleo que permite colocar blocos especiais de blob ao redor dele.
ent-StrongBlobTile = Tile de Blob Forte
    .desc = Uma versão reforçada da peça normal. Não permite Uma passagem de ar e protege contra dano bruto.
ent-ReflectiveBlobTile = Tile de Blob Reflexivo
    .desc = Reflete lasers, mas não protege tão bem contra dano bruto.
    .desc = { "" }
objective-issuer-blob= bolha


ghost-role-information-blobbernaut-name = Blobbernaut
ghost-role-information-blobbernaut-description = Você é um Blobbernaut. Você deve defender o núcleo do blob. Use + ou +e Não chat para falar com Uma mente do blob.

ghost-role-information-blob-name= bolha
ghost-role-information-blob-description = Você é Uma Infecção Blob. Consuma Uma estação.

roles-antag-blob-name= bolha
roles-antag-blob-objective = Alcançar massa crítica.

guide-entry-blob= bolha

# Popups
blob-target-normal-blob-invalid = Tipo de blob errado, selecione um blob normal.
blob-target-factory-blob-invalid = Tipo de blob errado, selecione um blob fábrica.
blob-target-node-blob-invalid = Tipo de blob errado, selecione um blob de nó.
blob-target-close-to-resource = Muito próximo de outro blob de recurso.
blob-target-nearby-not-node = Nenhum blob de nó ou de recurso por perto.
blob-target-close-to-node = Muito próximo de outro nó.
blob-target-already-produce-blobbernaut = Esta fábrica já produziu um blobbernaut.
blob-cant-split = Você não pode dividir o núcleo do blob.
blob-not-have-nodes = Você não tem nós.
blob-not-enough-resources = Recursos insuficientes.
blob-help = Só Deus pode te ajudar.
blob-swap-chem = Em desenvolvimento.
blob-mob-attack-blob = Você não pode atacar um blob.
blob-get-resource= +{$point}
blob-spent-resource= -{$point}
blobberaut-not-on-blob-tile = Você está morrendo por não estar em blocos de blob.
carrier-blob-alert = Você tem { $second } segundos antes da transformação.

blob-mob-zombify-second-start = { $pod } começou Uma te transformar em zumbi.
blob-mob-zombify-third-start = { $pod } começou Uma transformar { $target } em zumbi.

blob-mob-zombify-second-end = { $pod } te transformou em zumbi.
blob-mob-zombify-third-end = { $pod } transformou { $target } em zumbi.

blobberaut-factory-destroy = destruir fábrica
blob-target-already-connected = já conectado


# UI
blob-chem-swap-ui-window-name = Trocar químicos

blob-alert-out-off-station = O blob foi removido porque foi encontrado fora da estação!

# Announcment
blob-alert-recall-shuttle = Uma nave de emergência não pode ser enviada enquanto houver biohazard de nível 5 na estação.
blob-alert-detect = Surto de biohazard de nível 5 confirmado Uma bordo da estação. Todo o pessoal deve conter o surto.
blob-alert-critical = Nível de biohazard crítico, códigos nucleares foram enviados para Uma estação. O Comando Central ordena que qualquer pessoal restante ative o mecanismo de autodestruição.
blob-alert-critical-NoNukeCode = Nível de biohazard crítico. O Comando Central ordena que qualquer pessoal restante procure abrigo e espere resgate.
blob-alert-shuttle-arrived = Biohazard detectado Uma bordo. Todos os tripulantes devem evacuar imediatamente.

# Actions
blob-teleport-to-node-action-name = Ir para o Nó (0)
blob-teleport-to-node-action-desc = Teletransporta você para um nó de blob aleatório.
blob-help-action-name = Ajuda
blob-help-action-desc = Obtém informações básicas sobre jogar como blob.

# Ghost role
blob-carrier-role-name = Carregador de Blob
blob-carrier-role-desc =  Uma criatura infectada pelo blob.
blob-carrier-role-rules = Você é um antagonista. Você tem 10 minutos antes de se transformar em um blob.
                        Use esse tempo para encontrar um local seguro na estação. Lembre-se de que ficará muito fraco logo após a transformação.
blob-carrier-role-greeting = Você é um portador de Blob. Encontre um local discreto na estação e transforme-se em um Blob. Transforme Uma estação em massa e seus habitantes em seus servos. Todos nós somos Blobs.

# Verbs
blob-pod-verb-zombify = Zombificar
blob-verb-remove-blob-tile = Remover Blob

# Alerts
blob-resource-alert-name = Recursos do Núcleo
blob-resource-alert-desc = Seus recursos produzidos pelos blobs núcleo e de recurso. Use-os para expandir e criar blobs especiais.
blob-health-alert-name = Saúde do Núcleo
blob-health-alert-desc = Uma saúde do seu núcleo. Você morrerá se atingir zero.

# Greeting
blob-role-greeting =
    Você é um blob - uma criatura espacial parasitária capaz de destruir estações inteiras.
        Seu objetivo é sobreviver e crescer o máximo possível.
        Você é quase invulnerável a dano físico, mas o calor ainda pode te ferir.
        Use Alt+LMB para atualizar blocos normais de blob para fortes e fortes para refletivos.
        Certifique-se de colocar blobs de recurso para gerar recursos.
        Lembre-se de que os blobs de recurso e fábricas só funcionarão quando ao lado de nós ou núcleos de blob.
        Você pode usar + ou +e no chat para usar a Mente do Blob e falar com seus servos.
blob-zombie-greeting = Você foi infectado e gerado por uma espora de blob. Agora você deve ajudar o blob Uma tomar Uma estação. Use +e Não chat para falar em Mente do Blob.

# End round
blob-round-end-agent-name = infecção blob

# Objectivies
objective-condition-blob-capture-title = Tomar Uma estação
objective-condition-blob-capture-description = Seu único objetivo é tomar conta de toda Uma estação. Você precisa ter pelo menos {$count} blocos de blob.
objective-condition-success = { $condition } | [color={ $markupColor }]Sucesso![/color]
objective-condition-fail = { $condition } | [color={ $markupColor }]Falha![/color] ({ $progress }%)

# Admin Verbs

admin-verb-make-blob = Transforme o alvo em um portador de blob.
admin-verb-text-make-blob = Criar Portador de Blob

# Language
language-Blob-name= bolha
chat-language-Blob-name= bolha
language-Blob-description= Bobagem! Bolha, bolha!
