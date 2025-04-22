import json
import os
import statistics
from data_definitions import RootDefinition

def processEvents(data):
    sorted_data = sorted(data, key = lambda x: x['timestamp']) # Eventos ordenados por tiempo.
    
    total_events = len(sorted_data)
    stack = [] # Creamos la pila.
    stack.append(RootDefinition(stack)) # Le metemos a la pila los eventos.
    i = 0 # Contador.
    
    while i < total_events-1:
        # Guardamos el evento.
        currentEvent = sorted_data[i]
        # Si esta vacio, adios.
        if len(stack) == 0:
            break
        else:
            currentDef = stack[-1]
            # Analizamos el evento.
            consumeEvent = currentDef.consumeEvent(currentEvent)
            # Sumamos al contador.
            if consumeEvent:
                i += 1
    
    # Constante de todas las sesiones.
    totalSessions = len(stack[0].sessions)
    # Guardamos la media de los tipos de muerte de todas las sesiones.
    pinhosDeads = 0
    enemiesDeads = 0
    cameraDeads = 0
    # Guardamos el tiempo medio de todas las partidas de todas las sesiones.
    medianTimes = 0
    # Guardamos la media fallos de cada tipo de flecha por partida.
    tpArrowMiss=0
    dangeArrowMiss=0
    gameLenght = 0
    #
    
    print(stack[0].sessions)
    for session in stack[0].sessions:
        accTpArrowMiss=0
        accDangeArrowMiss=0
        accGameLenght = 0
        print(session.games)
        for game in session.games:
            accGameLenght += game.gameLenght
            pinhosDeads += game.pinhosDeadCount
            enemiesDeads += game.enemyDeadCount
            cameraDeads += game.cameraDeadData
            accTpArrowMiss = len(game.arrowsTp)
            accDangeArrowMiss = len(game.arrowsDamage)
        tpArrowMiss += accTpArrowMiss / len(session.games)
        dangeArrowMiss += accDangeArrowMiss / len(session.games)
        gameLenght += accGameLenght / len(session.games)

        
    
    # Y calculamos las medias de todo.
    if totalSessions >0:
        pinhosDeads /= totalSessions
        enemiesDeads /= totalSessions
        medianTimes /= totalSessions
        cameraDeads /= totalSessions
        tpArrowMiss /= totalSessions
        dangeArrowMiss /= totalSessions
        gameLenght /= totalSessions

    return {
            "medianTime": medianTimes,
            "deathsPerPinhos": pinhosDeads,
            "deathsPerEnemies": enemiesDeads,
            "deathsPerCamera": cameraDeads,
            "tpMissedArrows": tpArrowMiss,
            "dangeMissedArrows": dangeArrowMiss,
        }
        
if __name__ == '__main__':
    folder_path = './data'
    
    medianTime = []
    deathsPerPinhos = []
    deathsPerEnemies = []
    deathsPerCamera = []
    tpMissedArrows = []
    dangeMissedArrows = []
    
    for file_name in os.listdir(folder_path):
        
        if file_name.endswith('.json'):  # Process only JSON files
            # Lectura de JSONS.
            file_path = os.path.join(folder_path, file_name)
            # Carga el conetenido del JSON.
            with open(file_path, 'r') as file:
                data = json.load(file)
                # Porcesamos los eventos y guardamos los resultados.
                results = processEvents(data)
        # Restultados.
        medianTime.append(results['medianTime'])
        deathsPerPinhos.append(results['deathsPerPinhos'])
        deathsPerEnemies.append(results['deathsPerEnemies'])
        deathsPerCamera.append(results['deathsPerCamera'])
        tpMissedArrows.append(results['tpMissedArrows'])
        dangeMissedArrows.append(results['dangeMissedArrows'])
        
    medianT = statistics.median(medianTime)
    pinhos = statistics.median(deathsPerPinhos)
    enemies = statistics.median(deathsPerEnemies)
    camera = statistics.median(deathsPerCamera)
    tpArrows = statistics.median(tpMissedArrows)
    dangeArrows = statistics.median(dangeMissedArrows)
    
    print(f"TIEMPO MEDIO POR SESION: {medianT}")
    print(f"MEDIA DE MUERTES POR PINCHOS: {pinhos}")
    print(f"MEDIA DE MUERTES POR ENEMIGOS: {enemies}")
    print(f"MEDIA DE MUERTES POR CAMARA: {camera}")
    print(f"MEDIA DE FLECHAS DE TP FALLADAS: {tpArrows}")
    print(f"MEDIA DE FLECHAS DE ATAQUE FALLADAS: {dangeArrows}")