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

    [SerializeField] private bool _canInput;
    [SerializeField] private bool _carGrounded;
    [SerializeField] private float _downforce;
    [SerializeField] private float _steeringAngle;
    [SerializeField] private float _steeringDamp;
    [SerializeField] private float _dampStart;
    [SerializeField] private float _dampMin;


    public Vector3 centerOfMass;
    public float speedPerc; // Percentage of max speed between 0 and 1, correlating to 
    public float maxSteeringAngle;
    public float maxDownforce; // This is downforce at maxSpeed
    public float minDownforce; // This is always applied
    public float maxSpeed;
    public float brakeTorque;


    // Singleton
    public static CarController instance;

    private Engine _engine;
    private Rigidbody _rb;


    public AudioSource _engineAudioSrc;
    public AudioSource _boostAudioSrc;

    [SerializeField] private float minPitch;
    [SerializeField] private float maxPitch;
    [SerializeField] private float pitchReturn;

    public float fovSmooth;


    public List<AxleInfo> axleInfos = new List<AxleInfo>();
    public TrailRenderer[] trails;
    public ParticleSystem speedLines;


    public float horizontalInput;
    public float brakeInput;
    public float reverseInput;
    public float throttleInput;
    public bool boostInput;

    public float v;

    // Start is called before the first frame update
    void Start()
    {
        InitializeCar();
    }

    

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
            HandlePCInputs();
        #endif

        

        _steeringAngle = (maxSteeringAngle * horizontalInput) * _steeringDamp;

        UpdateAxles();
        HandleDownForce();
        HandleSteeringDamp();
        HandleEngineSound();
        HandleFX();
        _carGrounded =  GetGroundedStatus();

        // Remove Boost if we are boosting
        _engine.RemoveBoost(boostInput ? throttleInput * Time.deltaTime : 0);

        // Set speedPerc to a percentage between 0 and 1, based on speed
        speedPerc = Mathf.InverseLerp(0, maxSpeed, _rb.velocity.magnitude);
        
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxSpeed);
    }

    private void HandleFX()
    {
        float camVel = 0;
        if (speedPerc > 0.85f && boostInput)
        {
            Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, 75f, ref camVel, fovSmooth * Time.deltaTime);
            foreach (TrailRenderer trail in trails)
            {
                trail.emitting = true;
            }
            speedLines.enableEmission = true;
        }
        else
        {
            Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, 60f, ref camVel, fovSmooth * Time.deltaTime);
            foreach (TrailRenderer trail in trails)
            {
                trail.emitting = false;
            }
            speedLines.enableEmission = false;

        }
    }

    private void HandleEngineSound()
    {
        float vel = 0;
        if (throttleInput > 0 || reverseInput > 0)
        {
            float target = Mathf.Lerp(minPitch, maxPitch, speedPerc);
            _engineAudioSrc.pitch = Mathf.SmoothDamp(_engineAudioSrc.pitch, target, ref vel, (pitchReturn * Time.deltaTime) * 10);
        }
        else
        {
            _engineAudioSrc.pitch -= pitchReturn * Time.deltaTime;
        }

        _engineAudioSrc.pitch = Mathf.Clamp(_engineAudioSrc.pitch, minPitch, maxPitch);

        if (boostInput && _engine.GetBoostAmount() > 0 && !_boostAudioSrc.isPlaying)
        {
            _boostAudioSrc.PlayOneShot(_boostAudioSrc.clip);
        }
        else if(!boostInput)
        {
            _boostAudioSrc.Stop();
        }
    }

    private void HandleSteeringDamp()
    {
        _steeringDamp = Mathf.InverseLerp(1, _dampStart, speedPerc);
        // Clamp steeringDamp
        _steeringDamp = Mathf.Clamp(_steeringDamp, _dampMin, 1f);
    }

    private void HandlePCInputs()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        throttleInput = Input.GetKey(KeyCode.W) ? 1 : 0;
        throttleInput = Input.GetKey(KeyCode.S) ? -1 : throttleInput;
        brakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
        boostInput = Input.GetKey(KeyCode.LeftShift);
    }

    private void UpdateAxles()
    {
        foreach (AxleInfo info in axleInfos)
        {
            if (info.steer)
            {
                foreach (Wheel wheel in info.wheels)
                {
                    wheel.GetWheelCollider().steerAngle = _steeringAngle;
                }
            }

            if (info.motor)
            {
                foreach (Wheel wheel in info.wheels)
                {
                    wheel.GetWheelCollider().motorTorque = _engine.currentTorque * throttleInput;

                }
            }

            foreach (Wheel wheel in info.wheels)
            {
                wheel.GetWheelCollider().brakeTorque = brakeInput * brakeTorque;
            }
        }
    }

    private void HandleDownForce()
    {
        // Calculate downforce based on speed
        _downforce = speedPerc * maxDownforce;
        _downforce = Mathf.Clamp(_downforce, minDownforce, maxDownforce);
        _rb.AddForce(Vector3.down * _downforce);
    }

    private void InitializeCar()
    {
        _rb = GetComponent<Rigidbody>();
        _engine = GetComponent<Engine>();
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
