using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine
{
    [SerializeField]
    private float standardTorque;
    [SerializeField]
    private float boostTorque;
    [SerializeField]
    private float boost;


    private bool isBoosting;

    /// <summary>
    /// Get: Returns true if the engine is boosting, Set: Sets wether or not the engine is boost.
    /// </summary>
    public bool IsBoosting
    {
        get { return isBoosting; }
        set { isBoosting = value; }
    }

    /// <returns>Current Engine torque based on input</returns>
    public float GetTorque()
    { 
        // Check if we are boosting and return either standardTorqie or boostTorque
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
}
