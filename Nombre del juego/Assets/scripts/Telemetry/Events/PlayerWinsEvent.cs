using System;

[Serializable]
public class PlayerWinsEvent : GameEvent
{
	public PlayerWinsEvent(string gameId) : base(gameId, "PlayerWins")
	{
	}
}
