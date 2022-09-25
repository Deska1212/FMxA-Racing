using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialManager : MonoBehaviour
{
    public static TimeTrialManager instance;

    public List<TimeTrialLevelData> levelDatas;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        LoadAllBestTimes();
    }


    /// <summary>
    /// Save each of the Level Data's best times using player prefs
    /// </summary>
    public void SaveAllBestTimes()
    {
        foreach (TimeTrialLevelData data in levelDatas)
        {
            PlayerPrefs.SetFloat(data.trackIndex.ToString(), data.bestTime);
        }
    }


    /// <summary>
    /// Load and assign all of the level datas best times using player prefs
    /// </summary>
    public void LoadAllBestTimes()
    {
        for (int i = 0; i < levelDatas.Count; ++i)
        {
            // I have to copy the struct, adjust the value then apply it (Has to do with how C# and Unity deal with structs)
            TimeTrialLevelData dt = levelDatas[i];
            dt.bestTime = PlayerPrefs.GetFloat(dt.trackIndex.ToString());
            levelDatas[i] = dt;

        }
    }
}

[System.Serializable]
public struct TimeTrialLevelData
{
    public float bestTime;
    public float currentTime;
    public int trackIndex; // This is the backend track index in the array, not the one shown to the player
}
