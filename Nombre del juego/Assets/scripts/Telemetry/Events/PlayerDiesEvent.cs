using System;
using UnityEngine;

public enum DeathCause
{
    Spikes,
    Enemy,
    Camera
}

[Serializable]
public class PlayerDiesEvent : GameEvent
{
    public Vector3 position;
    public DeathCause cause;
    public string name;

    public PlayerDiesEvent(string gameId, Vector3 position, DeathCause cause, string name)
        : base(gameId, "PlayerDies")
    {
        this.position = position;
        this.cause = cause;
        this.name = name;
    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{position.x}:{position.y}:{position.z},{cause},{name}";
    }
}
