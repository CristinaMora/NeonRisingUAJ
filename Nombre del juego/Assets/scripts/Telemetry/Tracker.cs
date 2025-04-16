using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// Al diseñarlo hay que tener en cuenta qué vamos a hacer cuando se producen ciertos
// eventos catastróficos:
// • Nos hemos quedado sin espacio.
// • No tenemos red. -> guardar en local hasta que haya red
// • No tenemos permisos para almacenar los eventos.
// • Consumo de batería y datos (móviles).
// • El usuario reinstala la aplicación (dejamos de tener info del usuario concreto, si
// fuese necesario).
// • El usuario modifica los datos enviados (poco probable en métricas).
// • Privacidad en distintas regiones/países.

public class Tracker
{
    public enum Format { JSON, CSV/*, ... Otros formatos */ }; // Formatos disponibles para guardar los eventos

    // Opcional: Añadir el envío de trazas a una base de datos de Firebase o similar.
    public enum PersistenceType { LOCAL, DATABASE };

    private int EVENTS_TO_WRITE_SIZE;           // Numero limite de eventos para escribir
    private const string SALT = "UAJ-Grupo1";   // Salt que se usa para generar IDs unicas
    private static string sessionId;            // ID de la sesion

    // TODO: Crear eventos genericos
    // TODO: Manejar eventos puntuales donde se guardan en escenas especificas

    private string localPath;   // Ruta en donde se guarda el archivo con los datos
                                // de telemetría (local)
    Format format;              // Formato de escritura de los eventos
    PersistenceType persType;   // Tipo de persistencia de los eventos

    // TODO: Crear la cola de eventos, importante investigar sobre la concurrencia
    // mientras se está leyendo y eliminando de la cola, también se están añadiendo
    // eventos...
    // ConcurrentQueue es thread-safe lo que indica que si creamos un hilo aparte
    // gestiona las concurrencias que pueda haber
    // https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1?view=net-9.0
    // Opcional: Serialización y persistencia en una hebra independiente de la del videojuego.
    private ConcurrentQueue<Event> eventQueue;
    private Thread eventThread; // Hilo con bucle que gestiona la cola de eventos

    // Opcional: Añadir el envío de trazas a un servidor web.
    //private string DEFAULT_SERVER_DOMAIN = "https://example.com"; // Dominio del servidor web
    // Opcional: Manejar eventos muestreables (solamente escribir cada cierto tiempo)
    // Opcional: Posibilidad de poder desactivar el seguimiento de determinados tipos de eventos.
    // HashSet<Event> disabledEvents = new HashSet<Event>();

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

        eventQueue = new ConcurrentQueue<Event>();
        sessionId = Guid.NewGuid().ToString();
        format = ConfigManager.GetFormat();
        persType = ConfigManager.GetPersistenceType();
        EVENTS_TO_WRITE_SIZE = ConfigManager.GetEventsToWriteSize();

        switch (persType)
        {
            case PersistenceType.LOCAL:
                CreateLocalLogFile();
                break;
            case PersistenceType.DATABASE:
                InitiateDatabaseConnection();
                break;
            default: break;
        }

        // TODO: Bucle de lectura-escritura del archivo de guardado
        // que dependiendo del PersistenceType se escribirá/enviará
        // en local o por servidor
        InitiateLoop();
    }

    #region Persistencia Local
    /// <summary>
    /// Crea y abre el archivo donde volcar los datos
    /// </summary>
    private void CreateLocalLogFile()
    {
        localPath = Application.dataPath + "/" + ConfigManager.GetLogFilename();
        Debug.Log("Ruta del archivo de telemetria: " + localPath);
    }
    #endregion

    #region Persistencia por servidor con base de datos
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
    /// Envia el evento a Firebase
    /// </summary>
    /// <param name="e"></param>
    private void SendEventToFirebase(Event e)
    {
        // Firebase solo permite envio de datos con formato JSON
        Debug.Log("Sending event to Firebase: " + e.ToJSON());

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("events")
            .Push()
            .SetRawJsonValueAsync(e.ToJSON())
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
                    Debug.Log("Event correctly sent");
            });
    }
    #endregion

    #region Gestion de la cola de eventos
    /// <summary>
    /// Inicia el hilo de lectura-escritura con bucle usando el ConcurrentQueue
    /// </summary>
    private void InitiateLoop()
    {
        // TODO: Crear un hilo que contenga un bucle

    }

    /// <summary>
    /// Mete un elemento en la cola de eventos y si supera un maximo,
    /// hace flush dependiendo del tipo de persistencia
    /// </summary>
    /// <param name="e">Evento a meter a la cola.</param> 
    public void SendEvent(Event e)
    {
        eventQueue.Enqueue(e);

        // Si supera un maximo escribe
        if (eventQueue.Count >= EVENTS_TO_WRITE_SIZE)
        {
            switch (persType)
            {
                case PersistenceType.LOCAL:
                    FlushQueueToFile();
                    break;
                case PersistenceType.DATABASE:
                    FlushQueueToFirebase();
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Saca de la cola cuando se superen cierto elementos y escribe en el archivo en el formato
    /// (+ si se mete por tiempo)
    /// </summary>
    public void FlushQueueToFile()
    {
        int i = 0;
        while (eventQueue.TryDequeue(out Event e) && i < EVENTS_TO_WRITE_SIZE)
        {
            if (e != null)
            {
                string data;
                switch (format)
                {
                    case Format.JSON:
                        data = e.ToJSON();
                        break;
                    case Format.CSV:
                        data = e.ToCSV();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
                File.AppendAllText(localPath, data + "\n");
            }
            i++;
        }
    }

    /// <summary>
    /// Saca de la cola de eventos y los envia a la base de datos de Firebase
    /// </summary>
    private void FlushQueueToFirebase()
    {
        int i = 0;
        while (eventQueue.TryDequeue(out Event e) && i < EVENTS_TO_WRITE_SIZE)
        {
            if (e != null)
                SendEventToFirebase(e);
            i++;
        }
    }
    #endregion

    #region Utilidades
    /// <summary>
    /// Genera un ID unico a partir del evento, utiliza el tiempo actual y una salt
    /// </summary>
    /// <param name="e">Evento a partir del cual generar el ID</param>
    /// <returns></returns>
    private static string GenerateUniqueID(Event e)
    {
        using (var sha256 = SHA256.Create())
        {
            var epoch = new DateTime(1970, 1, 1);
            var millisecondsSinceEpoch = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
            var rawData = $"{e.GetSessionId()}-{SALT}-{millisecondsSinceEpoch}";
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
    #endregion

    public string GetSessionId()
    {
        return sessionId;
    }
}
