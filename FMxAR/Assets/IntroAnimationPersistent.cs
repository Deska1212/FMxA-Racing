using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroAnimationPersistent : MonoBehaviour
{

    public static IntroAnimationPersistent instance;


    public bool hasShownIntroAnimation;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
