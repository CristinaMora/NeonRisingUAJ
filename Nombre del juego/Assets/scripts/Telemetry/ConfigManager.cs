using System.IO;
using UnityEngine;

/// <summary>
/// Clase para gestionar la configuración del sistema de telemetría.  
/// Opcional: Configuración del sistema de telemetría por datos (fichero de configuración,
/// configuración desde el editor de Unity...)
/// </summary>
public static class ConfigManager
{
    private static string configPath = "config.json";
    private static ConfigData config;
    private static string route;

    public static string GetWebhookURL()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "webhook-url.txt");

        if (File.Exists(path))
            return File.ReadAllText(path).Trim();
        else
        {
            Debug.LogError("Couldn't find webhook-url.txt");
            return "";
        }
    }

    public static string GetAuthKey()
    {
        return config?.authKey;
    }

    public static string GetLogFilename()
    {
        return config?.logFilename;
    }

    public static Tracker.Format GetFormat()
    {
        return config != null && System.Enum.TryParse(config.format, out Tracker.Format format)
            ? format
            : Tracker.Format.JSON; // por defecto
    }

    public static Tracker.PersistenceType GetPersistenceType()
    {
        return config != null && System.Enum.TryParse(config.persistenceType, out Tracker.PersistenceType persistenceType)
            ? persistenceType
            : Tracker.PersistenceType.LOCAL; // por defecto
    }

    public static int GetEventsToWriteSize()
    {
        return config != null
            ? config.EVENTS_TO_WRITE_SIZE
            : 50; // por defecto
    }

    public static void SetAuthKey(string newAuthKey)
    {
        EnsureConfigFile();
        config.authKey = newAuthKey;
        SaveConfig();
    }

    public static void SetLogFilename(string newLogFilename)
    {
        EnsureConfigFile();
        config.logFilename = newLogFilename;
        SaveConfig();
    }

    public static void SetFormat(Tracker.Format newFormat)
    {
        EnsureConfigFile();
        config.format = newFormat.ToString();
        SaveConfig();
    }

    public static void SetPersistenceType(Tracker.PersistenceType newPersistenceType)
    {
        EnsureConfigFile();
        config.persistenceType = newPersistenceType.ToString();
        SaveConfig();
    }

    public static void SetEventsToWriteSize(int newEventsToWriteSize)
    {
        EnsureConfigFile();
        config.EVENTS_TO_WRITE_SIZE = newEventsToWriteSize;
        SaveConfig();
    }

    public static void SaveConfig()
    {
        string json = JsonUtility.ToJson(config, true);
        File.WriteAllText(route, json);
        Debug.Log($"Config file saved at {route}");
    }

    public static void EnsureConfigFile()
    {
        if (config != null) return;

        route = Application.dataPath + "/" + configPath;
        if (!File.Exists(route))
        {
            Debug.LogWarning($"Config file not found at {route}. Creating a new one with default values.");
            CreateDefaultConfigFile();
        }

        string json = File.ReadAllText(route);
        config = JsonUtility.FromJson<ConfigData>(json);
    }

    private static void CreateDefaultConfigFile()
    {
        ConfigData defaultConfig = new ConfigData
        {
            authKey = "SECRET_KEY",
            logFilename = "telemetry.json",
            format = "JSON",
            persistenceType = "LOCAL"
        };

        string json = JsonUtility.ToJson(defaultConfig, true);
        File.WriteAllText(route, json);
        Debug.Log($"Default config file created at {route}.");
    }

    [System.Serializable]
    private class ConfigData
    {
        public string authKey;
        public string logFilename;
        public string format;
        public string persistenceType;
        public int EVENTS_TO_WRITE_SIZE;
    }
}
