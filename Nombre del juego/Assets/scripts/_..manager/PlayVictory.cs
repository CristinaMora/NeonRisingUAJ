using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayVictory : MonoBehaviour
{
    private void OnTriggerEnter2D()
    {
        GameManager.Instance.OnPlayerVictory();

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
