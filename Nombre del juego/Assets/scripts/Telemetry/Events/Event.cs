using System;
using UnityEngine;

/// <summary>
/// Clase base abstracta que representa un evento dentro de nuestro sistema de telemetria.
/// </summary>
[Serializable]
public abstract class Event
{
    public string sessionId;            // ID de la sesion en la que se genera el evento
   
    public string eventType;            // Tipo de evento
    public long timestamp;              // Momento en el que se ha generado el evento
    public string authKey;              // Clave de autenticacion para el envio del evento

    /// <summary>
    /// Constructor base que inicializa los datos comunes del evento
    /// </summary>
    /// <param name="gameId">Identificador de la partida</param>
    /// <param name="eventType">Tipo de evento</param>
    protected Event(string eventType)
    {
        sessionId = Tracker.Instance.SessionId;
     
        this.eventType = eventType;
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        authKey = ConfigManager.GetAuthKey();
    }

    /// <summary>
    /// Obtiene el ID de la sesion
    /// </summary>
    /// <returns></returns>
    public string GetSessionId()
    {
        return sessionId;
    }

    /// <summary>
    /// Obtiene la marca de tiempo del evento
    /// </summary>
    /// <returns></returns>
    public long GetTimestamp()
    {
        return timestamp;
    }

    /// <summary>
    /// Establece la marca de tiempo del evento
    /// </summary>
    /// <param name="value"></param>
    public void SetTimestamp(long value)
    {
        timestamp = value;
    }

    /// <summary>
    /// Devuelve el evento en formato JSON 
    /// </summary>
    /// <returns>Texto en formato JSON</returns>
    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    /// <summary>
    /// Debe ser overrideado en eventos que hereden para definir como se escribe en formato CSV
    /// </summary>
    /// <returns>Texto en formato CSV</returns>
    public virtual string ToCSV()
    {
        return $"{sessionId},{eventType},{timestamp}";
    }

    // Otros formatos...
}
