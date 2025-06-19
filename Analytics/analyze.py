import json
import os
from data_definitions import RootDefinition
import matplotlib.pyplot as plt
import matplotlib.image as mpimg
from PDFWriter import PDFWriter 

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
    deathObjects = {}
    
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
            for object in game.objects: # Guardamos los objetos que han causado la muerte.
                if object['name'] in deathObjects:
                    deathObjects[object['name']] += 1
                else:
                    deathObjects[object['name']] = 1

    return {
            "dangeMissedArrows": dangeArrowMiss,
            "allDangeArrows": allDangeArrows,
            "tpMissedArrows": tpArrowMiss,
            "allTpArrows": allTpArrows,
            "allDeathsPerPinhos": pinhosDeads,
            "allDeathsPerEnemies": enemiesDeads,
            "allDeathsPerCamera": cameraDeads,
            "totalSessions": totalSessions,
            "deathObjects": deathObjects
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
    
    deathObjectsPerSession = [] # Lista con los diccionarios de objetos que han matado al jugador en cada sesion.
    # OTROS:
    totalSessions = [] # Numero de sesiones de cada archivo.
    pdfWriter = PDFWriter ("output.pdf")  
    
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
                if not resultsPerArchive['allDangeArrows'] == 0:
                    missedArrows = resultsPerArchive['dangeMissedArrows'] / resultsPerArchive['allDangeArrows']
                    allDangeMissedArrowsPerSession.append(missedArrows)
                    allDangeHitArrowsPerSession.append(1 - missedArrows)
                if not resultsPerArchive['allDangeArrows'] == 0:
                    missedArrows = resultsPerArchive['tpMissedArrows'] / resultsPerArchive['allTpArrows']
                    allTpMissedArrowsPerSession.append(missedArrows)
                    allTpHitArrowsPerSession.append(1 - missedArrows)
                # METRICAS 2: calculo de medias de la sesion.
                allDeathsPerPinhosPerSession.append(resultsPerArchive['allDeathsPerPinhos'] / resultsPerArchive['totalSessions'])
                allDeathsPerEnemiesPerSession.append(resultsPerArchive['allDeathsPerEnemies'] / resultsPerArchive['totalSessions'])
                allDdeathsPerCameraPersession.append(resultsPerArchive['allDeathsPerCamera'] / resultsPerArchive['totalSessions'])
                # METRICAS 2: calculo de proporciones de la sesion.
                allsSum = resultsPerArchive['allDeathsPerPinhos'] + resultsPerArchive['allDeathsPerEnemies'] + resultsPerArchive['allDeathsPerCamera']
                if not resultsPerArchive['allDeathsPerPinhos'] == 0 and not allsSum == 0:
                    pinhosProportionsPerSession.append(resultsPerArchive['allDeathsPerPinhos'] / allsSum)
                if not resultsPerArchive['allDeathsPerEnemies'] == 0 and not allsSum == 0:
                    enemiesProportionsPerSession.append(resultsPerArchive['allDeathsPerEnemies'] / allsSum)
                if not resultsPerArchive['allDeathsPerCamera'] == 0 and not allsSum == 0:
                    cameraProportionsPerSession.append(resultsPerArchive['allDeathsPerCamera'] / allsSum)
                # METRICAS 2: guardar los objetos que han matado al jugador.
                deathObjectsPerSession.append(resultsPerArchive['deathObjects'])
                

    # METRICAS 1: calculo de las proporciones generales.
    dangeMissedArrowsGlobal = allDangeMissedArrows / allDangeArrows if allDangeArrows != 0 else 0
    dangeHitArrowsGlobal = 1 - dangeMissedArrowsGlobal
    tpMissedArrowsGlobal = allTpMissedArrows / allTpArrows if allTpArrows != 0 else 0
    tpHitArrowsGlobal = 1 - tpMissedArrowsGlobal
    # METRICAS 2: calculos de las medias generales
    allSessions = 0
    for i in range(0, len(totalSessions)):
        allSessions += totalSessions[i]
        
    accPinhosDeaths = 0
    for i in range(0, len(allDeathsPerPinhosPerSession)):
        accPinhosDeaths += allDeathsPerPinhosPerSession[i] * totalSessions[i]
    pinhosDeathsGlobal = accPinhosDeaths / allSessions if allSessions != 0 else 0
    
    accEnemiesDeaths = 0
    for i in range(0, len(allDeathsPerEnemiesPerSession)):
        accEnemiesDeaths += allDeathsPerEnemiesPerSession[i] * totalSessions[i]
    enemiesDeathsGlobal = accEnemiesDeaths / allSessions if allSessions != 0 else 0
    
    accCameraDeaths = 0
    for i in range(0, len(allDdeathsPerCameraPersession)):
        accCameraDeaths += allDdeathsPerCameraPersession[i] * totalSessions[i]
    cameraDeathsGlobal = accCameraDeaths / allSessions if allSessions != 0 else 0

    # METRICAS 2: calculo de las proporciones generales.
    accSum = accPinhosDeaths + accEnemiesDeaths + accCameraDeaths      
    
    pinhosProportion = accPinhosDeaths / accSum if accSum != 0 else 0
    enemiesProportion = accEnemiesDeaths / accSum if accSum != 0 else 0
    cameraProportion = accCameraDeaths / accSum if accSum != 0 else 0
    
    # METRICAS 2: computo total de los objetos que han matado al jugador.
    deathObjectsGlobal = {}
    for i in range(0, len(deathObjectsPerSession)):
        for object in deathObjectsPerSession[i]:
            if object in deathObjectsGlobal:
                deathObjectsGlobal[object] += deathObjectsPerSession[i][object]
            else:
                deathObjectsGlobal[object] = deathObjectsPerSession[i][object]
                

    # Escritura de resultados:
    pdfWriter.addText(f"RESULTADOS:", 12)
    pdfWriter.addText(f"Total de archivos: {len(totalSessions)}", 8)
    pdfWriter.addText(f"Total de sesiones: {allSessions}", 8)
    # MAPA DE CALOR:
    generateDeathPositionPlot(all_death_positions)
    # METRICAS 1:
    pdfWriter.addText(f"\nMETRICAS 1:", 10)
    # Flechas de ataque:
    pdfWriter.addText(f"\nTASA GLOBAL DE FALLOS DE FLECHA DE ATAQUE: {round(dangeMissedArrowsGlobal, 2)}%", 8)
    pdfWriter.addText(f"TASA POR ARCHIVO DE FALLOS DE FLECHA DE ATAQUE:", 8)
    for i in range(0, len(allDangeMissedArrowsPerSession)):
        pdfWriter.addText(f"Archivo: {i}, Tasa: {round(allDangeMissedArrowsPerSession[i], 2)}", 8) 
    pdfWriter.addText(f"\nTASA GLOBAL DE ACIERTOS DE FLECHA DE ATAQUE: {round(dangeHitArrowsGlobal, 2)}%", 8)
    pdfWriter.addText(f"TASA POR ARCHIVO DE ACIERTOS DE FLECHA DE ATAQUE:", 8)
    for i in range(0, len(allDangeHitArrowsPerSession)):
        pdfWriter.addText(f"Archivo: {i}, Tasa: {round(allDangeHitArrowsPerSession[i], 2)}", 8) 
    # Flechas de tp:
    pdfWriter.addText(f"\nTASA GLOBAL DE FALLOS DE FLECHA DE TP: {round(tpMissedArrowsGlobal, 2)}%", 8)
    pdfWriter.addText(f"TASA POR ARCHIVO DE FALLOS DE FLECHA DE TP:", 8)
    for i in range(0, len(allTpMissedArrowsPerSession)):
        pdfWriter.addText(f"Archivo: {i}, Tasa: {round(allTpMissedArrowsPerSession[i], 2)}", 8) 
    pdfWriter.addText(f"\nTASA GLOBAL DE ACIERTOS DE FLECHA DE TP: {round(tpHitArrowsGlobal, 2)}%", 8)
    pdfWriter.addText(f"TASA POR ARCHIVO DE ACIERTOS DE FLECHA DE TP:", 8)
    for i in range(0, len(allTpHitArrowsPerSession)):
        pdfWriter.addText(f"Archivo: {i}, Tasa: {round(allTpHitArrowsPerSession[i], 2)}", 8) 
    # METRICAS 2:
    pdfWriter.addText(f"\nMETRICAS 2:", 10)
    # Muertes por pinhos:
    pdfWriter.addText(f"\nMEDIA GLOBAL DE MUERTES POR PINCHOS: {round(pinhosDeathsGlobal, 2)}", 8)
    pdfWriter.addText(f"MEDIA POR ARCHIVO DE MUERTES POR PINCHOS:", 8)
    for i in range(0, len(allDeathsPerPinhosPerSession)):
        pdfWriter.addText(f"Archivo: {i}, Media: {round(allDeathsPerPinhosPerSession[i], 2)}", 8) 
    # Muertes por enemigos:
    pdfWriter.addText(f"\nMEDIA GLOBAL DE MUERTES POR ENEMIGOS: {round(enemiesDeathsGlobal, 2)}", 8)
    pdfWriter.addText(f"MEDIA POR ARCHIVO DE MUERTES POR ENEMIGOS:", 8)
    for i in range(0, len(allDeathsPerEnemiesPerSession)):
        pdfWriter.addText(f"Archivo: {i}, Media: {round(allDeathsPerEnemiesPerSession[i], 2)}", 8) 
    # Muertes por camara:
    pdfWriter.addText(f"\nMEDIA GLOBAL DE MUERTES POR CAMARA: {round(cameraDeathsGlobal, 2)}", 8)
    pdfWriter.addText(f"MEDIA POR ARCHIVO DE MUERTES POR CAMARA:", 8)
    for i in range(0, len(allDdeathsPerCameraPersession)):
        pdfWriter.addText(f"Archivo: {i}, Media: {round(allDdeathsPerCameraPersession[i], 2)}", 8)
    # Proporciones:
    pdfWriter.addText(f"\nPROPORCION GLOBAL DE MUERTES POR PINCHOS: {round(pinhosProportion, 2)}", 8)
    pdfWriter.addText(f"PROPORCION POR ARCHIVO DE MUERTES POR PINCHOS:", 8)
    for i in range(0, len(pinhosProportionsPerSession)):
        pdfWriter.addText(f"Archivo: {i}, Media: {round(pinhosProportionsPerSession[i], 2)}%", 8) 
    pdfWriter.addText(f"\nPROPORCION GLOBAL DE MUERTES POR ENEMIGOS: {round(enemiesProportion, 2)}%", 8)
    pdfWriter.addText(f"PROPORCION POR ARCHIVO DE MUERTES POR ENEMIGOS:", 8)
    for i in range(0, len(enemiesProportionsPerSession)):
        pdfWriter.addText(f"Archivo: {i}, Media: {round(enemiesProportionsPerSession[i], 2)}%", 8) 
    pdfWriter.addText(f"\nPROPORCION GLOBAL DE MUERTES POR CAMARA: {round(cameraProportion, 2)}%", 8)
    pdfWriter.addText(f"PROPORCION POR ARCHIVO DE MUERTES POR CAMARA:", 8)
    for i in range(0, len(cameraProportionsPerSession)):
        pdfWriter.addText(f"Archivo: {i}, Media: {round(cameraProportionsPerSession[i], 2)}%", 8)
    # Frecuencias:
    pdfWriter.addText(f"\nTABLA DE FRECUENCIAS GLOBAL DE CADA OBJETO:", 8)
    deathObjectsGlobal = dict(sorted(deathObjectsGlobal.items(), key = lambda item: item[1], reverse = True))
    keys = list(deathObjectsGlobal.keys())
    values = list(deathObjectsGlobal.values())
    for i in range(0, len(deathObjectsGlobal)):
        pdfWriter.addText(f"Objeto: {keys[i]}, Veces: {values[i]}", 8)
    pdfWriter.addText(f"TABLA DE FRECUENCIAS POR ARCHIVO DE CADA OBJETO:", 8)
    for i in range(0, len(deathObjectsPerSession)):
        deathObjectsPerSession[i] = dict(sorted(deathObjectsPerSession[i].items(), key = lambda item: item[1], reverse = True))
        keys = list(deathObjectsPerSession[i].keys())
        values = list(deathObjectsPerSession[i].values())
        for j in range(0, len(deathObjectsPerSession[i])):
            pdfWriter.addText(f"Archivo: {i}, Objeto: {keys[j]}, Veces: {values[j]}", 8)
    # Graficos:
    pdfWriter.addText(f"\nGRAFICOS:", 10)
    keys = list(deathObjectsGlobal.keys())
    values = list(deathObjectsGlobal.values())
    pdfWriter.addGraph(keys, values)
    pdfWriter.addText("", 25)
    for i in range(0, len(deathObjectsPerSession)):
        deathObjectsPerSession[i] = dict(sorted(deathObjectsPerSession[i].items(), key = lambda item: item[1], reverse = True))
        keys = list(deathObjectsPerSession[i].keys())
        values = list(deathObjectsPerSession[i].values())
        pdfWriter.addGraph(keys, values)
        pdfWriter.addText("", 25)

    pdfWriter.close()
    