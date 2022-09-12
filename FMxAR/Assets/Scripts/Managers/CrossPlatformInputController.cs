using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossPlatformInputController : MonoBehaviour
{
    public enum InputMethod
    { 
        KEYBOARD,
        TOUCH
    }

    public static CrossPlatformInputController instance;

    public InputMethod inputMethod;

    public float horizontalInput;
    public float brakeInput;
    public float reverseInput;
    public float throttleInput;
    public bool boostInput;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        switch (inputMethod)
        { 
            case InputMethod.KEYBOARD:
                HandlePCInputs();
                break;
            case InputMethod.TOUCH:
                HandleMobileInputs();
                break;
            default:
                HandlePCInputs();
                break;
        }
        

    }

    private void HandlePCInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        throttleInput = Input.GetKey(KeyCode.W) ? 1 : 0;
        throttleInput = Input.GetKey(KeyCode.S) ? -1 : throttleInput;
        brakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
        boostInput = Input.GetKey(KeyCode.LeftShift);
    }
    private void HandleMobileInputs()
    {
        horizontalInput = SteeringWheel.instance.Value;
        throttleInput = Input.GetKey(KeyCode.W) ? 1 : 0;
        throttleInput = Input.GetKey(KeyCode.S) ? -1 : throttleInput;
        brakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
        boostInput = Input.GetKey(KeyCode.LeftShift);
    }


    public void SwitchInputMethod()
    {
        if (inputMethod == InputMethod.TOUCH)
            inputMethod = InputMethod.KEYBOARD;
        else
            inputMethod = InputMethod.TOUCH;
    }
}
