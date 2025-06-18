using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Tracker;
using UnityEngine.Rendering;


public class JsonSerializer : ISerializer
{
    public string Serialize(Event e)
    {
        Debug.Log("Serialize Json");
        return e.ToJSON();
    }

    public void AppendSerializedData(StringBuilder batch, string data, ref bool isFirst)
    {
        Debug.Log("Append Json");
        if (!isFirst)
            batch.Append(",\n");

        batch.Append(data);
        isFirst = false;
    }

    public string localPathExtension() { return ".json"; }

    public string initFile() { return "[\n"; }

    public bool changeOfSesion(ref string content, int lastbracket)
    {
        // Quitamos el cierre, la coma se escribira luego, pero vamos a introducir un salto de linea para diferenciar entre sesiones.
        int lastBracketIndex = content.LastIndexOf(']');
        if (lastBracketIndex != -1)
        {
            content = content.Substring(0, lastBracketIndex).TrimEnd();
        }

        return lastBracketIndex != -1;
    }

    public string endFile(string c) {
        // Solo escribir si no existe la llave final
        // Escribe "]"
        if (!c.EndsWith("]")) return "]";
        else return "";
    }
}
