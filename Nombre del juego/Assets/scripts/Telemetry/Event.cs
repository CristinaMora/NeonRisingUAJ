using System;

/// <summary>
/// Clase base abstracta que representa un evento dentro de nuestro sistema de telemetria.
/// </summary>
public abstract class Event
{
    private long sessionId;   // ID de la sesion en la que se genera el evento
    private int id;           // ID unico del evento
    private DateTime timestamp; // Momento en el que se ha generado el evento 

 
    // Constructor base que inicializa los datos comunes del evento
    protected Event(long sessionId, int id)
    {
        this.sessionId = sessionId;
        this.id = id;
        this.timestamp = DateTime.UtcNow;
    }

    // Obtiene el ID de la sesion
    public long GetSessionId()
    {
        return sessionId;
    }

    // Establece el ID de la sesion
    public void SetSessionId(long value)
    {
        sessionId = value;
    }
    // Obtiene el ID del evento
  
    public int GetId()
    {
        return id;
    }

    // Establece el ID del evento
    public void SetId(int value)
    {
        id = value;
    }

    // Obtiene la marca de tiempo del evento
    
    public DateTime GetTimestamp()
    {
        return timestamp;
    }

    
    // Establece la marca de tiempo del evento
    
    public void SetTimestamp(DateTime value)
    {
        timestamp = value;
    }

    
    // Metodo abstracto que debe ser implementado por cada tipo de event para definir como se guardan sus datos 
    
    public abstract void WriteData();
}
