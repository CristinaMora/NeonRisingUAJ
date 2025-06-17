using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class bowLight : MonoBehaviour
{
    [SerializeField]
    private Color activated;
    [SerializeField]
    private Color deactivated;
    private Light2D bowL;
    
    void Start()
    {
        bowL = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shotPointDetector.canShootArrow)
        {
            bowL.color = activated;
        }
        else
        {
            bowL.color = deactivated;
        }
    }
}
