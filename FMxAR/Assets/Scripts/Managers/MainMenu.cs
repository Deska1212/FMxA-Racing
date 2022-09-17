using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    // I could pull the data for the best time texts better by using the levels array index's and pulling them from the list.

    public TMP_Text trackOneBestTime;
    public TMP_Text trackTwoBestTime;
    public TMP_Text trackThreeBestTime;


    // Start is called before the first frame update
    void Start()
    {
        DisplayBestTimer();
        Time.timeScale = 1f; // We need to reset this because we set it to a lower value when we cross a finish time, and it is persistant through scenes.
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DisplayBestTimer()
    {
        trackOneBestTime.text = TimeTrialManager.instance.levelDatas[0].bestTime.ToString("F2");
        trackTwoBestTime.text = TimeTrialManager.instance.levelDatas[1].bestTime.ToString("F2");
        trackThreeBestTime.text = TimeTrialManager.instance.levelDatas[2].bestTime.ToString("F2");
    }
}
