class Definition:
    def __init__(self, stack, superDef = None) -> None:
        self.stack = stack
        self.superDef = superDef

    def consumeEvent(self, event) -> bool:
        pass
    # Se quita a si mismo de la pila y notifica al padre si existe
    def pop(self) -> None:
        child = self.stack.pop()
        if self.superDef is not None:
            self.superDef.onPop(child)
    # Lo que se hace cuando un hijo se quita de la pila
    def onPop(self, childrenDef) -> None:
        pass

class RootDefinition(Definition):
    def __init__(self, stack, superDef=None) -> None:
        super().__init__(stack, superDef)
        self.sessions = []

    def consumeEvent(self, event)-> bool:
        if event['eventType'] == "SessionStart":
            self.stack.append(SessionDefinition(self.stack, self))
            return False
        return True
    # Cuando se quita de la pila
    def onPop(self, childrenDef) -> None:
        self.sessions.append(childrenDef)
class SessionDefinition(Definition):
    # Constructora.
    def __init__(self, stack, superDef = None) -> None:
        super().__init__(stack, superDef)
        self.timeSessionStart = None
        self.timeSessionEnd = None
        self.playedTime = None
        self.sessionId = None
        self.games = []

    def consumeEvent(self, event)-> bool:
        if event['eventType'] == "GameStart":
            self.stack.append(GameDefinition(self.stack, self))
            return False
        elif event['eventType'] == "SessionStart":
            self.timeSessionStart = event['timestamp']
        elif event['eventType'] == "SessionEnd":
            self.timeSessionEnd = event['timestamp']
            self.playedTime = self.timeSessionEnd - self.timeSessionStart
            self.pop()
        return True
    # Cuando se quita de la pila
    def onPop(self, childrenDef) -> None:
        self.games.append(childrenDef)

class GameDefinition(Definition):
    def __init__(self, stack, superDef = None) -> None:
        super().__init__(stack, superDef)
        self.timeGameStart = 0
        self.timeGameEnd = 0
        self.gameLenght = 0
        # METRICAS 1:
        self.arrowsDamage = [] # Diccionario con la posicion y si la flecha de ataque a hitteado o no.
        self.arrowsTp = [] # Diccionario con la posicion y si la flecha de tp a hitteado o no.
        # METRCIAS 2:
        self.pinhosDeadCount = 0 # 0. Contiene las veces que muere por pinchos.
        self.enemyDeadCount = 0 # 1. Contiene las veces que muere por enemigos.
        self.cameraDeadData = [] # 2. Contiene posicion donde muere el jugador.
        self.objects = [] # 

    def consumeEvent(self, event) -> bool:
        if event['eventType'] == "PlayerDies": # Evento de muerte del jugador
            if event['cause'] == 0: # Muere por pinchos
                self.pinhosDeadCount += 1
                self.objects.append(dict(position = event['position'], name = event['name']))
            elif event['cause'] == 1: # Muere por enemigos
                self.enemyDeadCount += 1
                self.objects.append(dict(position = event['position'], name = event['name']))
            elif event['cause'] == 2: # Muere por camara
                self.cameraDeadData.append(event['position'])
        elif event['eventType'] == "ArrowShotEvent": # Evento de flecha
            if event['arrowType'] == 0: # Flecha danyo
                self.arrowsDamage.append(dict(position = event['position'], hasHit = event['hasHit']))
            elif event['arrowType'] == 1: # Flecha tp
                self.arrowsTp.append(dict(position = event['position'], hasHit = event['hasHit']))
        elif event['eventType'] == "GameStart": # Evento de inicio de partida
            self.timeGameStart = event['timestamp']
        elif event['eventType'] == "GameEnd": # Evento de fin de partida
            self.timeGameEnd = event['timestamp']
            self.gameLenght = self.timeGameEnd - self.timeGameStart
            self.pop()
        return True