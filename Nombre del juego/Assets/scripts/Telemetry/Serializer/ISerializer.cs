using System.Text;

/// <summary>
/// Interfaz para distintos tipos de formato (JSON, CSV, etc.).
/// </summary>
public interface ISerializer
{
    string Serialize(TrackerEvent e);

    void AppendSerializedData(StringBuilder batch, string data, ref bool isFirst);

    string GetLocalPathExtension();

    string InitFile();

    bool ChangeOfSession(ref string content, int lastbracket);

    string EndFile(string content);
}
