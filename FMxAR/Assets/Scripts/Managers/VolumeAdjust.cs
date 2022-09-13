using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeAdjust : MonoBehaviour
{

    public AudioSource[] srcs;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (AudioSource src in srcs)
        {
            src.volume = GetComponent<Slider>().value;
        }
    }
}
