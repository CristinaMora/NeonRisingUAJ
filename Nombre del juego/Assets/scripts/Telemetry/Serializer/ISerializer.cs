using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public interface ISerializer
{

    string Serialize(Event e);
    public void AppendSerializedData(StringBuilder batch, string data, bool isFirst);
}
