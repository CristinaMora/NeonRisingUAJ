using System;

[Serializable]
public class GameStartEvent : GameEvent
{

    public GameStartEvent(string gameId) : base(gameId, "GameStart")
    {

    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{timestamp}";
    }
}

