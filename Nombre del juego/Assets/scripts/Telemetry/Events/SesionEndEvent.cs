using System;

[Serializable]
public class SessionEndEvent : Event
{
    public long endTime;

    public SessionEndEvent(string gameId) : base(gameId, "SessionEnd")
    {
        endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{endTime}";
    }
}
