
using static Tracker;
using UnityEngine.Rendering;

public class FilePersistence : Persistence
{
    private string localPath;   // Ruta en donde se guarda el archivo con los datos

    public FilePersistence(PersistenceType _persType, Format _format, string _localPath, string webhookURL = null)
    {
        Persistence(_persType, _format, _localPath);

        localPath = _localPath;
    }
}
