using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for handling input from various input sources
/// and setting power and steering angles to the wheels
/// </summary>
public class CarController : MonoBehaviour
{
    /// <summary>
    /// Is user input enabled
    /// </summary>
    public bool userInput;

    /// <summary>
    /// True if all 4 wheels are grounded
    /// </summary>
    [SerializeField] private bool _carGrounded;

    [Space(5)]
    [Header("Readouts")]
    /// <summary>
    /// The current downforce exterted on the car
    /// </summary>
    [SerializeField] private float _currentDownforce;
    /// <summary>
    /// The current steering angle of the wheel
    /// </summary>
    [SerializeField] private float _currentSteeringAngle;
    /// <summary>
    /// The current steering damp modifier. Steering damp is at full when its 0, and takes no effect at one (1 - No effect, 0 - Max effect)
    /// </summary>
    [SerializeField] private float _currentSteeringDamp; 
    /// <summary>
    /// Percentage of max speed the car is currently at between 0 and 1
    /// </summary>
    public float speedPerc; // Percentage of max speed between 0 and 1


    [Space(5)]
    [Header("Steering properties")]

    /// <summary>
    /// The percentage of max speed that must be reached before steering damp starts to come into effect
    /// </summary>
    [Range(0, 1)] public float dampStartPerc;
    /// <summary>
    /// Steering damp @ max speed
    /// </summary>
    [Range(0, 1)] public float minSteerDamp;
    /// <summary>
    /// Maximum steering angle the vehicles steering capable wheels will turn with no damping effect
    /// </summary>
    public float maxSteeringAngle;


    [Space(5)]
    [Header("Physical Properties")]
    /// <summary>
    /// The vehicles center of mass, set slightly below body center for stability
    /// </summary>
    public Vector3 centerOfMass;
    /// <summary>
    /// Downforce at standstill
    /// </summary>
    public float minDownforce;
    /// <summary>
    /// Downforce at max speed
    /// </summary>
    public float maxDownforce;
    /// <summary>
    /// Vehicles max speed in m/s
    /// </summary>
    public float maxSpeed;
    /// <summary>
    /// Brake torque exerted on non-steering wheels when braking.
    /// </summary>
    public float brakeTorque;


    // Singleton
    public static CarController instance;

    // Reference variables
    [HideInInspector] public Engine engine;
    private Rigidbody _rb;


    [Space(5)]
    [Header("Audio FX")]
    public AudioSource engineAudioSrc; // Responsible for playing pitch shifting engine sound
    public AudioSource boostAudioSrc; // Only plays when we are boosting
    public AudioSource hitAudioSrc; // Plays when the car hits something

    private float hitSrcDefaultPitch; // Keep track of default pitch

    [Space(3)]
    ///<summary>Engine pitch at idle</summary>
    [SerializeField] public float minPitch;
    /// <summary>Engine pitch at max speed</summary>
    [SerializeField] public float maxPitch;
    /// <summary>How fast the engine pitch will return to idle</summary>
    [SerializeField] public float pitchReturn;

    [Space(5)]
    ///<summary>Axle information list</summary>
    public List<AxleInfo> axleInfos = new List<AxleInfo>();

    [Space(5)]
    [Header("Visual FX")]
    ///<summary>Speed trails that are activated when boosting going fast</summary>
    public TrailRenderer[] trails; // Speed trails active when boosting and above threshold
    /// <summary>"Wind" lines that are active when travelling fast</summary>
    public ParticleSystem speedLines; // "Wind" lines when traveling fast
    /// <summary>Exhaust from car when boosting</summary>
    public ParticleSystem boostExhaust; // Exhaust from the car when boosting
    /// <summary>Standard FOV for the camera when not moving</summary>
    public float nominalFov;
    /// <summary>FOV when travelling normally</summary>
    public float movingFov;
    /// <summary>FOV when travelling fast</summary>
    public float boostFov;
    /// <summary>The damping for fov zoom out when travelling fast, high values denote smoother movement</summary>
    public float fovDamp;

    private float desiredFov;


    [Space(5)]
    [Header("Brake Light FX")]
    public MeshRenderer brakeLightsRenderer;
    public Material brakeLightsStatic;
    public Material brakeLightsEmissive;

    public Light[] brakeLights;
    

    // Start is called before the first frame update
    void Start()
    {
        InitializeCar();
    }

    

