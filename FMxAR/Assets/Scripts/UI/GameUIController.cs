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

    public TextMeshProUGUI currentTime;
    public TextMeshProUGUI bestTime;


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
        currentTime.text = "Current: " + TimeTrialLevel.instance.levelData.currentTime.ToString("F2");
    }

    public void UpdateBestTimeUI()
    {
        bestTime.text = "Best: " + TimeTrialLevel.instance.levelData.bestTime.ToString("F2");
    }
}
