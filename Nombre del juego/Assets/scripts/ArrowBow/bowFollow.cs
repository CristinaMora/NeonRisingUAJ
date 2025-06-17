using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bowFollow : MonoBehaviour
{
    [SerializeField]
    private GameObject playerREF;

    // Update is called once per frame
    void Update()
    {
        transform.position = playerREF.transform.position;
    }
}
