using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistantsScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Immediately load the main menu scene
        GetComponent<LevelManager>().LoadLevel(1);
    }
}
