using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Life_Component : Life_System_Component
{
    // Start is called before the first frame update
    #region parametres
    static public Player_Life_Component instance;

    public bool isAlive = true;

    [SerializeField]
    private GameObject _explosion;


    #endregion
    void Start()
    {
        instance = this;
    }
    void Update()
    {
        if (isAlive == false)
        {
            Instantiate(_explosion, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }
    
    public void Damage(int damage)
    {
       _currentlife -= damage;
        if (_currentlife <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        PlayerDiesEvent playerDiesEvent = new PlayerDiesEvent(GameManager.Instance.gameId, transform.position,
                DeathCause.Enemy);
        Tracker.Instance.SendEvent(playerDiesEvent);
        GameManager.Instance.PlayerDies();
    }
}
