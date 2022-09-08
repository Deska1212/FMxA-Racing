using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    [SerializeField]
    private float standardTorque;
    [SerializeField]
    private float minTorque;
    [SerializeField]
    private float boostTorque;
    [SerializeField]
    private float boost;

    private const float boostAcqWhenInThreshold = 20f;
    private const float boostAcqSpeedThreshold = 0.4f;
    private const float boostRemovedPerSec = 10f;

    

    public float boostAcquisition;
    public float currentTorque;


    private void Update()
    {
        currentTorque = GetTorque(CarController.instance.boostInput);
        HandleBoostAcquisition();
        boost = Mathf.Clamp(boost, 0, 100f);
    }

    private void HandleBoostAcquisition()
    {
        if (!CarController.instance.boostInput && CarController.instance.speedPerc < boostAcqSpeedThreshold)
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
            return t;
        }

        t = Mathf.Lerp(standardTorque, minTorque, CarController.instance.speedPerc);
        Debug.Log(t);
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
