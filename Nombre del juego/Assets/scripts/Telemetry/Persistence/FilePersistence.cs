using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class FilePersistence : IPersistence
{
    private string localPath;           // Ruta en donde se guarda el archivo con los datos
    private ISerializer serializer;     // Formato de los eventos
    private bool createdFile = false;   // Flag para cuando no se ha creado un archivo
    private bool isFirstEntry = true;   // Si es la primera entrada de evento
    private StringBuilder batch;        

    public FilePersistence(ISerializer _format) : base()
    {
        serializer = _format;
        batch = new StringBuilder();
    }

    public void Send(TrackerEvent e)
    {
        string data = serializer.Serialize(e); // Serializamos el evento
        serializer.AppendSerializedData(batch, data, ref isFirstEntry);  // Escribimos el evento siguiendo el formato
    }

    /// <summary>
    /// Saca de la cola cuando se superen cierto elementos y escribe en el archivo en el formato
    /// (+ si se mete por tiempo)
    /// </summary>
    public void Flush(List<TrackerEvent> eventList)
    {
        if (!createdFile)
        {
            Debug.LogError("Critical error: File not created");
            return;
        }

        try
        {
            foreach (var e in eventList)
            {
                if (e != null)
                {
                    Send(e);
                }
            }

            if (batch.Length > 0)
            {
                File.AppendAllText(localPath, batch.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FilePersistence] Error while writing the events in the file: {ex.Message}\n");
        }
    }

    /// <summary>
    /// Crea el archivo.
    /// </summary>
    public void InitPersistence()
    {
        CreateLocalLogFile();
        isFirstEntry = IsFirstEntry();
    }

    /// <summary>
    /// Cierra el archivo.
    /// </summary>
    public void EndPersistence()
    {
        string content = File.ReadAllText(localPath).TrimEnd();
        File.AppendAllText(localPath, serializer.EndFile(content));
    }

    /// <summary>
    /// Comprueba si es la primera entrada en el archivo
    /// </summary>
    private bool IsFirstEntry()
    {
        try
        {
            // Si no existe es que es la primera entrada
            if (!File.Exists(localPath))
            {
                return true;
            }

            string content = File.ReadAllText(localPath).Trim();

            return content == "[" || content == "[\n";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Critical error: Unable to read log file.", ex);
        }
    }

    /// <summary>
    /// Crea y abre el archivo donde volcar los datos
    /// </summary>
    private void CreateLocalLogFile()
    {
        try
        {
            localPath = Application.dataPath + "/" + ConfigManager.GetLogFilename() + serializer.GetLocalPathExtension();
            // Para cada formato anadimos la extension correspondiente.

            // Si no existe el archivo, lo creamos y escribimos el inicio segun el formato
            if (!File.Exists(localPath))
            {
                // No existe
                File.WriteAllText(localPath, serializer.InitFile());
            }
            // Existe
            else
            {
                string content = File.ReadAllText(localPath).TrimEnd();

                // Existe pero esta vacio
                if (string.IsNullOrWhiteSpace(content))
                {
                    // Escribimos el inicio
                    File.WriteAllText(localPath, serializer.InitFile());
                }
                // Existe y tiene texto
                else
                {
                    int lastBracketIndex = 0;
                    if (serializer.ChangeOfSession(ref content, lastBracketIndex))
                    {
                        File.WriteAllText(localPath, content + "\n");
                    }
                }
            }

            createdFile = true;
            Debug.Log("Path to telemetry log file: " + localPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FilePersistence] There was an error while opening the file: {ex.Message}");
        }
    }
}
