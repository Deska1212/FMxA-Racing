using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This Class used Drag and pointer handlers to sort out touch input for the steering wheel. Note: Pointer and Drag handlers work both for touch input and mouse input.
/// </summary>
public class SteeringWheel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // Singleton
    public static SteeringWheel instance;


    public float maxAngle;
    public float valueMultiplier;
    public float returnSpeed;
    public bool wheelBeingHeld;
    public float nearThreshold = 200f;

    private Vector2 _centerPoint;
    private float _wheelCurrentAngle;
    private float _wheelPrevAngle;

    private RectTransform rTransform;

    [SerializeField] private float _value;
    public float Value { get { return _value; } }

    public float Angle { get { return _wheelCurrentAngle; } }



    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        rTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        // If the wheel is not being held, return the rotation to 0
        if (!wheelBeingHeld && _wheelCurrentAngle != 0)
        {
            float deltaAngle = returnSpeed * Time.deltaTime; // Angle Per sec to return
            if (Mathf.Abs(deltaAngle) > Mathf.Abs(_wheelCurrentAngle))
            {
                // Wheel angle is too close to 0 and will overshoot if we use delta angle so just reset to 0
                _wheelCurrentAngle = 0f;
            }
            else if (_wheelCurrentAngle > 0f)
            {
                _wheelCurrentAngle -= deltaAngle;
            }
            else
            {
                _wheelCurrentAngle += deltaAngle;
            }
        }

        // Rotate the wheel image
        rTransform.localEulerAngles = new Vector3(0f, 0f, -_wheelCurrentAngle);

        _value = _wheelCurrentAngle * valueMultiplier / maxAngle; // Calculate the axis value (between -1 and 1);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        wheelBeingHeld = true;
        _centerPoint = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, rTransform.position);
        _wheelPrevAngle = Vector2.Angle(Vector2.up, eventData.position - _centerPoint);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnDrag(eventData); // Perform one last OnMove calculation just in case
        wheelBeingHeld = false;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (wheelBeingHeld)
        {
            Vector2 touch = eventData.position;

            float wheelNewAngle = Vector2.Angle(Vector2.up, touch - _centerPoint);

            // Do nothing if the touch is too close to the center of the wheel
            if ((touch - _centerPoint).sqrMagnitude >= nearThreshold) // Lets not do a sqr root calculation each frame please
            {
                if (touch.x > _centerPoint.x)
                {
                    _wheelCurrentAngle += wheelNewAngle - _wheelPrevAngle;
                }
                else
                {
                    _wheelCurrentAngle -= wheelNewAngle - _wheelPrevAngle;
                }
            }

            // Update angle
            _wheelPrevAngle = wheelNewAngle;

            // Clamp wheel
            _wheelCurrentAngle = Mathf.Clamp(_wheelCurrentAngle, -maxAngle, maxAngle);
        }

        

    }
}
