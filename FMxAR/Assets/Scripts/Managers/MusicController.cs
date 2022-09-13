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
        musicSrc.PlayOneShot(intro);
        introTrackComplete = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!musicSrc.isPlaying)
        {
            musicSrc.PlayOneShot(loop);
        }
    }
}
