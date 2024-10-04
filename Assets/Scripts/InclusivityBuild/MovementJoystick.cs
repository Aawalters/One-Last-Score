using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementJoystick : MonoBehaviour
{
    public GameObject joystick;
    public GameObject joystickBackground;
    public Vector2 joystickVec;
    private Vector2 joystickTouchPos;
    private Vector2 joystickOriginalPos;
    private float joystickRadius;
    // Start is called before the first frame update
    public void Start()
    {
        joystickOriginalPos = joystickBackground.transform.position;
        joystickRadius = joystickBackground.GetComponent<RectTransform>().sizeDelta.y / 4;
    }

    public void PointerDown()
    {
        //joystick.transform.position = Input.mousePosition;
        //joystickBackground.transform.position = Input.mousePosition;
        //joystickTouchPos = Input.mousePosition;

        joystick.transform.position = joystickOriginalPos;
        joystickBackground.transform.position = joystickOriginalPos;
        joystickTouchPos = joystickOriginalPos;
    }

    public void Drag(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;

        Vector2 dragPos = pointerEventData.position;

        joystickVec = (dragPos - joystickTouchPos).normalized;

        float joystickDist = Vector2.Distance(dragPos, joystickTouchPos);

        if (joystickDist < joystickRadius)
        {
            joystick.transform.position = joystickTouchPos + joystickVec * joystickDist;
        }
        else
        {
            joystick.transform.position = joystickTouchPos + joystickVec * joystickRadius;
        }
    }

    public void PointerUp()
    {
        joystickVec = Vector2.zero;
        joystick.transform.position = joystickOriginalPos;
        joystickBackground.transform.position = joystickOriginalPos;
    }
}
