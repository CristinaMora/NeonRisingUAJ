using System;
[Serializable]
public class GameEvent : Event
{
	public string gameId;
	public GameEvent(string gameId,string eventName) : base(eventName)
	{
		this.gameId = gameId;
	}

	public override string ToCSV()
	{
		return base.ToCSV() + $",{gameId}";
	}
}
