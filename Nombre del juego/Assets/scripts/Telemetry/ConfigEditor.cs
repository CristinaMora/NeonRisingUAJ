using UnityEngine;
/// <summary>
/// Clase para editar la configuraci�n de la telemetria desde el inspector de Unity.
/// Para usarla, añade este script a un GameObject en la escena.
/// </summary>
public class ConfigEditor : MonoBehaviour
{
	[Tooltip("Clave de autenticación para el sistema de telemetría.")]
	[SerializeField] private string authKey;

	[Tooltip("Nombre del archivo donde se almacenarán los logs de eventos.")]
	[SerializeField] private string logFilename;

	[Tooltip("Formato en el que se guardarán los datos de telemetría (actualmente JSON o CSV).")]
	[SerializeField] private Tracker.Format format;

	[Tooltip("Tipo de persistencia que se usará para los eventos (LOCAL, DATABASE, WEBSERVER).")]
	[SerializeField] private Tracker.PersistenceType persistenceType;

	[Tooltip("Cantidad de eventos que deben acumularse antes de ser escritos. Debe ser un número entero positivo.")]
	[SerializeField] private string eventsToWriteSizeInput;


	private int EVENTS_TO_WRITE_SIZE;

	private void Awake()
	{
		ConfigManager.SetAuthKey(authKey);
		ConfigManager.SetLogFilename(logFilename);
		ConfigManager.SetFormat(format);
		ConfigManager.SetPersistenceType(persistenceType);

		
		if (int.TryParse(eventsToWriteSizeInput, out int parsedValue) && parsedValue > 0)
		{
			EVENTS_TO_WRITE_SIZE = parsedValue;
			ConfigManager.SetEventsToWriteSize(EVENTS_TO_WRITE_SIZE);
		}
		else
		{
			Debug.LogWarning("El valor de 'eventsToWriteSizeInput' no es válido para EVENTS_TO_WRITE_SIZE.");
		}

		new Tracker();
	}
	
}