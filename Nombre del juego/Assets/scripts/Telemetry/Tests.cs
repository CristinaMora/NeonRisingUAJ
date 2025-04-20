using UnityEngine;

public class Tests : MonoBehaviour
{
    private string gameId = "gameId"; // Cambiar por el ID la partida
    ArrowShotEvent arrowShotEvent;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("Space was pressed.");
            
        }

        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("ArrowShot.");
            //Vector3 shotPosition = Vector3.zero; // player.transform.position;
            //bool hasHit = true;
            //ArrowShotEvent.ArrowType arrowType = ArrowShotEvent.ArrowType.Damage;

            //ArrowShotEvent arrowEvent = new ArrowShotEvent(gameId, arrowType, shotPosition, hasHit);
            //Tracker.Instance.SendEvent(arrowEvent);
        }
    }
    private void OnDestroy()
    {
        // Metodo que vacia la cola del tracker
        Tracker.Instance.DestroyTracker();
    }
}
