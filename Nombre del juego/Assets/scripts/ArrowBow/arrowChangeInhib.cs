using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrowChangeInhib : MonoBehaviour
{
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            GameManager.Instance._player.GetComponent<PlayerInput_Component>().canArrowChange = false;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        GameManager.Instance._player.GetComponent<PlayerInput_Component>().canArrowChange = true;
    }
}
