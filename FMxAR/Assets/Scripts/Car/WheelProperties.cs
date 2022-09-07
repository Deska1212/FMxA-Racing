using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Scriptable object that holds different configurations for wheel colliders
/// Allows for bulk chaning configs at runtime.
/// </summary>
[CreateAssetMenu(fileName = "WheelProps", menuName = "WheelProperties", order = 1)]
public class WheelProperties : ScriptableObject
{
    public float dampingRate;

    // Suspension
    public float springyness;
    public float damp;
    public float targetPos;

    // Curve that we will build friction curve from
    public AnimationCurve forwardCurve;
    public AnimationCurve lateralCurve;

    public float forwardMultipier;
    public float lateralMultiplier;
}
