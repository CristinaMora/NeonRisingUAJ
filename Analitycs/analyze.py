import json
import os
import statistics
from data_definitions import RootDefinition
import matplotlib.pyplot as plt
import matplotlib.image as mpimg

def processEvents(data):
    sorted_data = sorted(data, key = lambda x: x['timestamp']) # Eventos ordenados por tiempo.
    
    total_events = len(sorted_data)
    stack = []
    stack.append(RootDefinition(stack)) # Lo metemos a la pila los eventos
    death_positions = []  # Lista para almacenar posiciones de muerte
    i = 0
    
    while i < total_events:
        # Guardamos el evento
        currentEvent = sorted_data[i]
        # Si esta vacio
        if len(stack) == 0:
            break
        else:
            currentDef = stack[-1]
            # Analizamos el evento
            consumeEvent = currentDef.consumeEvent(currentEvent)
            # Sumamos al contador
            if consumeEvent:
                i += 1
            # Capturar eventos de muerte del jugador
            if currentEvent.get("eventType") == "PlayerDies" and "position" in currentEvent:
                death_positions.append({
                    "x": currentEvent["position"]["x"],
                    "y": currentEvent["position"]["y"]
                })
    
    return stack, death_positions

# Genera una imagen con las posiciones de muerte del jugador
def generateDeathPositionPlot(death_positions, output_path="death_positions.png", background_path=r"back.png"):
    if not death_positions:
        print("No hay datos de posiciones de muerte para generar la imagen.")
        return

    # Extraer coordenadas x e y
    x_coords = [pos["x"] for pos in death_positions]
    y_coords = [pos["y"] for pos in death_positions]
    try:
        background_img = mpimg.imread(background_path)
    except FileNotFoundError:
        print(f"No se encontró la imagen de fondo en la ruta: {background_path}")
        return
    # Crear el grafico.
    plt.figure(figsize=(6, 15))
    plt.imshow(background_img, extent=[-10, 10, -6, 85], aspect='auto', alpha=0.6)
    plt.scatter(x_coords, y_coords, c="red", alpha=0.7, label="Posiciones de muerte")
    plt.title("Posiciones de muerte del jugador")
    plt.xlabel("Posición X")
    plt.ylabel("Posición Y")
    plt.xlim(-10, 10)  # Establecer limites del eje X
    plt.ylim(-6, 85)   # Establecer limites del eje Y
    plt.axhline(0, color="black", linewidth=0.5, linestyle="--")
    plt.axvline(0, color="black", linewidth=0.5, linestyle="--")
    plt.grid(color="gray", linestyle="--", linewidth=0.5, alpha=0.7)
    plt.legend()
    plt.tight_layout()

    plt.savefig(output_path)
    plt.close()

# Devuelve el numero de eventos de cada tipo. Numero de flechas de tipo X totales, flechas de tipo X falladas, muertes por X totales, etc...
def returnData(stack):
    # Total de sesiones:
    totalSessions = len(stack[0].sessions)
    # Metricas 1:
    dangeArrowMiss = 0
    allDangeArrows = 0
    tpArrowMiss = 0
    allTpArrows = 0
    # Metricas 2:
    pinhosDeads = 0
    enemiesDeads = 0
    cameraDeads = 0
    
    for session in stack[0].sessions:
        for game in session.games:
            # Metricas 1:
            allDangeArrows += len(game.arrowsDamage)
            for arrow in game.arrowsDamage:
                if not arrow['hasHit']:
                    dangeArrowMiss += 1
            allTpArrows += len(game.arrowsTp)
            for arrow in game.arrowsTp:
                if not arrow['hasHit']:
                    tpArrowMiss += 1
            # Metricas 2:
            pinhosDeads += game.pinhosDeadCount
            enemiesDeads += game.enemyDeadCount
            cameraDeads += len(game.cameraDeadData) # Esto es diferente porque en definitions guardamos solo la posicion.

    return {
            "dangeMissedArrows": dangeArrowMiss,
            "allDangeArrows": allDangeArrows,
            "tpMissedArrows": tpArrowMiss,
            "allTpArrows": allTpArrows,
            "allDeathsPerPinhos": pinhosDeads,
            "allDeathsPerEnemies": enemiesDeads,
            "allDeathsPerCamera": cameraDeads,
            "totalSessions": totalSessions
        }

