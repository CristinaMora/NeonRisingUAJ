using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Clase para editar la configuración de la telemetría desde la interfaz de usuario.
/// Para usarla, añade este script a un GameObject en la escena.
/// **Nota:** desactiva todos los objetos de la escena excepto a sí mismo y a los objetos con cámara
/// hasta que se inicie la sesión.
/// </summary>
public class ConfigEditor : MonoBehaviour
{
    private string authKey;
    private string logFilename;
    private Tracker.Format format;
    private Tracker.PersistenceType persistenceType;
    private int EVENTS_TO_WRITE_SIZE;
    private string eventsToWriteSizeInput;

    private Dictionary<GameObject, bool> initialActiveStates = new Dictionary<GameObject, bool>();

    void Awake()
    {
        authKey = ConfigManager.GetAuthKey();
        logFilename = ConfigManager.GetLogFilename();
        format = ConfigManager.GetFormat();
        persistenceType = ConfigManager.GetPersistenceType();
        EVENTS_TO_WRITE_SIZE = ConfigManager.GetEventsToWriteSize();
        eventsToWriteSizeInput = ConfigManager.GetEventsToWriteSize().ToString();

        DisableEverythingExceptThis();
    }

    void DisableEverythingExceptThis()
    {
        initialActiveStates.Clear();

        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj == this.gameObject || obj.transform.IsChildOf(this.transform))
                continue;

            if (obj.GetComponent<Camera>() != null)
                continue;

            if (obj.scene == this.gameObject.scene)
            {
                initialActiveStates[obj] = obj.activeSelf;
                obj.SetActive(false);
            }
                
        }

        AudioListener.pause = true;
        Time.timeScale = 0f;
    }

    void EnableEverything()
    {
        foreach (var entry in initialActiveStates)
        {
            if (entry.Key != null)
            {
                entry.Key.SetActive(entry.Value);
            }
        }

        AudioListener.pause = false;
        Time.timeScale = 1f;
    }

    void OnGUI()
    {
        float width = 300f;
        float height = 500f;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;

        GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
        GUILayout.BeginVertical();

        GUILayout.Label("Telemetry Config");

        GUILayout.Label("Auth Key:");
        authKey = GUILayout.TextField(authKey);
        if (GUILayout.Button("Save Auth Key"))
        {
            ConfigManager.SetAuthKey(authKey);
            Debug.Log($"AuthKey updated to: {authKey}");
        }

        GUILayout.Label("Log Filename:");
        logFilename = GUILayout.TextField(logFilename);
        if (GUILayout.Button("Save Log Filename"))
        {
            ConfigManager.SetLogFilename(logFilename);
            Debug.Log($"Log filename updated to: {logFilename}");
        }

        GUILayout.Label("Format:");
        format = (Tracker.Format)GUILayout.SelectionGrid((int)format, new string[] { "JSON", "CSV" }, 2);
        if (GUILayout.Button("Save Format"))
        {
            ConfigManager.SetFormat(format);
            Debug.Log($"Format updated to: {format}");
        }

        GUILayout.Label("Persistence Type:");
        persistenceType = (Tracker.PersistenceType)GUILayout.SelectionGrid((int)persistenceType, new string[] { "LOCAL", "DATABASE", "WEBSERVER" }, 3);
        if (GUILayout.Button("Save Persistence Type"))
        {
            ConfigManager.SetPersistenceType(persistenceType);
            Debug.Log($"Persistence Type updated to: {persistenceType}");
        }

        GUILayout.Label("EVENTS_TO_WRITE_SIZE:");
        eventsToWriteSizeInput = GUILayout.TextField(eventsToWriteSizeInput);
        if (GUILayout.Button("Save EVENTS_TO_WRITE_SIZE"))
        {
            if (int.TryParse(eventsToWriteSizeInput, out int parsedValue) && parsedValue > 0)
            {
                EVENTS_TO_WRITE_SIZE = parsedValue;
                ConfigManager.SetEventsToWriteSize(EVENTS_TO_WRITE_SIZE);
                Debug.Log($"EVENTS_TO_WRITE_SIZE updated to: {EVENTS_TO_WRITE_SIZE}");
            }
            else
            {
                Debug.LogError("Invalid input for EVENTS_TO_WRITE_SIZE. Please enter a positive integer.");
            }
        }

        GUILayout.Space(20f);

        if (GUILayout.Button("Start Session"))
        {
            Tracker tracker = new Tracker();

            EnableEverything();

            Destroy(gameObject);
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
