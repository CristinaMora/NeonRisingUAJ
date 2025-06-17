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

    public void AppendSerializedData(StringBuilder batch, string data, bool isFirst)
    {
        Debug.Log("Append CSV")
        batch.AppendLine(data);
    }
}
