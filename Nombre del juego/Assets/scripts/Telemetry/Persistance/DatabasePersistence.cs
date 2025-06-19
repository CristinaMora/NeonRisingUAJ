using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using static Tracker;

public class DatabasePersistence : Persistence
{
    private string webhookURL;  // URL del webhook para enviar los eventos a un servidor (Google Sheets)
    private bool connection = true; // Flag para cuando no se ha conectado al database
    private ISerializer serializer;

    public DatabasePersistence( string _webhookURL = null) : base()
    {
        webhookURL = _webhookURL;
        serializer = new JsonSerializer();

        InitiateDatabaseConnection();
    }

    /// <summary>
    /// Envia el evento a Firebase
    /// </summary>
    /// <param name="e"></param>
    public override void SendEvent(Event e)
    {
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
                    Debug.Log("Event correctly sent to Firebase");
            });
    }

    /// <summary>
    /// Saca de la cola de eventos y los envia a la base de datos de Firebase
    /// </summary>
    public override void FlushQueue(List<Event> eventList)
    {
        if (connection)
        {
            Debug.LogError("Critical error: Data base not connected");
            return;
        }
        foreach (var e in eventList)
        {
            if (e != null)
                SendEvent(e);
        }
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
                connection = false;
                Debug.LogError("Firebase not available: " + task.Result);
            }
        });
    }

}
