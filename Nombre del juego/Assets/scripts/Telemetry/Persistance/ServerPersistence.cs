
using static Tracker;

public class ServerPersistence : Persistence
{
    private string webhookURL;  // URL del webhook para enviar los eventos a un servidor (Google Sheets)

    public ServerPersistence(PersistenceType _persType, Format _format, string _localPath, string webhookURL = null)
    {

    }
}
