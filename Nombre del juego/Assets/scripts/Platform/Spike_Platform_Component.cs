using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike_Platform_Component : MonoBehaviour
{
    #region Parametres
    private Rigidbody2D _myRigidbody;
    [SerializeField]
    private float _timer = 3.0f;
    private float _currentime = 0;
    [SerializeField]
    private float _impulse = 4.0f;
    [SerializeField]
    private int _damage;
    private Transform _mytransform;

    #endregion
    private void OnCollisionEnter2D(Collision2D collision)
    {
        

        if (collision.gameObject.GetComponent<Player_Life_Component>())
        {

            PlayerDiesEvent playerDiesEvent = new PlayerDiesEvent(Tracker.Instance.SessionId, transform.position,
                DeathCause.Spikes);
            Tracker.Instance.SendEvent(playerDiesEvent);

            //Llamada al Game Manager correspondiente
            //collision.gameObject.GetComponent<Player_Life_Component>().gameObject.SetActive(false);// por ahora se queda asi
            GameManager.Instance.PlayerDies();
            //se llama al gameover 
        }


    }
    // Start is called before the first frame update
    void Start()
    {
        _mytransform = transform;
        _myRigidbody = GetComponent<Rigidbody2D>(); 
    }
    void Update()
    {
        _myRigidbody.velocity = _mytransform.up.normalized * -1;
        _currentime += Time.deltaTime;
        //if (_currentime >= 0)
        //{
        //    _currentime -= Time.deltaTime;
        //}
        //else
        //{
        //    _currentime = _timer;
        //    Jump();
        //}
        if(_currentime >= _timer)
        {
            Jump();
            _currentime = 0;
        }
    }
    void Jump()
    {
        //Debug.Log(_myRigidbody.velocity = _mytransform.forward.normalized * _impulse);
        _myRigidbody.velocity = _mytransform.up.normalized*_impulse;

    }
}
