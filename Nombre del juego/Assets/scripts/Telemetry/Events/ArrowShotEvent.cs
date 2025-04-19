using System;
using UnityEngine;

[Serializable]
public class ArrowShotEvent : Event
{
    public enum ArrowType { Damage, Teleport }
    public ArrowType arrowType;  // Tipo de flecha
    public Vector3 position;     // Posicion desde donde se disparo
    public bool hasHit;          // Si cumplio su propósito
    public long shotTime;       //cuando golpeo
    public ArrowShotEvent(string gameId, ArrowType arrowType, Vector3 pos, bool hasHit)
        : base(gameId, "ArrowShotEvent")
    {
        this.shotTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        this.arrowType = arrowType;
        this.position = pos;
        this.hasHit = hasHit;
    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{shotTime},{arrowType},{position.x}:{position.y}:{position.z},{hasHit}";
    }
}

