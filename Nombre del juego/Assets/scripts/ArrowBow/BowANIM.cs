using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowANIM : MonoBehaviour
{
    static public bool isBow = false;

    [SerializeField]
    private Sprite[] bowTENSION;
    [SerializeField]
    private float bowSpeed;

    [SerializeField]
    private Sprite[] bowIDLE;
    [SerializeField]
    private float idleBOWSpeed;

    private float bowProg = 0;

    private SpriteRenderer bowRENDER;
    private void Awake()
    {
        bowRENDER = GetComponent<SpriteRenderer>();
    }
    
    private void BowTension()
    {
        if (bowProg < 1)
        {
            bowProg += Time.deltaTime * bowSpeed;
            if (i == bowTENSION.Length)
            {
                i = 0;
            }
            bowRENDER.sprite = bowTENSION[i];
        }
        else if (bowProg >= 1)
        {
            i++;
            bowProg = 0;
        }
    }

    private float idleProg = 0;
    private int y = 0;
    private void Idle()
    {
        if (idleProg < 1)
        {
            idleProg += Time.deltaTime * bowSpeed;
            if (y == bowIDLE.Length)
            {
                y = 0;
            }
            bowRENDER.sprite = bowIDLE[y];
        }
        else if (idleProg >= 1)
        {
            y++;
            idleProg = 0;
        }
        
    }

    int i = 0;
    void Update()
    {
        if (BowController.BowInstance.MaxForce)
        {
            bowRENDER.sprite = bowTENSION[bowTENSION.Length - 1];
        }
        else if (isBow)
        {
            BowTension();
        }
        else if (!isBow)
        {
            i = 0;
            bowProg = 0;
            Idle();
        }
    }
}
