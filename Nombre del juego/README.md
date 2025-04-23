En la carpeta Assets > StreamingAssets, añadir los archivos google-services.json, google-services-desktop.json y webhook-url.txt de la carpeta de Drive.

Descargar los unitypackages para la implementacion con Unity en:
https://firebase.google.com/download/unity

Importar los siguientes unitypackages en el proyecto:
FirebaseAnalytics.unitypackage
FirebaseAppCheck.unitypackage
FirebaseDatabase.unitypackage

Si sale un error del siguiente estilo:

```txt
Assembly 'Assets/Firebase/Editor/Firebase.Editor.dll' will not be loaded due to errors:
Unable to resolve reference 'UnityEditor.iOS.Extensions.Xcode'. Is the assembly missing or incompatible with the current platform?
Reference validation can be disabled in the Plugin Inspector.
```

Con instalar el módulo de iOS en el proyecto se corrige. Más información en [link](https://github.com/firebase/firebase-unity-sdk/issues/218).
