using System;

[Serializable]
public class GameEvent : TrackerEvent
{
	public string gameId;

	public GameEvent(string gameId, string eventName) : base(eventName)
	{
		this.gameId = gameId;
	}

	public override string ToCSV()
	{
		return base.ToCSV() + $",{gameId}";
	}
}
