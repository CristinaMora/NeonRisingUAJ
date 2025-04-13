using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static UnityEditor.PlayerSettings;

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
    public enum Format { JSON, CSV }; // Formatos disponibles para guardar los eventos
    public enum PersistenceType { LOCAL, NETWORK };

    private const int EVENTS_TO_WRITE_SIZE = 50;// Numero limite de eventos para escribir
    private const string SALT = "UAJ-Grupo1";   // Salt que se usa para generar IDs unicas
    private long SESSION_ID;                    // ID de la sesion

    // TODO: Crear eventos genericos
    // TODO: Manejar eventos puntuales donde se guardan en escenas especificas

    private string localPath;   // Ruta en donde se guarda el archivo con los datos
                                // de telemetría (local)
    private FileStream logFile; // Stream para los datos en local
    Format format;              // Formato de escritura de los eventos

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
    private string DEFAULT_SERVER_DOMAIN = "https://example.com"; // Dominio del servidor web
    // Opcional: Añadir el envío de trazas a una base de datos de Firebase o similar.
    // Opcional: Manejar eventos muestreables (solamente escribir cada cierto tiempo)
    // Opcional: Posibilidad de poder desactivar el seguimiento de determinados tipos de eventos.
    // HashSet<Event> disabledEvents = new HashSet<Event>();
    // Opcional: Configuración del sistema de telemetría por datos (fichero de configuración,
    // configuración desde el editor de Unity...)
    // public string configFilename;
    // y cargar el archivo de configuracion por datos en la constructora

    public Tracker(string logFilename, Format chosenFormat, PersistenceType persType, long sesID)
    {
        _instance = this;

        localPath = Application.dataPath + logFilename;
        format = chosenFormat;
        SESSION_ID = sesID;

        // TODO: Bucle de lectura-escritura del archivo de guardado
        // que dependiendo del PersistenceType se escribirá/enviará
        // en local o por servidor
        switch (persType)
        {
            case PersistenceType.LOCAL:
                CreateLocalLogFile();
                break;
            case PersistenceType.NETWORK:
                InitiateNetworkConnection(logFilename);
                break;
            default: break;
        }

        InitiateLoop();
    }

    static private Tracker _instance;   // Acceso privado al singleton
    static public Tracker Instance      // Acceso publico al singleton
    {
        get
        {
            return _instance;
        }
    }

    #region Persistencia Local
    /// <summary>
    /// Crea y abre el archivo donde volcar los datos
    /// </summary>
    private void CreateLocalLogFile()
    {
        Debug.Log("Archivo telemetria: " + localPath);

        // Crear FileStream...
    }

    // Cerrar el FileStream??

    /// <summary>
    /// Escribe en logFile para el almacenamiento local
    /// </summary>
    public void Write()
    {
        //Evento e
        //switch (format)
        //case JSON:
        //string json = JsonUtility.ToJson(e)
        //File.AppendAllText(route, json + '\n');
        //break;
        //case CSV:
        //File.AppendAllText(route, e.ToCSV() + '\n');
        //break;
        //...
    }
    #endregion

    #region Persistencia por servidor web
    /// <summary>
    /// Posibilidad de iniciar una conexion con un servidor para enviar
    /// las trazas de datos
    /// </summary>
    /// <param name="logFilename">Nombre del paquete</param>
    private void InitiateNetworkConnection(string logFilename)
    {
        // Comprobar que se pueda conectar y enviar datos
        // Si no se puede en esta primera vez, volver a intentarlo
        // cada X tiempo y mientras tanto guardar los datos en local
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
    /// Escribe toda la cola y la vacia
    /// </summary>
    public void FlushQueue()
    {
        // Mientras se este vaciando la cola y siga habiendo eventos
        // se ejecuta el bucle
        while (eventQueue.TryDequeue(out Event evt))
        {
            // Programacion defensiva por si hay un evento vacio
            if (evt != null)
                // Escribimos el evento
                evt.WriteData();
        }
        // --Creo que puede darse el caso de que un elemento de la cola no se haya eliminado, si esto pasa no se como actuar (Consultar al grupo) --
        // Eliminamos los elementos de la cola
        // eventQueue.clear();
    }

    /// <summary>
    /// Mete un elemento en la cola de eventos.
    /// </summary>
    /// <param name="e">Evento a meter a la cola.</param> 
    public void AddEvent(Event e)
    {
        eventQueue.Enqueue(e);
        // Si supera un maximo escribe
        if (eventQueue.Count >= EVENTS_TO_WRITE_SIZE)
        {
            WriteData();
        }
    }

    /// <summary>
    /// Escribe la cola cuando se superen cierto elementos (+ si se mete por tiempo)
    /// </summary>
    public void WriteData()
    {
        int i = 0;
        while (eventQueue.TryDequeue(out Event e) && i < EVENTS_TO_WRITE_SIZE)
        {
            if (e != null)
                e.WriteData();
            i++;
        }
    }

    /// <summary>
    /// Evento que se llama desde el resto de scripts: almacena el nuevo evento en la cola
    /// </summary>
    /// <param name="e"></param>
    public void SendEvent(Event e)
    {
        // Aplicar el sessionId, id, timestamp
    }
    #endregion

    #region Utilidades
    /// <summary>
    /// Genera un ID unico a partir del evento, utiliza el tiempo actual y una salt
    /// </summary>
    /// <param name="e">Evento a partir del cual generar el ID</param>
    /// <returns></returns>
    private string GenerateUniqueID(Event e)
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
}
