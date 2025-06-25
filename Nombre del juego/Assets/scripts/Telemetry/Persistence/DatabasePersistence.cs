using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine;

public class DatabasePersistence : IPersistence
{
    private string webhookURL;          // URL del webhook para enviar los eventos a un servidor (Google Sheets)
    private bool connection = false;    // Flag para cuando se ha conectado al database
    private ISerializer serializer;

    public DatabasePersistence(string _webhookURL = null) : base()
    {
        webhookURL = _webhookURL;
        serializer = new JsonSerializer();
    }

    /// <summary>
    /// Envia el evento a Firebase
    /// </summary>
    /// <param name="e"></param>
    public void Send(TrackerEvent e)
    {
        if (!connection)
        {
            Debug.LogError("No conection with Firebase. Not sent");
            return;
        }

        // Firebase solo permite envio de datos con formato JSON
        string evt = serializer.Serialize(e);

        Debug.Log("Sending event to Firebase: " + evt);

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("events")
            .Push()
            .SetRawJsonValueAsync(evt)
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
                {
                    Debug.Log("Event correctly sent to Firebase");
                }
            });
    }

    /// <summary>
    /// Saca de la cola de eventos y los envia a la base de datos de Firebase
    /// </summary>
    public void Flush(List<TrackerEvent> eventList)
    {
        if (!connection)
        {
            Debug.LogError("Critical error: Data base not connected");
            return;
        }

        foreach (var e in eventList)
        {
            if (e != null)
            {
                Send(e);
            }
        }
    }

    public void InitPersistence()
    {
        InitiateDatabaseConnection();
    }

    public void EndPersistence()
    {
        // No hace falta
    }

    /// <summary>
    /// Posibilidad de iniciar una conexion con un servidor para enviar
    /// las trazas de datos y guardarlos en una base de datos.
    /// </summary>
    private void InitiateDatabaseConnection()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                connection = true;
                Debug.Log("Firebase available.");
            }
            else
            {
                Debug.LogError("Firebase not available: " + task.Result);
            }
        });
    }
}
