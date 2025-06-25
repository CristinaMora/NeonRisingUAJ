using Firebase;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEngine;

public class WebPersistence : IPersistence
{
    private string webhookURL;  // URL del webhook para enviar los eventos a un servidor (Google Sheets)
    private ISerializer serializer;

    public WebPersistence(string _webhookURL = null) : base()
    {
        webhookURL = _webhookURL;
        serializer = new JsonSerializer();
    }

    /// <summary>
    /// Envio de trazas por servidor web a Google Sheets + AppScript.
    /// </summary>
    public async void Send(TrackerEvent e)
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
    /// Saca de la cola de eventos y los envia al servidor web.
    /// </summary>
    public void Flush(List<TrackerEvent> eventList)
    {
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
        // No hace falta
    }

    public void EndPersistence()
    {
        // No hace falta
    }
}
