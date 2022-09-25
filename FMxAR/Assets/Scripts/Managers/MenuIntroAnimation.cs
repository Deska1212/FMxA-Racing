using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuIntroAnimation : MonoBehaviour
{
    public RectTransform title;
    public RectTransform trackPanel;
    public RectTransform selectTrackText;
    public RectTransform exitButton;

    public float startingDelay;
    public float scaleUpTime;

    public float titleMoveTime;

    public Vector3 titleEndPos;
    public Vector3 titleEndScale;


    // Start is called before the first frame update
    void Start()
    {
        startingDelay = IntroAnimationPersistent.instance.hasShownIntroAnimation == true ? 0.25f : 2.5f; // The title text gets tweened faster if we are returning from a track.
        TitleSequence();
    }

    void TitleSequence()
    {
        LeanTween.move(title, titleEndPos, titleMoveTime).setDelay(startingDelay).setEaseInBack();
        LeanTween.scale(title, titleEndScale, titleMoveTime).setDelay(startingDelay).setEaseInBack().setOnComplete(OtherComponentsSequence);
    }

    void OtherComponentsSequence()
    { 
        LeanTween.scale(trackPanel, Vector3.one, scaleUpTime).setEaseOutBack();
        LeanTween.scale(selectTrackText, Vector3.one, scaleUpTime).setEaseOutBack();
        LeanTween.scale(exitButton, Vector3.one, scaleUpTime).setEaseOutBack();
        SpeedUpForReturning();
        
    }

    private void SpeedUpForReturning()
    {
        IntroAnimationPersistent.instance.hasShownIntroAnimation = true;
    }
}
