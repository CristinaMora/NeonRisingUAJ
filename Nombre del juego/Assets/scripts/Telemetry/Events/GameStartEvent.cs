using System;

[Serializable]
public class GameStartEvent : Event
{
    public long startTime;

    public GameStartEvent(string gameId) : base(gameId, "GameStart")
    {
        //hora actual en milisegundos desde Unix
        startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{startTime}";
    }
}

