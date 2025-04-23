using System;

[Serializable]
public class GameStartEvent : GameEvent
{

    public GameStartEvent(string gameId) : base(gameId, "GameStart")
    {

    }
    
}

