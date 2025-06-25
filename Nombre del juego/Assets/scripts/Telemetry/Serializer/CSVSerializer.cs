using System.Text;

/// <summary>
/// Formato CSV.
/// </summary>
public class CSVSerializer : ISerializer
{
    public string Serialize(TrackerEvent e) { return e.ToCSV(); }

    public void AppendSerializedData(StringBuilder batch, string data, ref bool isFirst) { batch.AppendLine(data); }

    public string GetLocalPathExtension() { return ".csv"; }

    public string InitFile() { return ""; }

    public bool ChangeOfSession(ref string content, int lastbracket) { return false; }

    public string EndFile(string c) { return ""; }
}
