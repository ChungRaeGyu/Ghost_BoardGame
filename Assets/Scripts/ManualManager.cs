using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ManualManager : MonoBehaviour
{
    public GameObject Manual;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OpenManual(){
        Manual.SetActive(true);
    }
    public void CloseManual(){
        Manual.SetActive(false);
    }
}
