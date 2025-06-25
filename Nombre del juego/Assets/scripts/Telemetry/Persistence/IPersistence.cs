using System.Collections.Generic;

public interface IPersistence
{
    /// <summary>
    /// Envio de evento.
    /// </summary>
    public void Send(TrackerEvent e);

    /// <summary>
    /// Vacia la cola de eventos.
    /// </summary>
    public void Flush(List<TrackerEvent> eventList);

    /// <summary>
    /// Crea archivos o inicia conexiones.
    /// </summary>
    public void InitPersistence();

    /// <summary>
    /// Cierra archivos o conexiones.
    /// </summary>
    public void EndPersistence();
}
