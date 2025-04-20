using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using UnityEngine;

// Al disenarlo hay que tener en cuenta que vamos a hacer cuando se producen ciertos
// eventos catastroficos:
// � Nos hemos quedado sin espacio.
// � No tenemos red. -> guardar en local hasta que haya red
// � No tenemos permisos para almacenar los eventos.
// � Consumo de bater�a y datos (moviles).
// � El usuario reinstala la aplicacion (dejamos de tener info del usuario concreto, si
// fuese necesario).
// � El usuario modifica los datos enviados (poco probable en metricas).
// � Privacidad en distintas regiones/paises.

// TODO: Crear eventos genericos
// TODO: Manejar eventos puntuales donde se guardan en escenas especificas

public class Tracker
{
    public enum Format { JSON, CSV/*, ... Otros formatos */ }; // Formatos disponibles para guardar los eventos

    public enum PersistenceType { LOCAL, DATABASE, WEBSERVER }; // Envio de trazas a un archivo local,
                                                                // a base de datos (Firebase) o
                                                                // a un servidor web (Google Sheets)

    private int EVENTS_TO_WRITE_SIZE;           // Numero limite de eventos para escribir
    private static string sessionId;            // ID de la sesion
    public string SessionId { get { return sessionId; } } // ID de la sesion (para acceder desde fuera de la clase)

    private string localPath;   // Ruta en donde se guarda el archivo con los datos
                                // de telemetria (local)
    private string webhookURL;  // URL del webhook para enviar los eventos a un servidor (Google Sheets)

    Format format;              // Formato de escritura de los eventos
    PersistenceType persType;   // Tipo de persistencia de los eventos

    // ConcurrentQueue es thread-safe lo que indica que si creamos un hilo aparte
    // gestiona las concurrencias que pueda haber
    // https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1?view=net-9.0
    private ConcurrentQueue<Event> eventQueue;
    // Opcional: Serializacion y persistencia en una hebra independiente de la del videojuego.
    private Thread eventThread;         // Hilo con bucle que gestiona la cola de eventos
    private bool runningThread = true;  // False para dejar de ejecutar el hilo (Al cerrar el juego)
    private bool flushQueue = false;    // Booleano para que flushee la cola solo cuando queremos
    private AutoResetEvent writeSignal = new AutoResetEvent(false); // Senial mandada cuando queremos que se escriba un evento (por tiempo o flush)
    
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
        webhookURL = ConfigManager.GetWebhookURL();

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

        InitiateLoop();

    }


    #region Persistencia Local
    /// <summary>
    /// Crea y abre el archivo donde volcar los datos
    /// </summary>
    private void CreateLocalLogFile()
    {
        localPath = Application.dataPath + "/" + ConfigManager.GetLogFilename();
        Debug.Log("Path to telemetry log file: " + localPath);
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
        string json = e.ToJSON();

        Debug.Log("Sending event to Firebase: " + json);

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("events")
            .Push()
            .SetRawJsonValueAsync(json)
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
                    Debug.Log("Event correctly sent to Firebase");
            });
    }
    #endregion

    #region Persistencia por servidor web
    /// <summary>
    /// Envtio de trazas por servidor web
    /// </summary>
    private async void SendEventToWebServer(Event e)
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
        catch (System.Exception ex)
        {
            Debug.LogError("Exception sending event to Google Sheets: " + ex.Message);
        }
    }
    #endregion

    #region Gestion de la cola de eventos
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

                switch (persType)
                {
                    case PersistenceType.LOCAL:
                        FlushQueueToFile();
                        break;
                    case PersistenceType.DATABASE:
                        FlushQueueToFirebase();
                        break;
                    case PersistenceType.WEBSERVER:
                        FlushQueueToWebServer();
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("EXCEPCION en el hilo: " + e.Message);
        }
    }
    /// <summary>
    /// Metodo que activa el mecanismo para volcar la cola entera en el JSON
    /// </summary>
    public void FlushQueue()
    {
        // Booleano para indicar que queremos volcar la cola
        flushQueue = true;
        // Despertamos el hilo
        writeSignal.Set();
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
            // Despertamos el hilo
            writeSignal.Set();
        }
    }

    /// <summary>
    /// Saca de la cola cuando se superen cierto elementos y escribe en el archivo en el formato
    /// (+ si se mete por tiempo)
    /// </summary>
    public void FlushQueueToFile()
    {
        int i = 0;

        while (eventQueue.TryDequeue(out Event e) && (i < EVENTS_TO_WRITE_SIZE || flushQueue))
        {
            Debug.Log(i);
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

        // En caso de que este vaciando la cola entera, resetea la variable de control
        if (flushQueue) flushQueue = false;
    }

    /// <summary>
    /// Saca de la cola de eventos y los envia a la base de datos de Firebase
    /// </summary>
    private void FlushQueueToFirebase()
    {
        int i = 0;
        while (eventQueue.TryDequeue(out Event e) && (i < EVENTS_TO_WRITE_SIZE || flushQueue))
        {
            if (e != null)
                SendEventToFirebase(e);
            i++;
        }

        // En caso de que este vaciando la cola entera, resetea la variable de control
        if (flushQueue) flushQueue = false;
    }

    /// <summary>
    /// Saca de la cola de eventos y los envia al servidor web
    /// </summary>
    private void FlushQueueToWebServer()
    {
        while (eventQueue.TryDequeue(out Event e))
        {
            SendEventToWebServer(e);
        }
    }
    #endregion


    /// <summary>
    /// Metodo para cerrar los archivos y acabar con el bucle del hilo
    /// </summary>
    public void DestroyTracker()
    {
        // Vuelca lo que queda de la cola en JSON
        FlushQueue();

        // Para el bucle del hilo
        runningThread = false;

        // Inicia la ultima iteracion del bucle del hilo
        writeSignal.Set();


        // Si el hilo sigue activo
        if (eventThread.IsAlive)  // Espera a que el hilo termine para continuar (Para que no haya problemas al cerrar el juego)
            eventThread.Join(); // Este metodo puede provocar la congelacion del hilo principal, pero como lo vamos a usar al cerrar el juego no deberia dar problemas (Consultar con el grupo)
    }
}
