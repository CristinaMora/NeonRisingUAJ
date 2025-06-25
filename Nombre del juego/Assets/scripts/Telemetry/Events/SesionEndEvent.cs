using System;

[Serializable]
public class SessionEndEvent : TrackerEvent
{
    public SessionEndEvent() : base("SessionEnd")
    {
    }
}
