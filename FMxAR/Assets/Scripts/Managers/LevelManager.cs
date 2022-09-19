using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadLevel(int index)
    { 
        SceneManager.LoadScene(index);
    }

    /// <summary>
    /// Resets the current scene.
    /// </summary>
    public void ResetLevel()
    {
        Debug.Log("Level Reset");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
