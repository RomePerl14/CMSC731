using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;


public class PenguFly : MonoBehaviour
{


    // From HW 3

    // From HW3
    private UnityEngine.XR.InputDevice R_dev;
    private UnityEngine.XR.InputDevice L_dev;
    private bool controller_connected_L = false;
    private bool controller_connected_R = false;
    UnityEngine.Vector3 R;
    UnityEngine.Vector3 L;
    UnityEngine.Vector3 H;

    UnityEngine.Vector3 R_prime;
    UnityEngine.Vector3 L_prime;
    UnityEngine.Vector3 H_prime;

    public Transform RTransform;
    public Transform LTransform;
    public Transform HeadTransform;

    bool calibratedL = false;
    bool calibratedR = false;
    bool setup_complete = false;

    // XR Origin transform
    public Transform xrTransform;

    public GameObject notification;

    int counter = 0;

    // PenguFly stuff
    float xmax = 0;
    public float vmax = 50f;
    UnityEngine.Vector3 dir;
    float vel = 0;
    public float movement_threshold = 0.05f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        notification.SetActive(false);
        GetControllers();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if((controller_connected_L && controller_connected_R) != true)
        {
            GetControllers();
            return; // return until we get the dang controller!
        }

        if((calibratedL && calibratedR) != true)
        {
            calibratedL = GetPrimaryButtonPressL();
            calibratedR = GetPrimaryButtonPressR();
            R = RTransform.position;
            L = LTransform.position;
            H = HeadTransform.position;
            return;
        }
        else if(setup_complete != true)
        {
            setup_complete = true;
            // get xmax;
            xmax = Mathf.Max((H-L).magnitude, (H-R).magnitude);
            notification.SetActive(true);
            return;
        }

        if(setup_complete && counter < 100)
        {
            counter += 1;
            return;
        }
        else
        {
            notification.SetActive(false);
        }

        // Now, do stuff
        computePengu();
        xrTransform.position = xrTransform.position + (vel*Time.fixedDeltaTime)*dir;
    }

    void computePengu()
    {
        // get Head_prime, R_prime, L_prime
        R_prime = GetHandPositionR();
        L_prime = GetHandPositionL();
        H_prime = HeadTransform.position;
        R_prime[1] = 0; // set the height to 0
        L_prime[1] = 0; // set the height to 0
        H_prime[1] = 0; // set the height to 0


        // max velocity
        vmax += GetPrimaryJoystickValueR()[1]*1f;
        if(vmax <= 1e-4)
        {
            vmax = 0.01f;
        }
        if(vmax >= 50f)
        {
            vmax = 50f;
        }

        // direction
        dir = H_prime-(R_prime+L_prime)/2;

        if(dir.magnitude < movement_threshold)
        {
            vel = 0.0f;
            return;
        }

        // velocity
        vel = vmax*(dir.magnitude/xmax)*(dir.magnitude/xmax);
    }

    // From HW3
    bool GetControllers()
    {
        var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        List<UnityEngine.XR.InputDevice> handDevices;

        if(controller_connected_L != true)
        {
            desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Controller | UnityEngine.XR.InputDeviceCharacteristics.Left;
            handDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, handDevices);
            foreach (var device in handDevices)
            {
                Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
            }
            if(handDevices.Count == 1)
            {
                controller_connected_L = true;
                L_dev = handDevices[0];
                Debug.Log("Found Left hand controller!");
            }
            else if(handDevices.Count > 1)
            {
                Debug.LogError("Uhhhhh there are is more than one left hand controller!");
            }
        }
        
        if(controller_connected_R != true)
        {
            desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Controller | UnityEngine.XR.InputDeviceCharacteristics.Right;
            handDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, handDevices);
            foreach (var device in handDevices)
            {
                Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
            }
            if(handDevices.Count == 1)
            {
                controller_connected_R = true;
                R_dev = handDevices[0];
                Debug.Log("Found Right hand controller!");
            }
            else if(handDevices.Count > 1)
            {
                Debug.LogError("Uhhhhh there are is more than one right hand controller!");
            }
        }
        return(controller_connected_L && controller_connected_R);
    }

    // From HW3
    Vector2 GetPrimaryJoystickValueR()
    {
        Vector2 axes;
        if (R_dev.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out axes))
        {
            return axes;
        }
        return new Vector2(0,0);
    }

    // From HW3
    bool GetPrimaryButtonPressR()
    {
        bool buttonValue;
        return (R_dev.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue);
    }
    bool GetPrimaryButtonPressL()
    {
        bool buttonValue;
        return (L_dev.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue);
    }

    UnityEngine.Vector3 GetHandPositionR()
    {
        return this.RTransform.position;
    }
    UnityEngine.Vector3 GetHandPositionL()
    {
        return this.LTransform.position;
    }
}
