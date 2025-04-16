using UnityEngine;

public class Tests : MonoBehaviour
{
    private string gameId = "gameId"; // Cambiar por el ID la partida

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space was pressed.");
            Vector3 shotPosition = Vector3.zero; // player.transform.position;
            bool hasHit = true;
            ArrowShotEvent.ArrowType arrowType = ArrowShotEvent.ArrowType.Damage;

            ArrowShotEvent arrowEvent = new ArrowShotEvent(gameId, arrowType, shotPosition, hasHit);
            Tracker.Instance.SendEvent(arrowEvent);
        }
    }
}
