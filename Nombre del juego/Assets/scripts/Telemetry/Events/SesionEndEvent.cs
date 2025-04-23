using System;

[Serializable]
public class SessionEndEvent : Event
{
   

    public SessionEndEvent() : base("SessionEnd")
    {
       
    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{timestamp}";
    }
}
