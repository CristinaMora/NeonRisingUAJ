using System;

[Serializable]
public class GameEndEvent : GameEvent
{
    public GameEndEvent(string gameId) : base(gameId, "GameEnd")
    {

    }
    
}

