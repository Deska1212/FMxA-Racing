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
    [SerializeField] private bool _userInput; // Is user input enabled
    [SerializeField] private bool _carGrounded; // Are all 4 wheels grounded

    [Space(5)]
    [Header("Readouts")]
    [SerializeField] private float _currentDownforce; // The current downforce exterted on the car
    [SerializeField] private float _currentSteeringAngle; // The current steering angle of the wheel
    [SerializeField] private float _currentSteeringDamp; // The current steering damp modifier. Steering damp is at full when its 0, and takes no effect at one (1 - No effect, 0 - Max effect)
    public float speedPerc; // Percentage of max speed between 0 and 1


    [Space(5)]
    [Header("Steering properties")]
    [Range(0, 1)] public float dampStartPerc; // This is the % of maxSpeed the car must be before steering damp takes effect
    [Range(0, 1)] public float minSteerDamp; // Steering damp @ maxSpeed
    public float maxSteeringAngle; // Maximum steering angle the vehicles steering capable wheels will turn with no damping effect


    [Space(5)]
    [Header("Physical Properties")]
    public Vector3 centerOfMass;
    public float maxDownforce; // This is downforce at maxSpeed
    public float minDownforce; // Downforce clamped here, this is downforce a 0% maxSpeed
    public float maxSpeed; // Maximum speed of the car in m/s
    public float brakeTorque; // Brake torque exerted by each wheel when braking


    // Singleton
    public static CarController instance;

    // Reference variables
    [HideInInspector] public Engine engine;
    private Rigidbody _rb;


    [Space(5)]
    [Header("Audio FX")]
    public AudioSource engineAudioSrc; // Responsible for playing pitch shifting engine sound
    public AudioSource boostAudioSrc; // Only plays when we are boosting

    [Space(3)]
    [SerializeField] public float minPitch; // Min pitch engine audio will go to - Idle pitch
    [SerializeField] public float maxPitch; // The max pitch the engine audio will go to
    [SerializeField] public float pitchReturn; // How fast the engines pitch will return to idle

    [Space(5)]
    public List<AxleInfo> axleInfos = new List<AxleInfo>();

    [Space(5)]
    [Header("Visual FX")]
    public TrailRenderer[] trails; // Speed trails active when boosting and above threshold
    public ParticleSystem speedLines; // "Wind" lines when traveling fast
    public float nominalFov; // Standard FOV for the camera when not movng
    public float movingFov; // FOV when travelling normally.
    public float boostFov; // FOV when travelling fast
    public float fovDamp; // The damping for fov zoom out when travelling fast, high values denote smoother movement

    private float desiredFov;


    [Space(5)]
    [Header("Brake Light FX")]
    public MeshRenderer brakeLightsRenderer;
    public Material brakeLightsStatic;
    public Material brakeLightsEmissive;

    public Light[] brakeLights;



    // Hidden Input values
    

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

    private void HandleBrakeLights()
    {
        brakeLightsRenderer.material = CrossPlatformInputController.instance.brakeInput == 1 ? brakeLightsEmissive : brakeLightsStatic;

        foreach (Light l in brakeLights)
        {
            l.enabled = CrossPlatformInputController.instance.brakeInput == 1;
        }
        
    }

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
        Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, desiredFov, ref camVel, fovDamp * Time.deltaTime);
    }

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

    private void HandleSteeringDamp()
    {
        _currentSteeringDamp = Mathf.InverseLerp(1f, dampStartPerc, speedPerc);
        // Clamp steeringDamp
        _currentSteeringDamp = Mathf.Clamp(_currentSteeringDamp, minSteerDamp, 1f);
    }

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

    private void HandleDownForce()
    {
        // Calculate downforce based on speed
        _currentDownforce = speedPerc * maxDownforce;
        _currentDownforce = Mathf.Clamp(_currentDownforce, minDownforce, maxDownforce);
        _rb.AddForce(Vector3.down * _currentDownforce);
    }

    private void InitializeCar()
    {
        _rb = GetComponent<Rigidbody>();
        engine = GetComponent<Engine>();
        instance = this;

        _rb.centerOfMass = _rb.centerOfMass + centerOfMass;
    }

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

}

/// <summary>
/// Serializable struct that holds information on the cars wheels
/// </summary>
[System.Serializable]
public struct AxleInfo
{
    public string name;
    public bool motor;
    public bool steer;
    public List<Wheel> wheels;
}
