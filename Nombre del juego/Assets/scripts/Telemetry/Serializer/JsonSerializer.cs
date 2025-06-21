using System.Text;

public class JsonSerializer : ISerializer
{
    public string Serialize(Event e)
    {
        return e.ToJSON();
    }

    public void AppendSerializedData(StringBuilder batch, string data, ref bool isFirst)
    {
        if (!isFirst)
            batch.Append(",\n");

        batch.Append(data);
        isFirst = false;
    }

    public string GetLocalPathExtension() { return ".json"; }

    public string InitFile() { return "[\n"; }

    public bool ChangeOfSession(ref string content, int lastbracket)
    {
        // Quitamos el cierre, la coma se escribira luego, pero vamos a introducir un salto de linea para diferenciar entre sesiones.
        int lastBracketIndex = content.LastIndexOf(']');
        if (lastBracketIndex != -1)
        {
            content = content.Substring(0, lastBracketIndex).TrimEnd();
        }

        return lastBracketIndex != -1;
    }

    public string EndFile(string c)
    {
        // Solo escribir si no existe la llave final
        // Escribe "]"
        if (!c.EndsWith("]")) return "]";
        else return "";
    }
}
