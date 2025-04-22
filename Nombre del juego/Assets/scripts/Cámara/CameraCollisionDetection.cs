using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisionDetection : MonoBehaviour
{
    [SerializeField]
    private GameObject _camera;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>() && collision.gameObject.activeSelf)
        {
            PlayerDiesEvent playerDiesEvent = new PlayerDiesEvent(Tracker.Instance.SessionId, transform.position, 
                DeathCause.Camera);
            Tracker.Instance.SendEvent(playerDiesEvent);
            GameManager.Instance.PlayerDies();
            collision.gameObject.SetActive(false);
        }
        if (collision.GetComponent<Arrow>())
        {
            Destroy(collision.gameObject);
        }
    }
}
