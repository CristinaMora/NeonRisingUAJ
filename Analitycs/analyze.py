import json
import os
import statistics
from data_definitions import RootDefinition
import matplotlib.pyplot as plt
#import seaborn as sns
import numpy as np
from matplotlib.offsetbox import OffsetImage, AnnotationBbox
import matplotlib.image as mpimg
def processEvents(data):
    sorted_data = sorted(data, key = lambda x: x['timestamp']) # Eventos ordenados por tiempo.
    
    total_events = len(sorted_data)
    stack = [] # Creamos la pila.
    stack.append(RootDefinition(stack)) # Le metemos a la pila los eventos.
    death_positions = []  # Lista para almacenar posiciones de muerte.
    i = 0 # Contador.
    
    while i < total_events:
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
                # Capturar eventos de muerte del jugador.
            if currentEvent.get("eventType") == "PlayerDies" and "position" in currentEvent:
                death_positions.append({
                    "x": currentEvent["position"]["x"],
                    "y": currentEvent["position"]["y"]
                })
    
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
    #
    
    for session in stack[0].sessions:
        accTpArrowMiss=0
        accDangeArrowMiss=0
        accGameLenght = 0
        for game in session.games:
            accGameLenght += game.gameLenght
            pinhosDeads += game.pinhosDeadCount
            enemiesDeads += game.enemyDeadCount
            cameraDeads += len(game.cameraDeadData)
            accTpArrowMiss = len(game.arrowsTp)
            accDangeArrowMiss = len(game.arrowsDamage)
        # Si no hay partidas entonces no se tiene que hacer para evitar division por 0.
        if len(session.games) > 0:
            tpArrowMiss += accTpArrowMiss / len(session.games)
            dangeArrowMiss += accDangeArrowMiss / len(session.games)
            medianTimes += accGameLenght / len(session.games)

        
    
    # Y calculamos las medias de todo.
    if totalSessions >0:
        pinhosDeads /= totalSessions
        enemiesDeads /= totalSessions
        medianTimes /= totalSessions
        cameraDeads /= totalSessions
        tpArrowMiss /= totalSessions
        dangeArrowMiss /= totalSessions

        print("totalSessions: ", totalSessions)

    # Y lo devolvemos.
    return {
            "medianTime": medianTimes,
            "deathsPerPinhos": pinhosDeads,
            "deathsPerEnemies": enemiesDeads,
            "deathsPerCamera": cameraDeads,
            "tpMissedArrows": tpArrowMiss,
            "dangeMissedArrows": dangeArrowMiss,
        }, death_positions
def generateDeathPositionPlot(death_positions, output_path="death_positions.png", background_path=r"back.png"):
    #Genera una imagen con las posiciones de muerte del jugador.
    if not death_positions:
        print("No hay datos de posiciones de muerte para generar la imagen.")
        return

    # Extraer coordenadas x e y.
    x_coords = [pos["x"] for pos in death_positions]
    y_coords = [pos["y"] for pos in death_positions]
    try:
        background_img = mpimg.imread(background_path)
    except FileNotFoundError:
        print(f"No se encontró la imagen de fondo en la ruta: {background_path}")
        return
    # Crear el gráfico.
    plt.figure(figsize=(6, 15))
    plt.imshow(background_img, extent=[-10, 10, -6, 85], aspect='auto', alpha=0.6)
    plt.scatter(x_coords, y_coords, c="red", alpha=0.7, label="Posiciones de muerte")
    plt.title("Posiciones de muerte del jugador")
    plt.xlabel("Posición X")
    plt.ylabel("Posición Y")
    plt.xlim(-10, 10)  # Establecer límites del eje X.
    plt.ylim(-6, 85)   # Establecer límites del eje Y.
    plt.axhline(0, color="black", linewidth=0.5, linestyle="--")
    plt.axvline(0, color="black", linewidth=0.5, linestyle="--")
    plt.grid(color="gray", linestyle="--", linewidth=0.5, alpha=0.7)
    plt.legend()
    plt.tight_layout()

    # Guardar la imagen.
    plt.savefig(output_path)
    plt.close()
if __name__ == '__main__':
    folder_path = './data'
    
    # Metricas que queremos
    medianTime = []
    deathsPerPinhos = []
    deathsPerEnemies = []
    deathsPerCamera = []
    tpMissedArrows = []
    dangeMissedArrows = []
    all_death_positions = []  
    # Recorre todos los JSONS.
    for file_name in os.listdir(folder_path):
        
        if file_name.endswith('.json'):
            # Lectura de JSON.
            file_path = os.path.join(folder_path, file_name)
            # Carga el conetenido del JSON.
            with open(file_path, 'r') as file:
                data = json.load(file)
                # Procesamos los eventos y guardamos los resultados.
                results, death_positions = processEvents(data)
        # Restultados.
        medianTime.append(results['medianTime'])
        deathsPerPinhos.append(results['deathsPerPinhos'])
        deathsPerEnemies.append(results['deathsPerEnemies'])
        deathsPerCamera.append(results['deathsPerCamera'])
        tpMissedArrows.append(results['tpMissedArrows'])
        dangeMissedArrows.append(results['dangeMissedArrows'])
        all_death_positions.extend(death_positions)
    # Calculamos las medias.
    medianT = statistics.median(medianTime)
    pinhos = statistics.median(deathsPerPinhos)
    enemies = statistics.median(deathsPerEnemies)
    camera = statistics.median(deathsPerCamera)
    tpArrows = statistics.median(tpMissedArrows)
    dangeArrows = statistics.median(dangeMissedArrows)
    
    # Escribimos resultados.
    print(f"TIEMPO MEDIO POR SESION: {round(medianT/1000, 2)}s Y VARIANZA: {round(statistics.variance(medianTime), 2)}")
    print(f"MEDIA DE MUERTES POR PINCHOS: {round(pinhos, 2)} Y VARIANZA: {round(statistics.variance(deathsPerPinhos), 2)}")
    print(f"MEDIA DE MUERTES POR ENEMIGOS: {round(enemies, 2)} Y VARIANZA: {round(statistics.variance(deathsPerEnemies), 2)}")
    print(f"MEDIA DE MUERTES POR CAMARA: {round(camera, 2)} Y VARIANZA: {round(statistics.variance(deathsPerCamera), 2)}")
    print(f"MEDIA DE FLECHAS DE TP FALLADAS: {round(tpArrows/100, 4)}%")
    print(f"MEDIA DE FLECHAS DE ATAQUE FALLADAS: {round(dangeArrows/100, 4)}%")
    
    generateDeathPositionPlot(all_death_positions)