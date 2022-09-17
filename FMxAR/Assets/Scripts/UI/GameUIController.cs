using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    public static GameUIController instance;


    public Slider boostSlider;

    public TextMeshProUGUI currentTimeText;
    public TextMeshProUGUI bestTimeText;

    public TextMeshProUGUI winText;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        UpdateBestTimeUI();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBoostUI();
        UpdateCurrentTimeUI();
    }

    private void UpdateBoostUI()
    {
        boostSlider.value = CarController.instance.engine.GetBoostAmount();
    }

    private void UpdateCurrentTimeUI()
    {
        currentTimeText.text = "Current: " + TimeTrialLevel.instance.levelData.currentTime.ToString("F2");
    }

    public void UpdateBestTimeUI()
    {
        bestTimeText.text = "Best: " + TimeTrialLevel.instance.levelData.bestTime.ToString("F2");
    }

    public void ActivateWinText()
    {
        winText.gameObject.SetActive(true);
        UpdateWinText();
        LeanTween.scale(winText.gameObject, Vector3.one, 1.25f).setEase(LeanTweenType.easeOutBack);
    }

    private void UpdateWinText()
    {
        string text = "Track complete with a time of \n" + TimeTrialLevel.instance.levelData.currentTime.ToString("F2") + "s \n\n Returning to main menu...";
        winText.text = text;
    }
}
