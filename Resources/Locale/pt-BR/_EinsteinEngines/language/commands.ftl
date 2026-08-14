command-list-langs-desc =Liste os idiomas que sua entidade atual pode falar no momento.
command-list-langs-help =Uso: {$command}

command-saylang-desc =Envie uma mensagem em um idioma específico. Para escolher um idioma, você pode usar o nome do idioma ou sua posição na lista de idiomas.
command-saylang-help =Uso: {$command} <language id> <message>. Exemplo: {$command} TauCetiBasic "Olá, mundo!". Exemplo: {$command} 1 "Olá, mundo!"

command-language-select-desc =Selecione o idioma falado atualmente na sua entidade. Você pode usar o nome do idioma ou sua posição na lista de idiomas.
command-language-select-help =Uso: {$command} <language id>. Exemplo: {$command} 1. Exemplo: {$command} TauCetiBasic

command-language-spoken =Falado:
command-language-understood =Entendido:
command-language-current-entry= {$id}. {$language} - {$name} (atual)
command-language-entry = {$id}. {$language} - {$name}

command-language-invalid-number =O número do idioma deve estar entre 0 e {$total}. Como alternativa, use o nome do idioma.
command-language-invalid-language =O idioma {$id} não existe ou você não consegue falá-lo.

# Toolshed

command-description-language-add =Adiciona um novo idioma à entidade canalizada. Os dois últimos argumentos indicam se deve ser falado/compreendido. Exemplo: 'idioma próprio: adicionar "SolCommon" verdadeiro verdadeiro'
command-description-language-rm =Remove um idioma da entidade canalizada. Funciona de forma semelhante ao idioma: adicionar. Exemplo: 'idioma próprio:rm "TauCetiBasic" verdadeiro verdadeiro'.
command-description-language-lsspoken =Lista todos os idiomas que a entidade pode falar. Exemplo: 'idioma próprio:lsspoken'
command-description-language-lsunderstood =Lista todos os idiomas que a entidade pode entender. Exemplo: 'linguagem própria:lssunderstood'

command-description-translator-addlang =Adiciona um novo idioma de destino à entidade do tradutor canalizado. Consulte idioma: adicionar para obter detalhes.
command-description-translator-rmlang =Remove um idioma de destino da entidade de tradução canalizada. Veja idioma:rm para detalhes.
command-description-translator-addrequired =Adiciona um novo idioma obrigatório à entidade do tradutor canalizado. Exemplo: 'ent 1234 tradutor:addrequired "TauCetiBasic"'
command-description-translator-rmrequired =Remove um idioma obrigatório da entidade de tradução canalizada. Exemplo: 'ent 1234 tradutor:rmrequired "TauCetiBasic"'
command-description-translator-lsspoken =Lista todos os idiomas falados pela entidade de tradução canalizada. Exemplo: 'ent 1234 tradutor:lsspoken'
command-description-translator-lsunderstood =Lista todos os idiomas compreendidos pela entidade de tradução canalizada. Exemplo: 'ent 1234 tradutor:lssunderstood'
command-description-translator-lsrequired =Lista todos os idiomas necessários para a entidade de tradução canalizada. Exemplo: 'ent 1234 tradutor:lsrequired'

command-language-error-this-will-not-work =Isso não funcionará.
command-language-error-not-a-translator =A entidade {$entity} não é tradutora.
