using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bowInhib : MonoBehaviour
{
    [SerializeField]
    private GameObject _bow;
   
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            _bow.SetActive(false);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            _bow.SetActive(true);
        }
    }
}
