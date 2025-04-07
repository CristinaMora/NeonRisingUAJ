using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
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
    public enum Format { JSON, CSV };
    public enum PersistenceType { LOCAL, NETWORK };
    private string DEFAULT_SERVER_DOMAIN = "https://example.com";
    private const int EVENTS_TO_WRITE_SIZE = 50;
    private const string SALT = "UAJ-Grupo1";
    private long SESSION_ID; // ID de la sesion.
    // TODO: Crear struct Event con su sessionId, id, timestamp
    // TODO: Crear eventos genericos
    // TODO: Manejar eventos puntuales donde se guardan en escenas especificas
    // TODO: Manejar eventos muestreables (solamente escribir cada cierto tiempo)

    private FileStream logFile;
    // TODO: Crear la cola de eventos, importante investigar sobre la concurrencia
    // mientras se está leyendo y eliminando de la cola, también se están añadiendo
    // eventos...
    //private ConcurrentQueue<Event> eventQueue;
    Format format;

    //String donde se guarda el archivo con los datos de telemetría
    private string route;

    public Tracker(string logFilename, Format chosenFormat, PersistenceType persType, long sesID)
    {
        SESSION_ID = sesID;

        switch (persType)
        {
            case PersistenceType.LOCAL:
                CreateLocalLogFile(logFilename);
                break;
            case PersistenceType.NETWORK:
                InitiateNetworkConnection(logFilename);
                break;
            default: break;
        }

        // ...
        
        _instance = this;
    }

    /*
     * Genera un id unico a partir del evento
     * Utiliza el tiempo actual, una salt
     */
    private string GenerateUniqueID(/*Event event*/)
    {
        //using (var sha256 = SHA256.Create())
        //{
        //    var epoch = new DateTime(1970, 1, 1);
        //    var millisecondsSinceEpoch = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
        //    var rawData = $"{event.sessionId}-{SALT}-{millisecondsSinceEpoch}";
        //    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        //    return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        //}
        return "";
    }

    /*
     * Crear y abrir el archivo donde volcar
     * los datos
     */
    private void CreateLocalLogFile(string logFilename)
    {
        //Si se va a crear aquí el archivo a lo mejor lo suyo es que sea el start?
        route = Application.dataPath + "/telemetria.txt";
        Debug.Log("Archivo telemetria: " + route);

        //No hace falta cerrar el archivo porque vamos a escribir con 
        //File.AppendAllText(route, "json con los datos" + '\n');
    }

    // Cerrar el FileStream??

    /*
     * Posibilidad de enviar a un servidor los datos
     */
    private void InitiateNetworkConnection(string logFilename)
    {
        // Comprobar que se pueda conectar y enviar datos
        // Si no se puede en esta primera vez, volver a intentarlo
        // cada X tiempo y mientras tanto guardar los datos en local
    }

    // Singleton
    static private Tracker _instance;
    static public Tracker Instance
    {
        get
        {
            return _instance;
        }
    }

    // TODO: SendEvent()
    // TODO: Write()

    /*
     * Se llama desde el resto de scripts:
     * almacena el nuevo evento en la cola
     */
    public void SendEvent(/*Event event*/)
    {
        // Aplicar el sessionId, id, timestamp
    }

    /*
     * Escribe el par de [clave,valor]
     */
    public void Write(string key/*, ... value*/)
    {
        //Para escribir seguir el siguiente esquema:

        //Evento e
        //string json = JsonUtility.ToJson(e)
        //File.AppendAllText(route, json + '\n');

        //Con esto debería escribirse todo en un mismo archivo y cerrarse correctamente
    }

    // TODO: Bucle de lectura-escritura del archivo de guardado
    // que dependiendo del PersistenceType se escribirá/enviará
    // en local o por servidor
}
