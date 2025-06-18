using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CSVSerializer : ISerializer
{
    public string Serialize(Event e)
    {
        Debug.Log("Serialize CSV");
        return e.ToCSV();
    }

    public void AppendSerializedData(StringBuilder batch, string data, ref bool isFirst)
    {
        Debug.Log("Append CSV");
        batch.AppendLine(data);
    }

    public string localPathExtension() { return ".csv"; }

    public string initFile()
    {
        //Para CSV no tiene porqué hacer nada al principio
        return "";
    }

    public bool changeOfSesion(ref string content, int lastbracket) { return false; }

    public string endFile(string c) { return ""; }
}
