using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life_System_Component : MonoBehaviour
{
    #region parametres
    [SerializeField]
    public int _maxlife;
    public  int _currentlife;
    [SerializeField]
    int enemyscore = 10;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        _currentlife = _maxlife;
    }
    public void Damage(int Damage)
    {
        _currentlife -= Damage;
        if (_currentlife <= 0 )
        {
            Die();
        }
    }
    private void Die()
    {
        if (this.gameObject.GetComponent<Player_Life_Component>())
        {
            GameManager.Instance.PlayerDies();
        }
        else if (this.gameObject.GetComponent<BossMovement_Component>())
        {
            this.gameObject.SetActive(false);
            GameManager.Instance.OnPlayerVictory();
        }
        else
        {
            gameObject.SetActive(false);
            AudioManager.Instance.Play("EnemyDie");
            UIManager.Instance.AddScore(enemyscore);
        }

    }
    

}
