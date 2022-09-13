using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    [SerializeField]
    private float standardTorque;   // Standard torque we provide when still (Torque scales down with speed)
    [SerializeField]
    private float minTorque;        // Minimum torque we will scale down to
    [SerializeField]
    private float boostTorque;      // Torque we provide when boosting
    [SerializeField]
    private float boost;            // How much boost we have stored

    public float boostAcqWhenInThreshold = 20f;   // Boost we get per second we are within threshold
    public float boostAcqSpeedThreshold = 0.85f;  // Percentage of speed we have to be below before we start getting boost
    public float boostRemovedPerSec = 10f;        // Boost we loose per second when boosting

    public bool isBoosting; // Are we boosting, use this for checking with other classes, not CarController.boostInput, as that is for checking input

    public float boostAcquisition; // Boost we are currently getting per second
    public float currentTorque; // Torque our engine is currently able to exert to the wheels


    private void Update()
    {
        currentTorque = GetTorque(CrossPlatformInputController.instance.boostInput);
        HandleBoostAcquisition();
        boost = Mathf.Clamp(boost, 0, 100f);
    }

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
    /// Sets the boost to value.
    /// </summary>
    /// <param name="setTo">Value to set boost to.</param>
    public void SetBoostToValue(float setTo)
    { 
        
    }

    public void RemoveBoost(float value)
    {
        boost -= boostRemovedPerSec * value;
    }
}
