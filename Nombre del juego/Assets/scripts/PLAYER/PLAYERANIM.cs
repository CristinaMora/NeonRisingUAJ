using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLAYERANIM : MonoBehaviour
{
    static public PLAYERANIM pANIMATOR;
    #region IdleAnim
    [SerializeField]
    private Sprite[] pIdleANIM;
    [SerializeField]
    private float idleSpeed;
    private float idleProgression = 0;
    #endregion

    [SerializeField]
    private Sprite[] pRunANIM;

    [SerializeField]
    private float runSpeed;

    [SerializeField]
    private Sprite[] pJumpANIM;
    [SerializeField]
    private Sprite[] pFallingANIM;
    [SerializeField]
    private Sprite[] pdashANIM;


    public bool isRunning = false;
    public bool isJumping = false;
    public bool isFalling = false;
    public bool isDashing = false;

    private SpriteRenderer playerRenderer;
    void Start()
    {
        pANIMATOR = this;
        playerRenderer = GetComponent<SpriteRenderer>();
    }
    public void Idle()
    {
        if (idleProgression < 1)
        {
            idleProgression += Time.deltaTime * idleSpeed;
            if (IDLEi == pIdleANIM.Length)
            {
                IDLEi = 0;
            }
            playerRenderer.sprite = pIdleANIM[IDLEi];
        }
        else if (idleProgression >= 1)
        {
            IDLEi++;
            idleProgression = 0;
        }
    }
    public void Running()
    {
        if (idleProgression < 1)
        {
            idleProgression += Time.deltaTime * runSpeed;
            if (RUNi == pRunANIM.Length)
            {
                RUNi = 0;
            }
            playerRenderer.sprite = pRunANIM[RUNi];
        }
        else if (idleProgression >= 1)
        {
            RUNi++;
            idleProgression = 0;
        }

    }

    private int IDLEi = 0;
    private int RUNi = 0;
    // Update is called once per frame
    void Update()
    {
        
        if (isJumping)
        {
            playerRenderer.sprite = pJumpANIM[0];
        }
        else if (isFalling)
        {
            playerRenderer.sprite = pFallingANIM[0];
        }
        else if (isRunning)
        {
            Running();
        }
        else if (isDashing)
        {
            playerRenderer.sprite = pdashANIM[0];
        }
        else
        {
            Idle();
        }
        
                 

    }
}
