using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    Vector2 dir;
    private Transform _playerPosition;                  //posicion del jugador           
    private GameObject newBullet;                       //instancia de la nueva bala

    [SerializeField]
    private int _shootDistance = 10;
    [SerializeField]
    private int _shootTimeWait = 5;
    [SerializeField]
    private GameObject _bullet;                         //modelo de bala
    [SerializeField]
    private GameObject _origin;                         //objeto vacio de donde nace la bala y el raycast
    private bool _firstShoot=false;                     //booleano con el que vemos si hemos si el enemigo ha disparado por primera vez
    private bool _canShoot = true;                      //condici�n necesaria que permite o no el disparo
    private LayerMask _Default;                          // capa del suelo
    private GameObject _player;
    
    private float _atCD=1;                                  //Retardo entre disparos
    private void ShotAble()                         //funci�n que nos dice si se puede disparar o no
    {

        if (Physics2D.Raycast(_origin.transform.position, (_playerPosition.position -_origin.transform.position), (_playerPosition.position - _origin.transform.position).magnitude, _Default))         //raycast lanzado desde un punto del enemigo y 
        {
            _canShoot = false;
        }
        else _canShoot = true;


    }

    private void Shoot( in Vector2 dir, float force)
    {
        if(dir.magnitude <= _shootDistance)
        {
            newBullet = Instantiate(_bullet, _origin.transform.position, Quaternion.identity);
            newBullet.GetComponent<Rigidbody2D>().velocity = force * dir.normalized;
            _firstShoot = true;
            newBullet.GetComponent<Bullet_Controller_Component>().bulletOwner = gameObject;
        }
    }
    void Start()
    {
        _Default = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _player = GameManager.Instance._levelManager._player;
        _playerPosition = _player.transform;
        ShotAble();      
        _atCD = _atCD + Time.deltaTime;
        dir = _playerPosition.position + - _origin.transform.position;              // pongo un offset para que apunte un poco m�s arriba, para predecir el movimiento del personaje (+ _aimingUp * _offset)
        if (_atCD>0 && _firstShoot == false &&_canShoot==true)
        {
            Shoot(dir, 10.0f);
            _firstShoot = true;
        }
        else if (_atCD >= _shootTimeWait && _firstShoot==true && _canShoot==true)
        {          
            Shoot(dir, 10.0f);
            _atCD = 0;
        }       
    }
}
