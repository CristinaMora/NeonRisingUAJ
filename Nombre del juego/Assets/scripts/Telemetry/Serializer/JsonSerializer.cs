using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class JsonSerializer : ISerializer
{
    public string Serialize(Event e)
    {
        Debug.Log("Serialize Json");
        return e.ToJSON();
    }

    public void AppendSerializedData(StringBuilder batch, string data, bool isFirst)
    {
        Debug.Log("Append Json");
        if (!isFirst)
            batch.Append(",\n");

        batch.Append(data);
        isFirst = false;
    }
}
