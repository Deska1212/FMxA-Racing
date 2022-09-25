using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    ///<summary>Standard torque we provide when still (Torque scales down with speed)</summary>
    [SerializeField] private float standardTorque;
    ///<summary>Minimum torque</summary>
    [SerializeField] private float minTorque;
    ///<summary>Torque while boosting</summary>
    [SerializeField] private float boostTorque;
    ///<summary>How much boost we have reserved</summary>
    [SerializeField] private float boost;

    /// <summary>Boost we get per second we are within threshold</summary>
    public float boostAcqWhenInThreshold = 20f;
    /// <summary>Percentage of speed we have to be below before we start getting boost</summary>
    public float boostAcqSpeedThreshold = 0.85f;
    /// <summary>Boost we loose per second when boosting</summary>
    public float boostRemovedPerSec = 10f;
    /// <summary>Are we boosting, use this for checking with other classes, not CarController.boostInput, as that is for checking input</summary>
    public bool isBoosting;

    /// <summary>
    /// Boost whe are currently getting per second
    /// </summary>
    public float boostAcquisition;
    /// <summary>
    /// Torque our engine is currently able to exert to the wheels
    /// </summary>
    public float currentTorque;


    private void Update()
    {
        currentTorque = GetTorque(CrossPlatformInputController.instance.boostInput);
        HandleBoostAcquisition();
        boost = Mathf.Clamp(boost, 0, 100f);
    }

    /// <summary>
    /// Calcuates how much boost we should be aquiring per frame and adds it to our reserve
    /// </summary>
    private void HandleBoostAcquisition()
    {
        if (!CrossPlatformInputController.instance.boostInput && CarController.instance.speedPerc < boostAcqSpeedThreshold)
        {
            boostAcquisition = boostAcqWhenInThreshold * Time.deltaTime;
        }
        else
        {
            boostAcquisition = 0;
        }
        boost += boostAcquisition;
    }


    /// <summary>
    /// Gets torque based on input
    /// </summary>
    /// <returns>Current Engine torque based on input</returns>
    public float GetTorque(bool boosting)
    {
        float t;
        if (boosting && boost > 0)
        {
            t = boostTorque;
            isBoosting = true;
            return t;
        }
        isBoosting = false;
        t = Mathf.Lerp(standardTorque, minTorque, CarController.instance.speedPerc);
        return t;
    }


    /// <returns>How much boost this car has</returns>
    public float GetBoostAmount()
    { 
        return boost;
    }

    /// <summary>
    /// Removes boost from our reserve
    /// </summary>
    /// <param name="value">How much boost we are removing</param>
    public void RemoveBoost(float value)
    {
        boost -= boostRemovedPerSec * value;
    }
}
