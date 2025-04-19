using System;
using UnityEngine;
public enum DeathCause
{
    Spikes,
    Enemy,
    Camera
}

[Serializable]
public class PlayerDiesEvent : Event
{
    public long dieTime;
    public Vector3 position;
    public DeathCause cause;

    public PlayerDiesEvent(string gameId, Vector3 position, DeathCause cause)
        : base(gameId, "PlayerDies")
    {
        this.dieTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        this.position = position;
        this.cause = cause;
    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{dieTime},{position.x}:{position.y}:{position.z},{cause}";
    }
}
