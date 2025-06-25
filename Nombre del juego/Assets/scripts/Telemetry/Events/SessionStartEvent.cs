using System;

[Serializable]
public class SessionStartEvent : TrackerEvent
{
    public SessionStartEvent() : base("SessionStart")
    {
    }
}
