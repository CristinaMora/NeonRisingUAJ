import json
import os
import statistics
from data_definitions import RootDefinition

def processEvents(data):
    sorted_data = sorted(data, key = lambda x: x['timestamp']) # Eventos ordenados por tiempo.
    stack = [] # Creamos la pila.
    stack.append(RootDefinition(stack)) # Le metemos a la pila los eventos.
    i = 0 # Contador.
    
    while i < len(sorted_data):
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
    totalSessions = len(stack[-1].sessions)
    # Guardamos la media de los tipos de muerte de todas las sesiones.
    pinhosDeads = 0
    enemiesDeads = 0
    cameraDeads = 0
    # Guardamos el tiempo medio de todas las partidas de todas las sesiones.
    medianTimes = 0
    # Guardamos la media fallos de cada tipo de flecha por partida.
    tpArrowMiss=0
    dangeArrowMiss=0
    #
    for session in stack[-1].sessions:
        medianTimes += session.medianTimeGames
        for game in session.games:
            pinhosDeads += game.pinhosDeadCount
            enemiesDeads += game.enemyDeadCount
            cameraDeads += game.cameraDeadData
            tpArrowMiss = len(game.arrowsTp)
            dangeArrowMiss = len(game.arrowsDamage)
    # Y calculamos las medias de todo.
    medianTimes /= totalSessions
    pinhosDeads /= totalSessions
    enemiesDeads /= totalSessions
    cameraDeads /= totalSessions
    tpArrowMiss /= len(stack[-1].sessions.games)
    dangeArrowMiss /= len(stack[-1].sessions.games)

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
        # Archivos corruptos empiezan por X.
        if file_name.startswith('X'):
            continue
        # Lectura de JSONS.
        if file_name.endswith('.json'):
            file_path = os.path.join(folder_path, file_name)

            # Carga el conetenido del JSON.
            with open(file_path, 'r') as file:
                data = json.load(file)

            # Porcesamos los eventos y guardamos los resultados.
            results = processEvents(data)

            # Restultados.
            medianTime = results['medianTime']
            deathsPerPinhos = results['deathsPerPinhos']
            deathsPerEnemies = results['deathsPerEnemies']
            deathsPerCamera = results['deathsPerCamera']
            tpMissedArrows = results['tpMissedArrows']
            dangeMissedArrows = results['dangeMissedArrows']
            
            medianT = statistics.median(medianTime)
            pinhos = statistics.median(deathsPerPinhos)
            enemies = statistics.median(deathsPerEnemies)
            camera = statistics.median(deathsPerCamera)
            tpArrows = statistics.median(tpMissedArrows)
            dangeArrows = statistics.median(dangeMissedArrows)
            
            print(f"TIEMPO MEDIO POR SESION: {medianT}, CON VARIANZA: {statistics.variance(medianTime)}")
            print(f"MEDIA DE MUERTES POR PINCHOS: {pinhos}, CON VARIANZA: {statistics.variance(deathsPerPinhos)}")
            print(f"MEDIA DE MUERTES POR ENEMIGOS: {enemies}, CON VARIANZA: {statistics.variance(deathsPerEnemies)}")
            print(f"MEDIA DE MUERTES POR CAMARA: {camera}, CON VARIANZA: {statistics.variance(deathsPerCamera)}")
            print(f"MEDIA DE FLECHAS DE TP FALLADAS: {tpArrows}, CON VARIANZA: {statistics.variance(tpMissedArrows)}")
            print(f"MEDIA DE FLECHAS DE ATAQUE FALLADAS: {dangeArrows}, CON VARIANZA: {statistics.variance(dangeMissedArrows)}")