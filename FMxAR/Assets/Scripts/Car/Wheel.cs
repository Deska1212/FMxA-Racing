using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wheel : MonoBehaviour
{
    public string name;
    /// <summary>
    /// Sideways slip
    /// </summary>
    [SerializeField] private float lateralSlip;
    /// <summary>
    /// Slip in the forward/back direction
    /// </summary>
    [SerializeField] private float forwardSlip;
    /// <summary>
    /// True if either lateral or forward slip is above slipping threshold
    /// </summary>
    [SerializeField] private bool isSlipping;

    /// <summary>
    /// 0 is no slip, 1 is full slip, wheel is deemed slipping above this value
    /// </summary>
    private const float SLIPPING_THRESHOLD = 0.5f;
    
    public ParticleSystem psys;
    
    /// <summary>
    /// Wheel graphic
    /// </summary>
    public GameObject wheelGraphic;
    /// <summary>
    /// Wheel collider
    /// </summary>
    private WheelCollider wheelCollider;
    /// <summary>
    /// Audio source for the wheel
    /// </summary>
    private AudioSource _audioSrc;
    /// <summary>
    /// How much the pitch is randomly shifted for effect
    /// </summary>
    public float pitchVariation;

    private float _startingPitch;

    /// <summary>
    /// The scriptable object config that this wheels values are built from
    /// </summary>
    public WheelProperties wheelProperties;
    /// <summary>
    ///  How loud the wheels are when traversing the ground normally; not slipping.
    /// </summary>
    public float baseWheelSoundModifier;

    /// <summary>
    /// True if the wheel is grounded on good terrain, false if the wheel is in the air or on bad terrain.
    /// </summary>
    public bool goodTerrain;
    /// <summary>
    /// Standard damping rate for the wheel
    /// </summary>
    public float baseDampingRate;
    /// <summary>
    /// The damping rate for the wheel when in the air or on bad terrain
    /// </summary>
    public float badTerrainDampingRate;

    /// <summary>
    /// Returns true if this wheel is grounded
    /// </summary>
    public bool WheelGrounded
    {
        get
        {
            return wheelCollider.isGrounded;
        }
    }

    /// <summary>
    /// Initializes wheel collider values
    /// </summary>
    /// <returns>False if failed to initialise</returns>
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
        jointSpring.targetPosition = wheelProperties.targetPos;

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

    /// <summary>
    /// Returns this wheels wheel collider
    /// </summary>
    /// <returns>This wheels collider</returns>
    public WheelCollider GetWheelCollider()
    { 
        return wheelCollider;
    }

    private void Start()
    {
        _audioSrc = GetComponent<AudioSource>();
        _startingPitch = _audioSrc.pitch;
        InitWheelCollider();
    }

    private void Update()
    {
        UpdateGraphicPosition();
        UpdateSlipValues();
        _audioSrc.pitch = RandomisePitch();
        goodTerrain = GoodTerrainCheck();
        isSlipping = IsSlipping();
        // Audio Volume of the slipping sound is high if were slipping, otherwise low but scales up with speed
        _audioSrc.volume = isSlipping ? Mathf.Max(lateralSlip, forwardSlip) : (CarController.instance.speedPerc / 5) * baseWheelSoundModifier;
        psys.enableEmission = isSlipping;


        GetWheelCollider().wheelDampingRate = goodTerrain ? baseDampingRate : badTerrainDampingRate;
        
    }

    private float RandomisePitch()
    {
        float rand = Random.Range(-pitchVariation, pitchVariation);
        return _startingPitch + rand;
    }

    private void UpdateSlipValues()
    {
        forwardSlip = Mathf.Abs(GetWheelHit().forwardSlip);
        lateralSlip = Mathf.Abs(GetWheelHit().sidewaysSlip);
    }

    /// <summary>
    /// Updates the position of the graphical wheel
    /// </summary>
    private void UpdateGraphicPosition()
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelGraphic.transform.position = position;
        wheelGraphic.transform.rotation = rotation;
    }

    /// <summary>
    /// Returns wheel hit information this frame
    /// </summary>
    /// <returns>Wheel hit</returns>
    public WheelHit GetWheelHit()
    {
        WheelHit hit;
        bool verify = wheelCollider.GetGroundHit(out hit);
        if (verify)
        { 
            return hit;
        }
        return new WheelHit();
    }

    /// <summary>
    /// Checks if this wheel is above the slipping threshold
    /// </summary>
    /// <returns>True if above slipping threshold</returns>
    public bool IsSlipping()
    {
        return lateralSlip > SLIPPING_THRESHOLD || forwardSlip > SLIPPING_THRESHOLD;
    }

    /// <summary>
    /// Checks for good terrain
    /// </summary>
    /// <returns>True if wheel on good terrain, false if in air or on bad terrain</returns>
    private bool GoodTerrainCheck()
    {
        if (WheelGrounded)
        {
            return GetWheelHit().collider.tag == "Track";
        }
        else
        {
            return false;
        }
    }

    public void OnDrawGizmos()
    {
        Color colour = Color.Lerp(Color.green, Color.red, forwardSlip);
        Gizmos.color = colour;
        Gizmos.DrawSphere(transform.position + (transform.forward * 0.35f), 0.25f);

        Color cubeColour = Color.Lerp(Color.green, Color.red, lateralSlip);
        Gizmos.color = cubeColour;
        Gizmos.DrawCube(transform.position, new Vector3(0.25f, 0.25f, 0.25f));
    }
}
