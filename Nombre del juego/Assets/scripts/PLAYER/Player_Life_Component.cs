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
}
