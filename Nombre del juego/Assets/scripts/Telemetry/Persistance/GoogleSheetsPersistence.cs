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

public class GooglePersistence : Persistence
{
    private string webhookURL;  // URL del webhook para enviar los eventos a un servidor (Google Sheets)

    public GooglePersistence(PersistenceType _persType, Format _format, string _webhookURL = null) : base(_persType, _format)
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
    /// Envio de trazas por servidor web a Google Sheets + AppScript
    /// </summary>
    public override async void SendEvent(Event e)
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

    /// <summary>
    /// Saca de la cola de eventos y los envia al servidor web
    /// </summary>
    public override void FlushQueue(ConcurrentQueue<Event> eventQueue)
    {
        while (eventQueue.TryDequeue(out Event e))
        {
            if (e != null)
                SendEvent(e);
        }
    }
}
