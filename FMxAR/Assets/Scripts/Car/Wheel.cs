using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    private float lateralSlip;
    private float forwardSlip;
    private bool isSlipping;

    public GameObject wheelGraphic;
    private WheelCollider wheelCollider;

    // ScriptableObject that hold a wheel configuration we can build from
    public WheelProperties wheelProperties;
    


    public bool WheelGrounded
    {
        get
        {
            return wheelCollider.isGrounded;
        }
    }

    private bool InitWheelCollider()
    {
        // Grab the collider from the gameobject
        wheelCollider = GetComponentInChildren<WheelCollider>();

        if (wheelCollider == null)
        {
            Debug.LogError("ERROR: Could not initialise wheel: Missing Collider");
            return false;
        }

        if (wheelProperties == null)
        {
            Debug.LogError("ERROR: Could not initialise wheel: Missing Properties");
            return false;
        }  

        // Build friction curves based on scriptable objects animation curve
        // I have to do some wierd stuff here and cache the curve, as opposed to setting
        // them directly as unity complains when i try to change a property of the curve
        // when i set the stiffness if its attached to the collider.
        WheelFrictionCurve forCurve = BuildFrictionCurve(wheelProperties.forwardCurve);
        WheelFrictionCurve latCurve = BuildFrictionCurve(wheelProperties.lateralCurve);

        forCurve.stiffness = wheelProperties.forwardMultipier;
        latCurve.stiffness = wheelProperties.lateralMultiplier;

        wheelCollider.forwardFriction = forCurve;
        wheelCollider.sidewaysFriction = latCurve;
        

        // Set up suspension spring based on given values
        JointSpring jointSpring = new JointSpring();
        jointSpring.spring = wheelProperties.springyness;
        jointSpring.damper = wheelProperties.damp;

        wheelCollider.suspensionSpring = jointSpring;

        return true;
    }

    /// <summary>
    /// Function that takes in an animation curve and builds a friction curve from it.
    /// This allows us to alter friction values using a curve at runtime.
    /// </summary>
    /// <param name="curve">AnimationCurve to build from</param>
    /// <returns>A new WheelFrictionCurve based on parametered values.</returns>
    private WheelFrictionCurve BuildFrictionCurve(AnimationCurve curve)
    {
        // Verify that the AnimationCurve has at least 3 keys to sample from
        if (curve.length < 3)
        {
            // Returns a new WheelFrictionCurve with default values
            return new WheelFrictionCurve();
        }
        WheelFrictionCurve newCurve = new WheelFrictionCurve();

        newCurve.extremumValue = curve.keys[1].value;
        newCurve.extremumSlip = curve.keys[1].time;

        newCurve.asymptoteValue = curve.keys[2].value;
        newCurve.asymptoteSlip = curve.keys[2].value;


        return newCurve;
    }

    public WheelCollider GetWheelCollider()
    { 
        return wheelCollider;
    }

    private void Start()
    {
        InitWheelCollider();
    }

    private void Update()
    {
        UpdateGraphicPosition();
    }

    private void UpdateGraphicPosition()
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelGraphic.transform.position = position;
        wheelGraphic.transform.rotation = rotation;
    }
}
