using System;
using System.IO;
using System.Text;
using UnityEngine;
using static Tracker;
public class FilePersistence : Persistence
{
    private string localPath;   // Ruta en donde se guarda el archivo con los datos

    public FilePersistence(Format _format, string _localPath) : base(_format)
    {
        localPath = _localPath;

        CreateLocalLogFile();
    }

    /// <summary>
    /// Crea y abre el archivo donde volcar los datos
    /// </summary>
    private void CreateLocalLogFile()
    {
        try
        {
			localPath = Application.dataPath + "/" + ConfigManager.GetLogFilename();
			// Para cada formato añadimos la extensión correspondiente.
			switch (format)
			{
				case Format.CSV:
					localPath += ".csv";
					break;
				case Format.JSON:
					localPath += ".json";

					// Si no existe el archivo, lo creamos y escribimos el inicio del JSON
					if (!File.Exists(localPath))
					{
						// No existe: lo creamos y escribimos [
						File.WriteAllText(localPath, "[\n");
					}
					else
					{
						string content = File.ReadAllText(localPath).TrimEnd();

						// Existe pero está vacío
						if (string.IsNullOrWhiteSpace(content))
						{
							File.WriteAllText(localPath, "[\n");
						}
						else
						{

							// Quitamos el cierre, la coma se escribirá luego, pero vamos a introducir un salto de línea para diferenciar entre sesiones.
							int lastBracketIndex = content.LastIndexOf(']');
							if (lastBracketIndex != -1)
							{
								content = content.Substring(0, lastBracketIndex).TrimEnd();
								File.WriteAllText(localPath, content + "\n");
							}
						}
					}
					break;
				default: break;
			}
			Debug.Log("Path to telemetry log file: " + localPath);
		}
        catch (Exception ex)
        {
			Debug.LogError($"[FilePersistence] There was an error while opening the file: {ex.Message}");
		}
      
    }

    /// <summary>
    /// Saca de la cola cuando se superen cierto elementos y escribe en el archivo en el formato
    /// (+ si se mete por tiempo)
    /// </summary>
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

					if (format == Format.JSON)
					{
						// En caso de no ser el primero, necesita una coma delante.
						if (!isFirst)
							batch.Append(",\n");
						batch.Append(data);
						isFirst = false;
					}
					else
					{
						batch.AppendLine(data);
					}
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
}
