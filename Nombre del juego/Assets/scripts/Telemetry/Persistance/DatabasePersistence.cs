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

public class DatabasePersistence : Persistence
{
    private string webhookURL;  // URL del webhook para enviar los eventos a un servidor (Google Sheets)

    public DatabasePersistence(Format _format, string _webhookURL = null) : base(_format)
    {
        webhookURL = _webhookURL;

        InitiateDatabaseConnection();
    }

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
    public override void SendEvent(Event e)
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


    /// <summary>
    /// Saca de la cola de eventos y los envia a la base de datos de Firebase
    /// </summary>
    public override void FlushQueue(CircularQueue<Event> eventQueue)
    {
        while (eventQueue.TryDequeue(out Event e))
        {
            if (e != null)
                SendEvent(e);
        }

    }
}