    // Update is called once per frame
    void Update()
    {

        

        _currentSteeringAngle = (maxSteeringAngle * CrossPlatformInputController.instance.horizontalInput) * _currentSteeringDamp;

        UpdateAxles();
        HandleDownForce();
        HandleSteeringDamp();
        HandleEngineSound();
        HanldeVisualFX();
        HandleBrakeLights();
        _carGrounded =  GetGroundedStatus();

        // Remove Boost if we are boosting
        engine.RemoveBoost(CrossPlatformInputController.instance.boostInput ? CrossPlatformInputController.instance.throttleInput * Time.deltaTime : 0);

        // Set speedPerc to a percentage between 0 and 1, based on speed
        speedPerc = Mathf.InverseLerp(0, maxSpeed, _rb.velocity.magnitude);
        
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxSpeed);
    }

    /// <summary>
    /// Handling activating/deactivation brake lights and emissive material when the player is braking
    /// </summary>
    private void HandleBrakeLights()
    {
        brakeLightsRenderer.material = CrossPlatformInputController.instance.brakeInput == 1 ? brakeLightsEmissive : brakeLightsStatic;

        foreach (Light l in brakeLights)
        {
            l.enabled = CrossPlatformInputController.instance.brakeInput == 1;
        }
        
    }

    /// <summary>
    /// Handles speed and boosting FX
    /// </summary>
    private void HanldeVisualFX()
    {
        float camVel = 0;
        if (speedPerc > 0.85f)
        {
            // Car is currently going very fast
            if (engine.isBoosting)
            {
                // Car is currently boosting
                desiredFov = boostFov;

                // Boosting camera effect
                speedLines.enableEmission = true;
            }
            
            // Speed Trails
            foreach (TrailRenderer trail in trails)
            {
                trail.emitting = true;
            }
        }
        else if (speedPerc > 0.1f && CrossPlatformInputController.instance.throttleInput >= 1 && CrossPlatformInputController.instance.brakeInput == 0)
        {
            // Car is currently moving and not braking
            desiredFov = movingFov;
            foreach (TrailRenderer trail in trails)
            {
                trail.emitting = false;
            }
            speedLines.enableEmission = false;

        }
        else
        {
            // Car is currently stationary
            desiredFov = nominalFov;
            foreach (TrailRenderer trail in trails)
            {
                trail.emitting = false;
            }
            speedLines.enableEmission = false;
        }

        boostExhaust.enableEmission = engine.isBoosting;

        Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, desiredFov, ref camVel, fovDamp * Time.deltaTime);
    }

    /// <summary>
    /// Handles the boosting sound and the pitch shifted engine sound using smooth damp. Clamps between min & max values.
    /// </summary>
    private void HandleEngineSound()
    {
        float vel = 0;
        if (CrossPlatformInputController.instance.throttleInput > 0 || CrossPlatformInputController.instance.reverseInput > 0)
        {
            float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedPerc);
            engineAudioSrc.pitch = Mathf.SmoothDamp(engineAudioSrc.pitch, targetPitch, ref vel, (pitchReturn * Time.deltaTime) * 10);
        }
        else
        {
            engineAudioSrc.pitch -= pitchReturn * Time.deltaTime;
        }

        engineAudioSrc.pitch = Mathf.Clamp(engineAudioSrc.pitch, minPitch, maxPitch);





        if (CrossPlatformInputController.instance.boostInput && engine.GetBoostAmount() > 0 && !boostAudioSrc.isPlaying)
        {
            boostAudioSrc.PlayOneShot(boostAudioSrc.clip);
        }
        else if(!CrossPlatformInputController.instance.boostInput)
        {
            boostAudioSrc.Stop();
        }
    }

    /// <summary>
    /// Calculates and applies steering damp to final input value to then get sent to the wheels
    /// </summary>
    private void HandleSteeringDamp()
    {
        _currentSteeringDamp = Mathf.InverseLerp(1f, dampStartPerc, speedPerc);
        // Clamp steeringDamp
        _currentSteeringDamp = Mathf.Clamp(_currentSteeringDamp, minSteerDamp, 1f);
    }

    /// <summary>
    /// Updates all of the axles torque, steering angle, and brake torque based on user input that frame.
    /// </summary>
    private void UpdateAxles()
    {
        foreach (AxleInfo info in axleInfos)
        {
            if (info.steer)
            {
                foreach (Wheel wheel in info.wheels)
                {
                    wheel.GetWheelCollider().steerAngle = _currentSteeringAngle;
                }
            }

            if (!info.steer)
            {
                foreach (Wheel wheel in info.wheels)
                { 
                    wheel.GetWheelCollider().brakeTorque = CrossPlatformInputController.instance.brakeInput * brakeTorque;
                }
            }

            if (info.motor)
            {
                foreach (Wheel wheel in info.wheels)
                {
                    wheel.GetWheelCollider().motorTorque = engine.currentTorque * CrossPlatformInputController.instance.throttleInput;

                }
            }

            
        }
    }

    /// <summary>
    /// Calculates and applies downforce
    /// </summary>
    private void HandleDownForce()
    {
        // Calculate downforce based on speed
        _currentDownforce = speedPerc * maxDownforce;
        _currentDownforce = Mathf.Clamp(_currentDownforce, minDownforce, maxDownforce);
        _rb.AddForce(Vector3.down * _currentDownforce);
    }

    /// <summary>
    /// Initializes components and rigidbody
    /// </summary>
    private void InitializeCar()
    {
        _rb = GetComponent<Rigidbody>();
        engine = GetComponent<Engine>();
        instance = this;

        _rb.centerOfMass = _rb.centerOfMass + centerOfMass;
    }

    /// <summary>
    /// Checks wether the cars wheels are grounded
    /// </summary>
    /// <returns>True if ALL 4 wheels are grounded, otherwise false</returns>
    private bool GetGroundedStatus()
    {
        foreach (AxleInfo info in axleInfos)
        {
            foreach (Wheel wheel in info.wheels)
            {
                if (wheel.GetWheelCollider().isGrounded == false)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Check for level finished trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "LevelEnd")
        {
            TimeTrialLevel.instance.FinishLevel();
        }
    }

    // Check if we have collided with another gameobjects and play a collision sound.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.isStatic)
        {
            hitAudioSrc.pitch = hitSrcDefaultPitch + UnityEngine.Random.Range(-0.2f, 0.2f);
            hitAudioSrc.Play();
        }
    }

}

/// <summary>
/// Serializable struct that holds information on the cars wheels
/// </summary>
[System.Serializable]
public struct AxleInfo
{
    ///<summary>Axle name (front, rear)</summary>
    public string name;
    ///<summary>Is this axle powered?</summary>
    public bool motor;
    ///<summary>Is this axle capable of steering?</summary>
    public bool steer;
    ///<summary>Wheels on this axle</summary>
    public List<Wheel> wheels;
}
