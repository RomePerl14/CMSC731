using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;


public class Fly : MonoBehaviour
{

    // From HW3
    public enum WhichHand
    {
        Left,
        Right,
    };

    public enum MovementType
    {
        Survival,
        Creative
    };

    // From HW 3
    public WhichHand whichHand = WhichHand.Left;
    List<UnityEngine.XR.InputDevice> handDevices = new List<UnityEngine.XR.InputDevice>();

    // From HW3
    private UnityEngine.XR.InputDevice currentDevice;
    private bool controller_connected = false;

    // Pointing Direction
    public MovementType movementType = MovementType.Survival;
    UnityEngine.Vector3 normal_vector = new Vector3(0,1,0);

    // Camera
    public Camera VRCamera; // Get the camera

    // Joystick
    UnityEngine.Vector2 desired_movement;

    // XR origin;
    public Transform xrTransform;

    // Top speed
    public float topSpeed = 0.01f; // meters/second


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(VRCamera == null)
        {
            Debug.LogError("Did not add camera to component before runnning scene!!! Boo on you!");
            // Try to get a camera
            VRCamera = this.GetComponentInChildren<Camera>(); //  There should only be one camera
        }
        if(xrTransform == null)
        {
            Debug.LogError("Did not add XR Origin transform to component before runnning scene!!! Boo on you! Exiting");
            #if UNITY_EDITOR
                // This is the equivalent of clicking the Play button again to stop
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        
        GetController();
    }

    // Update is called once per frame
    void Update()
    {
        if(controller_connected != true)
        {
            GetController();
            return; // return until we get the dang controller!
        }
        // Get the joystick inputs
        //  Update the position of the player based on either the movement type
        desired_movement = GetJoystickValue()*topSpeed; // First index is forward/back, second index is left/right

        UnityEngine.Vector3 transformed_movement = VRCamera.transform.rotation * new UnityEngine.Vector3(desired_movement[0], 0, desired_movement[1]);
        if(movementType == MovementType.Survival)
        {
            // project the transformed vector onto the ground plane, the y axis
            xrTransform.position = transformed_movement - Vector3.Dot(transformed_movement,normal_vector)*normal_vector + this.xrTransform.position;
        }
        else if(movementType == MovementType.Creative)
        {
            xrTransform.position = transformed_movement + this.xrTransform.position;
        }
    }

    // From HW3
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

    // From HW3 code
    Vector2 GetJoystickValue()
    {
        Vector2 axes;
        if (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out axes))
        {
            return axes;
        }
        return new Vector2(0,0);
    }
}
