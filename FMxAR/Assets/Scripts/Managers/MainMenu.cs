using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f; // We need to reset this because we set it to a lower value when we cross a finish time, and it is persistant through scenes.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
