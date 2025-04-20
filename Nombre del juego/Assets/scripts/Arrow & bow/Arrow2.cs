using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow2 : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField]
    public int multiplier=2;
    [SerializeField]
    private int Damage;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        AudioManager.Instance.Play("Disparo");
        ArrowShotEvent arrowShotEvent;
        if (collision.gameObject.GetComponent<CameraFollow>()==false)
        {
            Destroy(this.gameObject);//se elimina la bala al chocar con lo que sea
            if (collision.gameObject.GetComponent<Enemy_Life_Component>())//si es con el enemigo se le hace daño
            {
                //Debug.Log("entramos en el trigger");
                GameManager.Instance.EnemyDamage(Damage, collision.gameObject);

                Debug.Log("Shoot acertado");
                arrowShotEvent = new ArrowShotEvent(Tracker.Instance.SessionId, ArrowShotEvent.ArrowType.Damage,
                    transform.position, true);
            }
            else
            {
                Debug.Log("Shoot fallado");
                arrowShotEvent = new ArrowShotEvent(Tracker.Instance.SessionId, ArrowShotEvent.ArrowType.Damage,
                    transform.position, false);
            }
        }
        else
        {
            Debug.Log("Shoot fallado");
            arrowShotEvent = new ArrowShotEvent(Tracker.Instance.SessionId, ArrowShotEvent.ArrowType.Damage,
                transform.position, false);
        }
      
        Tracker.Instance.SendEvent(arrowShotEvent);
    }
    void Update()
    {
        //rotacion de la flecha
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
