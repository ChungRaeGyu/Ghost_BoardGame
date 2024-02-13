using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class point : MonoBehaviour
{
    GameManager gamemanager;
    int myname;
    // Start is called before the first frame update
    void Start()
    {
        gamemanager = GameObject.Find("GameManager").GetComponent<GameManager>();
        myname = Convert.ToInt32(gameObject.name);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
