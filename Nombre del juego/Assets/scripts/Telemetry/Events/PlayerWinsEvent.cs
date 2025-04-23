using System;
using UnityEngine;


[Serializable]
public class PlayerWinsEvent : GameEvent
{
	public PlayerWinsEvent(string gameId) : base(gameId, "PlayerWins")
	{
	}
}
