using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using TMPro;

/// <summary>
/// Contains information and methods to finish a time trial level
/// </summary>
public class TimeTrialLevel : MonoBehaviour
{
    public static TimeTrialLevel instance;

    public TimeTrialLevelData levelData;
    public int levelIndex;
    public bool startingCountdown = true;
    public bool levelFinished;

    public float countdownTimer;
    public TextMeshProUGUI countdownTimerText;

    private void Start()
    {
        instance = this;
        bool fail = RetrieveLevelData();
        levelData.currentTime = 0f;
        StartCoroutine(LevelStartRoutine());    
    }

    private void Update()
    {
        if (countdownTimer > 0f)
        {
            countdownTimer -= Time.deltaTime;
            countdownTimerText.text = countdownTimer.ToString("F1");
        }
        else
        {
            countdownTimer = 0;
            countdownTimerText.text = "GO!";
            // Start lerping out the alpha
            if (!LeanTween.isTweening())
            { 
                LeanTween.scale(countdownTimerText.gameObject, Vector3.zero, 1f).setEase(LeanTweenType.easeInBack);
            }
        }

        if (startingCountdown == false)
        { 
            levelData.currentTime += Time.deltaTime;
        }
    }
    public void FinishLevel()
    {
        // Cache the new data in the TimeTrialManager
        Debug.Log("Level Finished with a time of " + levelData.currentTime);
        levelFinished = true;
        CarController.instance.userInput = false;

        float finishTime = levelData.currentTime;

        if (finishTime < levelData.bestTime || levelData.bestTime == 0f)
        { 
            // New best time
            levelData.bestTime = finishTime;
            GameUIController.instance.UpdateBestTimeUI();
        }

        // Update our levelData
        TimeTrialManager.instance.levelDatas[levelData.trackIndex] = levelData;

        // Save the best times to player prefs
        TimeTrialManager.instance.SaveAllBestTimes();

        // Pull up the end level scene

        // Drop the time scale for effect
        Time.timeScale = 0.3f;

        // Chromatic Abberation for effect
        ChromaticAberration chrom;
        bool vol = GetComponent<Volume>().profile.TryGet<ChromaticAberration>(out chrom);
        Debug.Log(vol);
        chrom.intensity.value = 1f;
        

        
        StartCoroutine(ReturnToMenuRoutine());
    }

    public IEnumerator LevelStartRoutine()
    {
        // Countdown
        yield return new WaitForSeconds(3f);

        // Enable input
        CarController.instance.userInput = true;

        // Start current time counter
        startingCountdown = false;
    }

    public IEnumerator ReturnToMenuRoutine()
    {
        // Countdown
        yield return new WaitForSeconds(5f);

        // Return us to the main menu
    }

    public bool RetrieveLevelData()
    {
        if (TimeTrialManager.instance == null)
        {
            return false;
        }
        levelData = TimeTrialManager.instance.levelDatas[levelIndex];
        return true;
    }

}
