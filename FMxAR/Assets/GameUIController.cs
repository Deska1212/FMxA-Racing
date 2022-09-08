using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    public static GameUIController instance;

    public Slider boostSlider;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBoostUI();
    }

    private void UpdateBoostUI()
    {
        boostSlider.value = CarController.instance.engine.GetBoostAmount();
    }
}
