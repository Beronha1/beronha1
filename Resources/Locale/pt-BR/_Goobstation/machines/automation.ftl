signal-port-name-powered = Alimentado
signal-port-description-powered = This port is invoked with HIGH or LOW depending on the machine's power being switched on or off.

signal-port-name-plumbing-input = Encanamento: Entrada
signal-port-description-plumbing-input = A plumbing automation slot to pump liquids into.

signal-port-name-plumbing-output = Plumbing: Output
signal-port-description-plumbing-output = A plumbing automation slot to pump liquids out of.

signal-port-name-plumbing-dispenser = Encanamento: Distribuidor
signal-port-description-plumbing-dispenser = A plumbing automation slot to pump liquids into or out of a dispenser's beaker.

# Robotic Arm

signal-port-name-input-machine = Item: Input Machine
signal-port-description-input-machine = Uma saída de automação de máquina para retirar itens, em vez de pegar do cháo.

signal-port-name-output-machine = Item: Output Machine
signal-port-description-output-machine = A machine automation slot to insert items into, instead of placing them on the floor.

signal-port-name-item-moved = item movido
signal-port-description-item-moved = Signal port that gets pulsed after an item is moved by this arm.

signal-port-name-automation-slot-filter = item: Slot de filtro
signal-port-description-automation-slot-filter = Uma saída de automação para o filtro de uma máquina de automação.

# Reagent Grinder

signal-port-name-automation-slot-beaker = Item: Beaker Slot
signal-port-description-automation-slot-beaker = An automation slot for a liquid-handling machine's beaker.

signal-port-name-automation-slot-input = item: Itens de entrada
signal-port-description-automation-slot-input = Uma saída de automação para armazenamento de itens de entrada de uma máquina.

# Flatpacker

signal-port-name-automation-slot-board = item: Slot de placa
signal-port-description-automation-slot-board = Uma saída de automação para Uma placa de circuito de um flatpacker.

signal-port-name-automation-slot-materials = item: Armazenamento de materiais
signal-port-description-automation-slot-materials = Uma saída de automação para inserir materiais no armazenamento de uma máquina.

# Disposal Unit

signal-port-name-flush = Descarga
signal-port-description-flush = Porta de sinal para alternar o mecanismo de descarga de uma unidade de descarte.

signal-port-name-eject = Ejetar
signal-port-description-eject = Signal port to eject a disposal unit's contents.

signal-port-name-ready = Pronto
signal-port-description-ready = Signal port that gets pulsed after a disposal unit becomes fully pressurized.

# Storage Bin

signal-port-name-automation-slot-storage = item: Armazenamento
signal-port-description-automation-slot-storage = An automation slot for a storage bin's inventory.

signal-port-name-storage-inserted = Inserido
signal-port-description-storage-inserted = Signal port that gets pulsed after an item is inserted into a storage bin.

signal-port-name-storage-removed = Removido
signal-port-description-storage-removed = Signal port that gets pulsed after an item is removed from a storage bin.

# Fax Machine

signal-port-name-automation-slot-paper = item: Papel
signal-port-description-automation-slot-paper = Uma saída de automação para Uma bandeja de papel de uma máquina de fax.

signal-port-name-fax-copy = Copiar fax
signal-port-description-fax-copy = Porta de sinal para copiar o papel de uma máquina de fax.

# Constructor / Interactor

signal-port-name-machine-start = Iniciar
signal-port-description-machine-start = Porta de sinal para iniciar uma máquina uma vez.

signal-port-name-machine-autostart = Auto Start
signal-port-description-machine-autostart = Signal port to control starting after completing automatically.

signal-port-name-machine-started = Iniciado
signal-port-description-machine-started = Signal port that gets pulsed after a machine starts.

signal-port-name-machine-completed = Completed
signal-port-description-machine-completed = Signal port that gets pulsed after a machine completes its work.

signal-port-name-machine-failed = Falhou
signal-port-description-machine-failed = Signal port that gets pulsed after a machine fails to start.

# Interactor

signal-port-name-automation-slot-tool = item: Ferramenta
signal-port-description-automation-slot-tool = Uma saída de automação para Uma ferramenta em mãos de um interator.

signal-port-name-alt-interact = Modo de interação alternativa
signal-port-description-alt-interact = Porta de sinal para alternar o modo de interação alternativa, ou defini-la para um valor HIGH/LOW.

signal-port-name-use-in-hand = Usar Não modo na mão
signal-port-description-use-in-hand = Signal port to toggle use in hand mode, or set it to a HIGH/LOW value. This will ignore targets and use Z or Alt+Z on the held tool.

signal-port-name-harm-mode = Modo agressivo
signal-port-description-harm-mode = Signal port to toggle harm mode, or set it to a HIGH/LOW value. This will hit the target with the held tool like the interactor is in harm mode.

# Autodoc

signal-port-name-automation-slot-autodoc-hand = Item: Autodoc Hand
signal-port-description-automation-slot-autodoc-hand = An automation slot for an autodoc's held organ/part/etc from STORE ITEM / GRAB ITEM instructions.

# Gas Canister

signal-port-name-automation-slot-gas-tank = item: Tanque de gás
signal-port-description-automation-slot-gas-tank = Uma saída de automação para um tanque de gás.

# ChemMaster

signal-port-name-automation-slot-bottles = item: Slot de frascos
signal-port-description-automation-slot-bottles = An automation slot for a ChemMaster's pill or liquid bottle.

# Radiation Collector

signal-port-name-rad-empty = Vazio
signal-port-description-rad-empty = Signal port set to HIGH if the tank is missing or below 33% pressure, LOW otherwise.

signal-port-name-rad-low = Baixo
signal-port-description-rad-low = Signal port set to HIGH if the tank is below 66% pressure, LOW otherwise.

signal-port-name-rad-full = Cheio
signal-port-description-rad-full = Signal port set to HIGH if the tank is above 66% pressure, LOW otherwise.

# Lathe
signal-port-name-lathe-print = Print last recipe
signal-port-description-lathe-print = Signal port that prints the last set recipe when pulsed.
