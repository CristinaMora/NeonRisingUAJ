using System;

[Serializable]
public class GameEndEvent : Event
{
    public long endTime;

    public GameEndEvent(string gameId) : base(gameId, "GameEnd")
    {
        endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{endTime}";
    }
}

