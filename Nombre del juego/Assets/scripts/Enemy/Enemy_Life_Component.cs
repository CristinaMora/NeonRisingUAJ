using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_Life_Component : Life_System_Component
{
    #region parametres
    [SerializeField]
    private int _damageontouch;

    #endregion
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player_Life_Component>() && collision.gameObject.activeSelf)
        {
            GameManager.Instance.OnPlyerDamage(_damageontouch);
        }
    }
  
}
