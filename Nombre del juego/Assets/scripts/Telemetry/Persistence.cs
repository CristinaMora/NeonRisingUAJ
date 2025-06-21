using System.Collections.Generic;

public class Persistence
{
    public Persistence()
    { }

    // --- SEND EVENT ---
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">Evento a aniadir</param>
    public virtual async void SendEvent(Event e) { }

    // --- FLUSH QUEUE ---
    /// <summary>
    /// Vacia la cola de eventos
    /// </summary>
    /// <param name="queue">Cola de eventos</param>
    public virtual void FlushQueue(List<Event> eventList) { }

    // --- END PERSISTANCE ---
    /// <summary>
    /// Cierra archivos y conexiones
    /// </summary>
    public virtual void EndPersistance() { }
}
