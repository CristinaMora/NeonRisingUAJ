using Firebase;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEngine;

public class WebPersistence : Persistence
{
    private string webhookURL;  // URL del webhook para enviar los eventos a un servidor (Google Sheets)
    private bool connection = true; // Flag para cuando no se ha conectado a web
    private ISerializer serializer;

    public WebPersistence(string _webhookURL = null) : base()
    {
        webhookURL = _webhookURL;
        serializer = new JsonSerializer();

        InitiateDatabaseConnection();
    }

    /// <summary>
    /// Envio de trazas por servidor web a Google Sheets + AppScript
    /// </summary>
    public override async void SendEvent(Event e)
    {
        string evt = serializer.Serialize(e);


        Debug.Log("Sending event to Google Sheets: " + evt);

        using HttpClient client = new HttpClient();
        var content = new StringContent(evt, Encoding.UTF8, "application/json");

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
    public override void FlushQueue(List<Event> eventList)
    {
        if (connection)
        {
            Debug.LogError("Critical error: Web not connected");
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
