using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class FPSCounter : MonoBehaviour
{

    private int lastFrameIndex;
    private float[] frameDeltaTimeArray;

    private TextMeshProUGUI fpsText;

    // Start is called before the first frame update
    void Awake()
    {
        frameDeltaTimeArray = new float[50];
    }

    private void Start()
    {
        fpsText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

        fpsText.text = "FPS: " + Mathf.RoundToInt(CalculateFPS()).ToString();
    }

    private float CalculateFPS()
    {
        float total = 0f;

        foreach (float deltaTime in frameDeltaTimeArray)
        {
            total += deltaTime;
        }

        return frameDeltaTimeArray.Length / total;
    }
}
