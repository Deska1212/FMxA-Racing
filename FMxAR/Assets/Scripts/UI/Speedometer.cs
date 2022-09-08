using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    public float startingZRot;
    public float maxSpeedZRot;
    public float smooth;
    public float jitter;

    public RectTransform needleRectTransform;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        float desiredRot = Mathf.Lerp(startingZRot, maxSpeedZRot, CarController.instance.speedPerc);
        float vel = 0;
        if (Mathf.Approximately(needleRectTransform.rotation.z, maxSpeedZRot))
        {
            Debug.Log("Jitter");
            needleRectTransform.rotation = Quaternion.Euler(0f, 0f, needleRectTransform.rotation.z + jitter);
        }

        needleRectTransform.rotation =  Quaternion.Euler(0f, 0f, Mathf.SmoothDamp(needleRectTransform.rotation.z, desiredRot, ref vel, smooth * Time.deltaTime));
        
    }
}
