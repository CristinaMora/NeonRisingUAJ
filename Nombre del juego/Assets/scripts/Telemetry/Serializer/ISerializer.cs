using System.Text;

public interface ISerializer
{
    string Serialize(Event e);
    void AppendSerializedData(StringBuilder batch, string data, ref bool isFirst);

    string GetLocalPathExtension();

    string InitFile();

    bool ChangeOfSession(ref string content, int lastbracket);

    string EndFile(string content);
}
