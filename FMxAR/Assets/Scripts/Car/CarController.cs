using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for handling input from various input sources
/// and setting power and steering angles to the wheels
/// </summary>
public class CarController : MonoBehaviour
{

    private bool _canInput;
    private bool _carGrounded;
    private Engine _engine;
    [SerializeField] private float _steeringAngle;
    [SerializeField] private float _steeringDamp;

    public float maxSteeringAngle;

    // Singleton
    public static CarController instance;


    public List<AxleInfo> axleInfos = new List<AxleInfo>();


    public float horizontalInput;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        horizontalInput = Input.GetAxis("Horizontal");
#endif
        _steeringAngle = (maxSteeringAngle * horizontalInput) * _steeringDamp;

        foreach (AxleInfo info in axleInfos)
        {
            if (info.steer)
            {
                foreach (Wheel wheel in info.wheels)
                {
                    wheel.GetWheelCollider().steerAngle = _steeringAngle;
                }
            }
        }
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