if __name__ == '__main__':
    folder_path = './data'
    
    # Metricas:
    # MMAPA DE CALOR:
    all_death_positions = [] 
    # METRICAS 1:
    allDangeMissedArrowsPerSession = [] # Lista de proporciones de flechas de ataque falladas en cada sesion.
    allDangeHitArrowsPerSession = [] # Lista de proporciones de flechas de ataque acertadas en cada sesion.
    allTpMissedArrowsPerSession = [] # Lista de proporciones de flechas de tp falladas en cada sesion.
    allTpHitArrowsPerSession = [] # Lista de proporciones de flechas de tp acertadas en cada sesion.
    
    allDangeMissedArrows = 0 # Numero total de flechas de ataque falladas.
    allDangeArrows = 0 # Numero total de flechas de ataque lanzadas.
    allTpMissedArrows = 0 # Numero total de flechas de tp falladas.
    allTpArrows = 0 # Numero total de flechas de tp lanzadas.
    # METRICAS 2:
    allDeathsPerPinhosPerSession = [] # Lista de medias de muertes por pinchos en cada sesion.
    allDeathsPerEnemiesPerSession = [] # Lista de medias de muertes por enemigos en cada sesion.
    allDdeathsPerCameraPersession = [] # Lista de medias de muertes por camara en cada sesion.
    
    pinhosProportionsPerSession = [] # Lista de proporciones de muertes por pinchos en cada sesion.
    enemiesProportionsPerSession = [] # Lista de proporciones de muertes por enemigos en cada sesion.
    cameraProportionsPerSession = [] # Lista de proporciones de muertes por camara en cada sesion.
    # OTROS:
    totalSessions = [] # Numero de sesiones de cada archivo.
    
    for file_name in os.listdir(folder_path):
        if file_name.endswith('.json'):
            file_path = os.path.join(folder_path, file_name)
            with open(file_path, 'r') as file:
                data = json.load(file)
                stack, death_positions = processEvents(data)
                resultsPerArchive = returnData(stack) # Resultados del archivo actual.
                totalSessions.append(resultsPerArchive['totalSessions'])
                # Mapa de calor: guardar las posiciones de muerte de la sesion.
                all_death_positions.extend(death_positions)
                # METRICAS 1: actualizar los contadores de flechas para las proporciones globales..
                allDangeMissedArrows += resultsPerArchive['dangeMissedArrows']
                allDangeArrows += resultsPerArchive['allDangeArrows']
                allTpMissedArrows += resultsPerArchive['tpMissedArrows']
                allTpArrows += resultsPerArchive['allTpArrows']                      
                # METRICAS 1: calculo de proporciones por sesion.
                missedArrows = resultsPerArchive['dangeMissedArrows'] / resultsPerArchive['allDangeArrows']
                allDangeMissedArrowsPerSession.append(missedArrows)
                allDangeHitArrowsPerSession.append(1 - missedArrows)
                missedArrows = resultsPerArchive['tpMissedArrows'] / resultsPerArchive['allTpArrows']
                allTpMissedArrowsPerSession.append(missedArrows)
                allTpHitArrowsPerSession.append(1 - missedArrows)
                # METRICAS 2: calculo de medias de la sesion.
                allDeathsPerPinhosPerSession.append(resultsPerArchive['allDeathsPerPinhos'] / resultsPerArchive['totalSessions'])
                allDeathsPerEnemiesPerSession.append(resultsPerArchive['allDeathsPerEnemies'] / resultsPerArchive['totalSessions'])
                allDdeathsPerCameraPersession.append(resultsPerArchive['allDeathsPerCamera'] / resultsPerArchive['totalSessions'])
                # METRICAS 2: calculo de proporciones de la sesion.
                allsSum = resultsPerArchive['allDeathsPerPinhos'] + resultsPerArchive['allDeathsPerEnemies'] + resultsPerArchive['allDeathsPerCamera']
                pinhosProportionsPerSession.append(resultsPerArchive['allDeathsPerPinhos'] / allsSum)
                enemiesProportionsPerSession.append(resultsPerArchive['allDeathsPerEnemies'] / allsSum)
                cameraProportionsPerSession.append(resultsPerArchive['allDeathsPerCamera'] / allsSum)
                

    # METRICAS 1: calculo de las proporciones generales.
    dangeMissedArrowsGlobal = allDangeMissedArrows / allDangeArrows
    dangeHitArrowsGlobal = 1 - dangeMissedArrowsGlobal
    tpMissedArrowsGlobal = allTpMissedArrows / allTpArrows 
    tpHitArrowsGlobal = 1 - tpMissedArrowsGlobal
    # METRICAS 2: calculos de las medias generales
    allSessions = 0
    for i in range(0, len(totalSessions)):
        allSessions += totalSessions[i]
        
    accPinhosDeaths = 0
    for i in range(0, len(allDeathsPerPinhosPerSession)):
        accPinhosDeaths += allDeathsPerPinhosPerSession[i] * totalSessions[i]
    pinhosDeathsGlobal = accPinhosDeaths / allSessions
    
    accEnemiesDeaths = 0
    for i in range(0, len(allDeathsPerEnemiesPerSession)):
        accEnemiesDeaths += allDeathsPerEnemiesPerSession[i] * totalSessions[i]
    enemiesDeathsGlobal = accEnemiesDeaths / allSessions
    
    accCameraDeaths = 0
    for i in range(0, len(allDdeathsPerCameraPersession)):
        accCameraDeaths += allDdeathsPerCameraPersession[i] * totalSessions[i]
    cameraDeathsGlobal = accCameraDeaths / allSessions

    # METRICAS 2: calculo de las proporciones generales.
    accSum = accPinhosDeaths + accEnemiesDeaths + accCameraDeaths      
    
    pinhosProportion = accPinhosDeaths / accSum
    enemiesProportion = accEnemiesDeaths / accSum
    cameraProportion = accCameraDeaths / accSum


    # Escritura de resultados:
    print(f"RESULTADOS:")
    print(f"Total de sesiones: {allSessions}")
    # MAPA DE CALOR:
    generateDeathPositionPlot(all_death_positions)
    # METRICAS 1:
    print(f"\nMETRICAS 1:")
    # Flechas de ataque:
    print(f"\nTASA GLOBAL DE FALLOS DE FLECHA DE ATAQUE: {round(dangeMissedArrowsGlobal, 2)}%")
    print(f"TASA POR SESION DE FALLOS DE FLECHA DE ATAQUE:")
    for i in range(0, len(allDangeMissedArrowsPerSession)):
        print(f"Sesion: {i} Tasa: {round(allDangeMissedArrowsPerSession[i], 2)}") 
    print(f"\nTASA GLOBAL DE ACIERTOS DE FLECHA DE ATAQUE: {round(dangeHitArrowsGlobal, 2)}%")
    print(f"TASA POR SESION DE ACIERTOS DE FLECHA DE ATAQUE:")
    for i in range(0, len(allDangeHitArrowsPerSession)):
        print(f"Sesion: {i} Tasa: {round(allDangeHitArrowsPerSession[i], 2)}") 
    # Flechas de tp:
    print(f"\nTASA GLOBAL DE FALLOS DE FLECHA DE TP: {round(tpMissedArrowsGlobal, 2)}%")
    print(f"TASA POR SESION DE FALLOS DE FLECHA DE TP:")
    for i in range(0, len(allTpMissedArrowsPerSession)):
        print(f"Sesion: {i} Tasa: {round(allTpMissedArrowsPerSession[i], 2)}") 
    print(f"\nTASA GLOBAL DE ACIERTOS DE FLECHA DE TP: {round(tpHitArrowsGlobal, 2)}%")
    print(f"TASA POR SESION DE ACIERTOS DE FLECHA DE TP:")
    for i in range(0, len(allTpHitArrowsPerSession)):
        print(f"Sesion: {i} Tasa: {round(allTpHitArrowsPerSession[i], 2)}") 
    # METRICAS 2:
    print(f"\nMETRICAS 2:")
    # Muertes por pinhos:
    print(f"\nMEDIA GLOBAL DE MUERTES POR PINCHOS: {round(pinhosDeathsGlobal, 2)}")
    print(f"MEDIA POR SESION DE MUERTES POR PINCHOS:")
    for i in range(0, len(allDeathsPerPinhosPerSession)):
        print(f"Sesion: {i} Media: {round(allDeathsPerPinhosPerSession[i], 2)}") 
    # Muertes por enemigos:
    print(f"\nMEDIA GLOBAL DE MUERTES POR ENEMIGOS: {round(enemiesDeathsGlobal, 2)}")
    print(f"MEDIA POR SESION DE MUERTES POR ENEMIGOS:")
    for i in range(0, len(allDeathsPerEnemiesPerSession)):
        print(f"Sesion: {i} Media: {round(allDeathsPerEnemiesPerSession[i], 2)}") 
    # Muertes por camara:
    print(f"\nMEDIA GLOBAL DE MUERTES POR CAMARA: {round(cameraDeathsGlobal, 2)}")
    print(f"MEDIA POR SESION DE MUERTES POR CAMARA:")
    for i in range(0, len(allDdeathsPerCameraPersession)):
        print(f"Sesion: {i} Media: {round(allDdeathsPerCameraPersession[i], 2)}")
    # Proporciones:
    print(f"\nPROPORCION GLOBAL DE MUERTES POR PINCHOS: {round(pinhosProportion, 2)}")
    print(f"PROPORCION POR SESION DE MUERTES POR PINCHOS:")
    for i in range(0, len(pinhosProportionsPerSession)):
        print(f"Sesion: {i} Media: {round(pinhosProportionsPerSession[i], 2)}%") 
    print(f"\nPROPORCION GLOBAL DE MUERTES POR ENEMIGOS: {round(enemiesProportion, 2)}%")
    print(f"PROPORCION POR SESION DE MUERTES POR ENEMIGOS:")
    for i in range(0, len(enemiesProportionsPerSession)):
        print(f"Sesion: {i} Media: {round(enemiesProportionsPerSession[i], 2)}%") 
    print(f"\nPROPORCION GLOBAL DE MUERTES POR CAMARA: {round(cameraProportion, 2)}%")
    print(f"PROPORCION POR SESION DE MUERTES POR CAMARA:")
    for i in range(0, len(cameraProportionsPerSession)):
        print(f"Sesion: {i} Media: {round(cameraProportionsPerSession[i], 2)}%") 