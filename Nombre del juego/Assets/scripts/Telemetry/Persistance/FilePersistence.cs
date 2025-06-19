using System;
using System.IO;
using System.Text;
using UnityEngine;
using static Tracker;
public class FilePersistence : Persistence
{
    private string localPath;   // Ruta en donde se guarda el archivo con los datos
    private ISerializer serializer; // Formato de los eventos

    public FilePersistence(ISerializer _format) : base()
    {
        serializer = _format;

        CreateLocalLogFile();
    }

    /// <summary>
    /// Saca de la cola cuando se superen cierto elementos y escribe en el archivo en el formato
    /// (+ si se mete por tiempo)
    /// </summary>
    /// 
    public override void FlushQueue(CircularQueue<Event> eventQueue)
    {

        try
        {
            int i = 0;
            bool isFirst = IsFirstJsonEntry();

            StringBuilder batch = new StringBuilder();

            while (eventQueue.TryDequeue(out Event e))
            {
                if (e != null)
                {
                    string data;

                    data = serializer.Serialize(e); //Serializamos el evento
                    serializer.AppendSerializedData(batch, data, ref isFirst);  //Escribimos el evento siguiendo el formato

                }
                i++;
            }

            if (batch.Length > 0)
                File.AppendAllText(localPath, batch.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FilePersistence] Error while writing the events in the file: {ex.Message}\n");
        }

    }

    /// <summary>
    /// Cierra los archivos usando el formato necesario
    /// </summary>
    public override void EndPersistance()
    {
        string content = File.ReadAllText(localPath).TrimEnd();
        File.AppendAllText(localPath, serializer.endFile(content));
    }

    private bool IsFirstJsonEntry()
    {
        try
        {
            if (!File.Exists(localPath)) return true;

            string content = File.ReadAllText(localPath).Trim();

            return content == "[" || content == "[\n";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Critical error: Unable to read JSON log file.", ex);
        }
    }


    /// <summary>
    /// Crea y abre el archivo donde volcar los datos
    /// </summary>
    private void CreateLocalLogFile()
    {
        try
        {
            localPath = Application.dataPath + "/" + ConfigManager.GetLogFilename() + serializer.localPathExtension();
            // Para cada formato a�adimos la extensi�n correspondiente.

            // Si no existe el archivo, lo creamos y escribimos el inicio según el formato
            if (!File.Exists(localPath))
            {
                // No existe
                File.WriteAllText(localPath, serializer.initFile());
            }
            // Existe
            else
            {
                string content = File.ReadAllText(localPath).TrimEnd();

                // Existe pero esta vacio
                if (string.IsNullOrWhiteSpace(content))
                {
                    //Escribimos el inicio
                    File.WriteAllText(localPath, serializer.initFile());
                }
                //Existe y tiene texto
                else
                {
                    int lastBracketIndex = 0;
                    if (serializer.changeOfSesion(ref content, lastBracketIndex))
                    {
                        File.WriteAllText(localPath, content + "\n");
                    }
                }
            }

            Debug.Log("Path to telemetry log file: " + localPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FilePersistence] There was an error while opening the file: {ex.Message}");
        }

    }
}
