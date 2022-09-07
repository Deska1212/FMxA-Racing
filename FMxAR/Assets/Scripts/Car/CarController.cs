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
    private float _steeringAngle;
    private float _steeringDamp;

    // Singleton
    public static CarController instance;


    public List<AxleInfo> axleInfos = new List<AxleInfo>();

    public AnimationCurve curve;
    public WheelFrictionCurve friction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

/// <summary>
/// Serializable struct that holds information on the cars wheels
/// </summary>
[System.Serializable]
public struct AxleInfo
{
    public bool motor;
    public bool steer;
    public List<Wheel> wheels;
}
