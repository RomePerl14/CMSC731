using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;


public class SslHandController : MonoBehaviour
{

    //-------- HAND SELECTION --------//
    public enum WhichHand // enum to select which hand we are going to use
    {
        Left,
        Right,
    };
    public WhichHand whichHand; // public enum variable
    List<UnityEngine.XR.InputDevice> handDevices = new List<UnityEngine.XR.InputDevice>(); // list of the current hand devices
    private UnityEngine.XR.InputDevice currentDevice; // our actual current device
    private bool controller_connected = false; // boolean to indicate if we've connected to the controller

    public SslHand hand; // hand animation class, maybe other things?

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the controller
        GetController();
    }

    // Update is called once per frame
    void Update()
    {
        if(controller_connected != true)
        {
            GetController();
            return;
        }
        hand.SetGrip(GetGripValue());
        hand.SetTrigger(GetTriggerValue());
    }

    

    void GetController()
    {
        var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        switch(whichHand)
        {
            case WhichHand.Left:
                desiredCharacteristics = desiredCharacteristics | UnityEngine.XR.InputDeviceCharacteristics.Left;
                UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, handDevices);

                foreach (var device in handDevices)
                {
                    Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
                }
                break;
            case WhichHand.Right:
                desiredCharacteristics = desiredCharacteristics | UnityEngine.XR.InputDeviceCharacteristics.Right;
                UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, handDevices);

                foreach (var device in handDevices)
                {
                    Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
                }
                break;
        }

        if(handDevices.Count == 1)
        {
            controller_connected = true;
            currentDevice = handDevices[0];
            Debug.Log("Found hand controller!");

        }
        else if(handDevices.Count > 1)
        {
            switch(whichHand)
            {
                case WhichHand.Left:
                    Debug.LogError("Uhhhhh there are is more than one left hand controller!");
                    break;
                case WhichHand.Right:
                    Debug.LogError("Uhhhhh there are is more than one right hand controller!");
                    break;
            }
        }
    }

    public UnityEngine.Quaternion GetRotation()
    {
        return this.transform.rotation;
    }

    public UnityEngine.Vector3 GetPosition()
    {
        return this.transform.position;
    }

    public float GetTriggerValue()
    {
        float triggerValue;
        if(currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue))
        {
            return triggerValue;
        }
        return 0f;
    }

    public float GetGripValue()
    {
        float gripValue;
        if(currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out gripValue))
        {
            return gripValue;
        }
        return 0f;
    }

    public Vector2 GetPrimaryJoystickValue()
    {
        Vector2 axes;
        if (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out axes))
        {
            return axes;
        }
        return new Vector2(0,0);
    }

    public bool GetPrimaryButtonPress()
    {
        bool buttonValue;
        return (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue);
    }

    public bool GetSecondaryButtonPress()
    {
        bool buttonValue;
        return (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out buttonValue) && buttonValue);
    }

    public bool GetTriggerPress()
    {
        bool triggerValue;
        return (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue);
    }

    public bool GetGripPress()
    {
        bool gripValue;
        return (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripValue) && gripValue);
    }
}
