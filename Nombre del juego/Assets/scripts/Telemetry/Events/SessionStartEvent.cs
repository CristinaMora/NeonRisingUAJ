using System;

[Serializable]
public class SessionStartEvent : Event
{
    public long startTime;

    public SessionStartEvent(string gameId) : base(gameId, "SessionStart")
    {
        startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{startTime}";
    }
}
