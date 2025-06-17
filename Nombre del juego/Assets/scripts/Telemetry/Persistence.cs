using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using UnityEngine;
using static Tracker;

public class Persistence
{
    Format format;              // Formato de escritura de los eventos
    PersistenceType persType;   // Tipo de persistencia de los eventos


    public Persistence(PersistenceType _persType, Format _format)
    {
        persType = _persType;
        format = _format;

        switch (persType)
        {
            case PersistenceType.LOCAL:
                CreateLocalLogFile();
                break;
            case PersistenceType.DATABASE:
                InitiateDatabaseConnection();
                break;
            default: break;
        }

    }
    // --- SEND EVENT ---
    abstract public void SendEvent(Event e);

    #region Persistencia Local
    /// <summary>
    /// Crea y abre el archivo donde volcar los datos
    /// </summary>
    private void CreateLocalLogFile()
    {
        localPath = Application.dataPath + "/" + ConfigManager.GetLogFilename();
        // Para cada formato añadimos la extensión correspondiente.
        switch (format)
        {
            case Format.CSV:
                localPath += ".csv";
                break;
            case Format.JSON:
                localPath += ".json";

                // Si no existe el archivo, lo creamos y escribimos el inicio del JSON
                if (!File.Exists(localPath))
                {
                    // No existe: lo creamos y escribimos [
                    File.WriteAllText(localPath, "[\n");
                }
                else
                {
                    string content = File.ReadAllText(localPath).TrimEnd();

                    // Existe pero está vacío
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        File.WriteAllText(localPath, "[\n");
                    }
                    else
                    {

                        // Quitamos el cierre, la coma se escribirá luego, pero vamos a introducir un salto de línea para diferenciar entre sesiones.
                        int lastBracketIndex = content.LastIndexOf(']');
                        if (lastBracketIndex != -1)
                        {
                            content = content.Substring(0, lastBracketIndex).TrimEnd();
                            File.WriteAllText(localPath, content + "\n");
                        }
                    }
                }
                break;
            default: break;
        }
        Debug.Log("Path to telemetry log file: " + localPath);
    }
    #endregion 

    #region Persistencia por servidor con base de datos
    /// <summary>
    /// Posibilidad de iniciar una conexion con un servidor para enviar
    /// las trazas de datos y guardarlos en una base de datos
    /// </summary>
    private void InitiateDatabaseConnection()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase available.");
            }
            else
            {
                Debug.LogError("Firebase not available: " + task.Result);
            }
        });
    }

    /// <summary>
    /// Envia el evento a Firebase
    /// </summary>
    /// <param name="e"></param>
    private void SendEventToFirebase(Event e)
    {
        // Firebase solo permite envio de datos con formato JSON
        string json = e.ToJSON();

        Debug.Log("Sending event to Firebase: " + json);

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("events")
            .Push()
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Error sending event to Firebase");

                    if (task.Exception != null)
                    {
                        foreach (var innerException in task.Exception.InnerExceptions)
                        {
                            Debug.LogError("Firebase error: " + innerException.Message);
                        }
                    }
                }
                else if (task.IsCompleted)
                    Debug.Log("Event correctly sent to Firebase");
            });
    }
    #endregion

    #region Persistencia con Google Sheets + AppScript
    /// <summary>
    /// Envio de trazas por servidor web a Google Sheets + AppScript
    /// </summary>
    private async void SendEventToWebServer(Event e)
    {
        string json = e.ToJSON();

        Debug.Log("Sending event to Google Sheets: " + json);

        using HttpClient client = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await client.PostAsync(webhookURL, content);

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("Event correctly sent to Google Sheets");
            }
            else
            {
                Debug.LogError("Error sending event to Google Sheets: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception sending event to Google Sheets: " + ex.Message);
        }
    }
    #endregion

    // --- FLUSH QUEUE ---

    public abstract void FlushQueue(ConcurrentQueue<Event> eventQueue, int nEvents) { }

    /// <summary>
    /// Saca de la cola cuando se superen cierto elementos y escribe en el archivo en el formato
    /// (+ si se mete por tiempo)
    /// </summary>
    public void FlushQueueToFile(ConcurrentQueue<Event> eventQueue)
    {
        int i = 0;
        bool isFirst = IsFirstJsonEntry();

        StringBuilder batch = new StringBuilder();

        while (eventQueue.TryDequeue(out Event e) && (i < EVENTS_TO_WRITE_SIZE || flushQueue))
        {
            if (e != null)
            {
                string data;
                switch (format)
                {
                    case Format.JSON:
                        data = e.ToJSON();
                        break;
                    case Format.CSV:
                        data = e.ToCSV();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }

                if (format == Format.JSON)
                {
                    // En caso de no ser el primero, necesita una coma delante.
                    if (!isFirst)
                        batch.Append(",\n");
                    batch.Append(data);
                    isFirst = false;
                }
                else
                {
                    batch.AppendLine(data);
                }
            }
            i++;
        }

        if (batch.Length > 0)
            File.AppendAllText(localPath, batch.ToString());
    }
    private bool IsFirstJsonEntry()
    {
        if (!File.Exists(localPath)) return true;

        string content = File.ReadAllText(localPath).Trim();

        return content == "[" || content == "[\n";
    }

    /// <summary>
    /// Saca de la cola de eventos y los envia a la base de datos de Firebase
    /// </summary>
    public void FlushQueueToFirebase(ConcurrentQueue<Event> eventQueue)
    {
        while (eventQueue.TryDequeue(out Event e))
        {
            if (e != null)
                SendEventToFirebase(e);
        }

    }

    /// <summary>
    /// Saca de la cola de eventos y los envia al servidor web
    /// </summary>
    public void FlushQueueToWebServer(ConcurrentQueue<Event> eventQueue)
    {
        while (eventQueue.TryDequeue(out Event e))
        {
            if (e != null)
                SendEventToWebServer(e);
        }
    }
}
