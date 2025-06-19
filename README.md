# Grupo 01

El material de entrega se encuentra en los siguientes enlaces:

- [Documento de diseño de la evaluación, métricas y eventos](https://docs.google.com/document/d/11No24L4QlzMVcvrEAbuMexVIQcMBNANlsSzRxQic0t0/edit?usp=sharing)

- [Documento con los resultados de las métricas y las conclusiones alcanzadas](https://docs.google.com/document/d/1_dxsybOs0hxeWAp5r9HXvBQ8oM2EbFuGL3kE6exFckc/edit?usp=sharing)

- [Sistema de telemetría](https://github.com/CristinaMora/NeonRisingUAJ/tree/Pruebas-uaj/Nombre%20del%20juego/Assets/scripts/Telemetry)

- [Juego instrumentalizado con el sistema de telemetría](https://github.com/CristinaMora/NeonRisingUAJ/releases/tag/telemetry)

- [Archivos de datos generados por el sistema de telemetría](https://github.com/CristinaMora/NeonRisingUAJ/tree/Pruebas-uaj/Analitycs/data)

- [Scripts de Python para realizar el cálculo de las métricas a partir del archivo de trazas](https://github.com/CristinaMora/NeonRisingUAJ/tree/Pruebas-uaj/Analytics), para probar con los archivos de datos generados de forma automatizada (importante tener previamente instalado `Python 3.11`):

    ```ps
    .\analyze.bat
    ```

## Partes opcionales

Se han realizado los siguientes opcionales:

- Serialización y persistencia en una hebra independiente de la del videojuego.
- Añadir al menos otro medio de serialización (CSV, binario. . . ), configurable desde
código. -> Se ha creado la opción con CSV para persistencia local
- Añadir el envío de trazas a una base de datos de Firebase o similar. -> Se envían trazas a una base de datos de Firebase y como adición, se envían trazas a un documento de [Google Sheets de Google Drive](https://docs.google.com/spreadsheets/d/1yjRxqTQEtxAjRrnXdojslADJ7RCknznUzoeeCnI2PNs/edit?usp=drive_link). Ver vídeos: [opcionalFirebase](https://drive.google.com/file/d/157Z7oh7D1C0RtCSQZmS8Q1keOlqbYG5y/view?usp=drive_link) y [opcionalGoogleSheets](https://drive.google.com/file/d/1iEjKZuc68s8QLaJ0vvMkENpRZ7maLrDM/view?usp=drive_link)
- Configuración del sistema de telemetría por datos (fichero de configuración, configuración desde el editor de Unity. . . ) -> Se genera un archivo de configuración `config.json` desde el editor de Unity

## Dónde se ha añadido código de instrumentalización

- [Tracker.cs, Persistence.cs, CircularQueue.cs, CSVSerializer.cs, todos los eventos usados,e tc.](https://github.com/CristinaMora/NeonRisingUAJ/tree/Pruebas-uaj/Nombre%20del%20juego/Assets/scripts/Telemetry)
- [GameManager.cs](https://github.com/CristinaMora/NeonRisingUAJ/blob/Pruebas-uaj/Nombre%20del%20juego/Assets/scripts/_..manager/GameManager.cs)
- [Arrow.cs](https://github.com/CristinaMora/NeonRisingUAJ/blob/Pruebas-uaj/Nombre%20del%20juego/Assets/scripts/ArrowBow/Arrow.cs)
- [Arrow2.cs](https://github.com/CristinaMora/NeonRisingUAJ/blob/Pruebas-uaj/Nombre%20del%20juego/Assets/scripts/ArrowBow/Arrow2.cs)
- [CameraCollisionDetection.cs](https://github.com/CristinaMora/NeonRisingUAJ/blob/Pruebas-uaj/Nombre%20del%20juego/Assets/scripts/C%C3%A1mara/CameraCollisionDetection.cs)
- [Spike_Platform_Component.cs](https://github.com/CristinaMora/NeonRisingUAJ/blob/Pruebas-uaj/Nombre%20del%20juego/Assets/scripts/Platform/Spike_Platform_Component.cs)
- [Life_System_Component.cs](https://github.com/CristinaMora/NeonRisingUAJ/blob/Pruebas-uaj/Nombre%20del%20juego/Assets/scripts/PLAYER/Life_System_Component.cs)

## Ejemplo de build y análisis

Se ha grabado un [vídeo](https://drive.google.com/file/d/1Cbg4Ni1uTpjVBAVr3-ahbI5sPdFQdvCa/view?usp=drive_link) que incluye desde el proceso de elegir la configuración desde el editor de Unity y buildear el juego hasta la ejecución del script de análisis.

## Otros

Para probar con el proyecto de Unity para enviar trazas a Firebase y Google Sheets es necesario añadir los archivos encontrados en el siguiente [link](https://drive.google.com/drive/folders/1x7bUWiPZkQxv0QZBUDw-MuDWm_wcBSGX?usp=sharing) en la carpeta `...NeonRisingUAJ\Nombre del juego\Assets\StreamingAssets` pues incluye claves secretas para acceder a los servidores.

Además, hay que descargar los unitypackages para la implementación de Firebase con Unity en:
https://firebase.google.com/download/unity

Importar los siguientes unitypackages en el proyecto:

- FirebaseAnalytics.unitypackage
- FirebaseAppCheck.unitypackage
- FirebaseDatabase.unitypackage

Si al abrir el proyecto de Unity sale un error del siguiente estilo:

```txt
Assembly 'Assets/Firebase/Editor/Firebase.Editor.dll' will not be loaded due to errors:
Unable to resolve reference 'UnityEditor.iOS.Extensions.Xcode'. Is the assembly missing or incompatible with the current platform?
Reference validation can be disabled in the Plugin Inspector.
```

con instalar el módulo de iOS en el proyecto se corrige: `File > Build Settings > iOS > Install with Unity Hub`. Más información sobre este error en: [link](https://github.com/firebase/firebase-unity-sdk/issues/218).

En el siguiente [Google Sheets](https://docs.google.com/spreadsheets/d/1yjRxqTQEtxAjRrnXdojslADJ7RCknznUzoeeCnI2PNs/edit?usp=drive_link) se puede observar los datos obtenidos si se envían trazas con la versión de persistencia de WEBSERVER.

La base de datos de Firebase es privada pero podemos darle acceso sin problema.
