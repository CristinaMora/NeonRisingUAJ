using System.Text;

public class CSVSerializer : ISerializer
{
    public string Serialize(Event e)
    {
        return e.ToCSV();
    }

    public void AppendSerializedData(StringBuilder batch, string data, ref bool isFirst)
    {
        batch.AppendLine(data);
    }

    public string GetLocalPathExtension() { return ".csv"; }

    public string InitFile()
    {
        //Para CSV no tiene porqué hacer nada al principio
        return "";
    }

    public bool ChangeOfSession(ref string content, int lastbracket) { return false; }

    public string EndFile(string c) { return ""; }
}
