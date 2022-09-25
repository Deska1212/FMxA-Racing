using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioClip intro;
    public AudioClip loop;

    public AudioSource musicSrc;

    public bool introTrackComplete;


    // Start is called before the first frame update
    void Start()
    {
        // Play the intro sequence to the games music
        musicSrc.PlayOneShot(intro);
        introTrackComplete = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Loop the loopable section
        if (!musicSrc.isPlaying)
        {
            musicSrc.PlayOneShot(loop);
        }
    }
}
