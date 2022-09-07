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
    private Engine _engine;
    private Rigidbody _rb;
    [SerializeField] private float _steeringAngle;
    [SerializeField] private float _steeringDamp;
    [SerializeField] private float _downforce;
    private const float _minSteeringDamp = 0.5f;

    public Vector3 centerOfMass;
    public float maxSteeringAngle;
    public float maxDownforce; // This is downforce at maxSpeed
    public float maxSpeed;
    public float brakeTorque;

    // Singleton
    public static CarController instance;


    public List<AxleInfo> axleInfos = new List<AxleInfo>();


    public float horizontalInput;
    public float brakeInput;
    public float reverseInput;
    public float throttleInput;
    public bool boostInput;

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
        _carGrounded =  GetGroundedStatus();

        // Remove Boost if we are boosting
        _engine.RemoveBoost(boostInput ? throttleInput * Time.deltaTime : 0);

        // Clamp steeringDamp
        _steeringDamp = Mathf.Clamp(_steeringDamp, _minSteeringDamp, 1f);
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxSpeed);
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
                    wheel.GetWheelCollider().motorTorque = _engine.GetTorque(boostInput) * throttleInput;

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
        _downforce = Mathf.InverseLerp(0, maxSpeed, _rb.velocity.magnitude) * maxDownforce;
        _rb.AddForce(Vector3.down * _downforce);
    }

    private void InitializeCar()
    {
        _rb = GetComponent<Rigidbody>();
        _engine = GetComponent<Engine>();

        _rb.centerOfMass = centerOfMass;
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
