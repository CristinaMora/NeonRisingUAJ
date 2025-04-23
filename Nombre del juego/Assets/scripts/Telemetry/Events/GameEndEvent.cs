using System;

[Serializable]
public class GameEndEvent : GameEvent
{
  
    public GameEndEvent(string gameId) : base(gameId, "GameEnd")
    {

    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{timestamp}";
    }
}

