using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used Vector3.SmoothDamp to follow the car at a smooth velocity. Also responsible for pointing the camera in the direction of a car. Controls offset values.
/// </summary>
public class CarCamera : MonoBehaviour
{
    /// <summary>
    /// Target to follow.
    /// </summary>
    public Transform target; 

    /// <summary>
    /// Camera Boom.
    /// </summary>
    public Vector3 offset;
    private Vector3 vel;

    /// <summary>
    /// How smooth the camera changes velocity.
    /// </summary>
    [Range(1f, 500f)] public float positionSmooth;

    // I have to put this in fixed update so it is smooth and doesn't jitter.
    void FixedUpdate()
    {
        Vector3 localOff = target.right * offset.x + target.up * offset.y + target.forward * offset.z;  // Convert the offset to a local position and store it in a vector
        Vector3 pos = target.transform.position + localOff;                                             // Add the offset to desired camera position
        Vector3 dirToTarget = target.position - transform.position;                                     // Get a vector pointing from the camera to the car

        // Smooth damp the position of the camera to the desired pos
        transform.position = Vector3.SmoothDamp(transform.position, pos, ref vel, positionSmooth * Time.deltaTime);

        // Point the camera along the vector going from the camera to the car. Using Quaternion.LookRotation instead of transform.LookAt as it provides
        // more customizability down the line if I so desire
        transform.rotation = Quaternion.LookRotation(dirToTarget); 
    }
}
