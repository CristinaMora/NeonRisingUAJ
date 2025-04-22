class Definition:
    # Constructora.
    def __init__(self, stack, superDef = None) -> None:
        self.stack = stack
        self.superDef = superDef
    # Dado un evento, lo consume.
    def consumeEvent(self, event) -> bool:
        pass
    # Se quita a si mismo de la pila y notifica al padre si existe.
    def pop(self) -> None:
        child = self.stack.pop()
        if self.superDef is not None:
            self.superDef.onPop(child)
    # Lo que se hace cuando un hijo se quita de la pila.
    def onPop(self, childrenDef) -> None:
        pass

class RootDefinition(Definition):
    # Constructora.
    def __init__(self, stack, superDef=None) -> None:
        super().__init__(stack, superDef)
        self.sessions = []
    # Consume los eventos segun su tipo. 
    def consumeEvent(self, event)-> bool:
        if event['eventType'] == "SessionStart":
            self.stack.append(SessionDefinition(self.stack, self))
            return False
        return True
    # Cuando se quita de la pila.
    def onPop(self, childrenDef) -> None:
        print("ONPOP ROOT")
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
    # Consume los eventos segun su tipo.
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
    # Cuando se quita de la pila.
    def onPop(self, childrenDef) -> None:
        print("ONPOP SESSION")
        self.games.append(childrenDef)

class GameDefinition(Definition):
    # Constructora.
    def __init__(self, stack, superDef = None) -> None:
        super().__init__(stack, superDef)
        self.timeGameStart = 0
        self.timeGameEnd = 0
        self.gameLenght = 0
        self.pinhosDeadCount = 0 # 0
        self.enemyDeadCount = 0 # 1
        self.cameraDeadData = [] # 2
        self.arrowsDamage = []
        self.arrowsTp = []
    # Consume los eventos segun su tipo.
    def consumeEvent(self, event) -> bool:
        if event['eventType'] == "PlayerDies": # Evento de muerte del jugador.
            if event['cause'] == 0: # Muere por pinchos.
                self.pinhosDeadCount += 1
            elif event['cause'] == 1: # Muere por enemigos.
                self.enemyDeadCount += 1
            elif event['cause'] == 2: # Muere por camara.
                self.cameraDeadData.append(event['position'])
        elif event['eventType'] == "ArrowShotEvent": # Evento de flecha.
            if event['arrowType'] == 0: # Flecha danyo.
                self.arrowsDamage.append(dict(position = event['position'], hasHit = event['hasHit']))
            elif event['arrowType'] == 1: # Flecha tp.
                self.arrowsTp.append(dict(position = event['position'], hasHit = event['hasHit']))
        elif event['eventType'] == "GameStart": # Evento de inicio de partida.
            self.timeGameStart = event['timestamp']
        elif event['eventType'] == "GameEnd": # Evento de fin de partida.
            self.timeGameEnd = event['timestamp']
            self.gameLenght = self.timeGameEnd - self.timeGameStart
            self.pop()
        return True