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
    //AQUI
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
					//switch (format)
					//{
					//	case Format.JSON:
					//		data = e.ToJSON();
					//		break;
					//	case Format.CSV:
					//		data = e.ToCSV();
					//		break;
					//	default:
					//		throw new ArgumentOutOfRangeException(nameof(format), format, null);
					//}

                    ///Ahora mismo no est� cubierta la excepci�n de formato no encontrado
                    data = serializer.Serialize(e); //Serializamos el evento
                    serializer.AppendSerializedData(batch, data, ref isFirst);  //Escribimos el evento siguiendo el formato
                    

					//if (format == Format.JSON)
					//{
					//	// En caso de no ser el primero, necesita una coma delante.
					//	if (!isFirst)
					//		batch.Append(",\n");
					//	batch.Append(data);
					//	isFirst = false;
					//}
					//else
					//{
					//	batch.AppendLine(data);
					//}
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

    private bool IsFirstJsonEntry()
    {
		try
		{
			if (!File.Exists(localPath)) return true;

			string content = File.ReadAllText(localPath).Trim();

			return content == "[" || content == "[\n";
		}
		catch(Exception ex) 
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
