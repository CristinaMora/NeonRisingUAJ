using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

public class Tracker
{
    public enum Format { JSON, CSV/*, ... Otros formatos */ }; // Formatos disponibles para guardar los eventos

    public enum PersistenceType { LOCAL, DATABASE, WEBSERVER }; // Envio de trazas a un archivo local,
                                                                // a base de datos (Firebase) o
                                                                // a un servidor web (Google Sheets)

    private int EVENTS_TO_WRITE_SIZE;           // Numero limite de eventos para escribir
    private static string sessionId;            // ID de la sesion
    public string SessionId { get { return sessionId; } } // ID de la sesion (para acceder desde fuera de la clase)

    public string localPath;   // Ruta en donde se guarda el archivo con los datos
                                // de telemetria (local)
    private string webhookURL;  // URL del webhook para enviar los eventos a un servidor (Google Sheets)

    Format format;              // Formato de escritura de los eventos
    PersistenceType persType;   // Tipo de persistencia de los eventos

    private CircularQueue<Event> eventQueue;
    // Opcional: Serializacion y persistencia en una hebra independiente de la del videojuego
    private Thread eventThread;                                     // Hilo con bucle que gestiona la cola de eventos
    private volatile bool runningThread = true;                     // False para dejar de ejecutar el hilo (Al cerrar el juego)
    private volatile bool flushQueue = false;                       // Booleano para que flushee la cola solo cuando queremos
    private AutoResetEvent writeSignal = new AutoResetEvent(false); // Senial mandada cuando queremos que se escriba un evento (por tiempo o flush)

    // Opcional: Manejar eventos muestreables (solamente escribir cada cierto tiempo)
    // Opcional: Posibilidad de poder desactivar el seguimiento de determinados tipos de eventos.
    // HashSet<Event> disabledEvents = new HashSet<Event>();

    private Persistence persistenceObject;
    private ISerializer serializeFormat;

    static private Tracker _instance;   // Acceso privado al singleton
    static public Tracker Instance      // Acceso publico al singleton
    {
        get
        {
            return _instance;
        }
    }

    public Tracker()
    {
        _instance = this;


        sessionId = Guid.NewGuid().ToString();
        format = ConfigManager.GetFormat();
        persType = ConfigManager.GetPersistenceType();
        EVENTS_TO_WRITE_SIZE = ConfigManager.GetEventsToWriteSize();
        webhookURL = ConfigManager.GetWebhookURL();

        eventQueue = new CircularQueue<Event>(ConfigManager.GetEventsToWriteSize());

        //AQUI
        // Creation of the Serializer
        switch (format)
        {
            case Format.JSON:
                serializeFormat = new JsonSerializer();
                break;
            case Format.CSV:
                serializeFormat = new CSVSerializer();
                break;
        }

        // Creacion del persistance object
        switch (persType)
        {

            case PersistenceType.LOCAL:
              
                //AQUI
                persistenceObject = new FilePersistence(serializeFormat);
                break;
            case PersistenceType.DATABASE:
                persistenceObject = new DatabasePersistence(webhookURL);
                break;
            case PersistenceType.WEBSERVER:
                persistenceObject = new WebPersistence(webhookURL);
                break;
        }

        SessionStartEvent sessionStartEvent = new SessionStartEvent();
        TrackEvent(sessionStartEvent);
        InitiateLoop();
    }

    // HACERLA CIRCULAR
    // Gestion de la cola de eventos
    /// <summary>
    /// Inicia el hilo de lectura-escritura con bucle usando el ConcurrentQueue
    /// </summary>
    private void InitiateLoop()
    {
        // Crear un hilo que contenga un bucle
        runningThread = true;
        // Creamos el hilo y definimos el metodo con el bucle
        eventThread = new Thread(eventThreadLoop);
        // Inicia la ejecucion del hilo
        eventThread.Start();
    }

    /// <summary>
    /// Bucle del hilo lectura-escritura
    /// </summary>
    private void eventThreadLoop()
    {
        try
        {
            while (runningThread)
            {
                Debug.Log("Estado del hilo: " + eventThread.ThreadState);
                writeSignal.WaitOne(); // Espera que se le indique que guarde


                persistenceObject.FlushQueue(eventQueue);
            }
            Debug.Log("Final del hilo");
        }
        catch (Exception e)
        {
            Debug.LogError("EXCEPCION en el hilo: " + e.Message);
        }
    }

    /// <summary>
    /// Mete un elemento en la cola de eventos y si supera un maximo,
    /// hace flush dependiendo del tipo de persistencia
    /// </summary>
    /// <param name="e">Evento a meter a la cola.</param> 
    public void TrackEvent(Event e)
    {
        eventQueue.Push(e);

        // Si es en local, si supera un maximo escribe o si es servidor, envia directamente
        if (eventQueue.Count >= EVENTS_TO_WRITE_SIZE)
        {
            // Despertamos el hilo
            writeSignal.Set();
        }
    }

    /// <summary>
    /// Metodo para cerrar los archivos y acabar con el bucle del hilo
    /// </summary>
    public void DestroyTracker()
    {
        SessionEndEvent sessionEndEvent = new SessionEndEvent();
        TrackEvent(sessionEndEvent);

        // Para el bucle del hilo
        runningThread = false;

        // Inicia la ultima iteracion del bucle del hilo
        writeSignal.Set();

        // Si el hilo sigue activo
        if (eventThread != null && eventThread.IsAlive)  // Espera a que el hilo termine para continuar
            eventThread.Join();
        
        // Cierra archivos y conexiones
        persistenceObject.EndPersistance();
    }
}
