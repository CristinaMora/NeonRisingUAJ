using System;
using UnityEngine;

[Serializable]
public class ArrowShotEvent : Event
{
    public enum ArrowType { Damage, Teleport }
    public ArrowType arrowType;  // Tipo de flecha
    public Vector3 position;     // Posición desde donde se disparó
    public bool hasHit;          // Si cumplió su propósito

    public ArrowShotEvent(string gameId, ArrowType arrowType, Vector3 pos, bool hasHit)
        : base(gameId, "ArrowShotEvent")
    {
        this.arrowType = arrowType;
        this.position = pos;
        this.hasHit = hasHit;
    }

    public override string ToCSV()
    {
        return base.ToCSV() + $",{arrowType},{position},{hasHit}";
    }
}
