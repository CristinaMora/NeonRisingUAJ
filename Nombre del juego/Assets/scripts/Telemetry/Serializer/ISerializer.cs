using System.Text;
public interface ISerializer
{

    string Serialize(Event e);
    void AppendSerializedData(StringBuilder batch, string data, ref bool isFirst);

    string localPathExtension();

    string initFile();

    bool changeOfSesion(ref string content, int lastbracket);

    string endFile(string content);
}
