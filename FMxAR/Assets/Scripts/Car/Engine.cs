using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    [SerializeField]
    private float standardTorque;
    [SerializeField]
    private float boostTorque;
    [SerializeField]
    private float boost;

    public float boostRemovedPerSec;


    


    /// <returns>Current Engine torque based on input</returns>
    public float GetTorque(bool boosting)
    {
        if (boosting && boost > 0)
        {
            return boostTorque;
        }
        
        return standardTorque;
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
