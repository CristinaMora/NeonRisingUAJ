using System;

/// <summary>
/// Clase base abstracta que representa un evento dentro de nuestro sistema de telemetria.
/// </summary>
public abstract class Event
{
    private long sessionId;     // ID de la sesion en la que se genera el evento
    private int id;             // ID unico del evento
    private DateTime timestamp; // Momento en el que se ha generado el evento 

    /// <summary>
    /// Constructor base que inicializa los datos comunes del evento
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="id"></param>
    protected Event(long sessionId, int id)
    {
        this.sessionId = sessionId;
        this.id = id;
        this.timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Obtiene el ID de la sesion
    /// </summary>
    /// <returns></returns>
    public long GetSessionId()
    {
        return sessionId;
    }

    /// <summary>
    /// Establece el ID de la sesion
    /// </summary>
    /// <param name="value"></param>
    public void SetSessionId(long value)
    {
        sessionId = value;
    }

    /// <summary>
    /// Obtiene el ID del evento
    /// </summary>
    /// <returns></returns>
    public int GetId()
    {
        return id;
    }

    /// <summary>
    /// Establece el ID del evento
    /// </summary>
    /// <param name="value"></param>
    public void SetId(int value)
    {
        id = value;
    }

    /// <summary>
    /// Obtiene la marca de tiempo del evento
    /// </summary>
    /// <returns></returns>
    public DateTime GetTimestamp()
    {
        return timestamp;
    }

    /// <summary>
    /// Establece la marca de tiempo del evento
    /// </summary>
    /// <param name="value"></param>
    public void SetTimestamp(DateTime value)
    {
        timestamp = value;
    }
    
    /// <summary>
    /// Metodo abstracto que debe ser implementado por cada tipo de event para definir como se guardan sus datos
    /// </summary>
    public abstract void WriteData();

    /// <summary>
    /// Metodo abstracto que debe ser implementado por cada tipo de event para definir como se escribe en formato CSV
    /// </summary>
    /// <returns>Texto en formato CSV</returns>
    public abstract string ToCSV();
}
