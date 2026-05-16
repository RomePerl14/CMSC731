using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;

public class RDW : MonoBehaviour
{
    // From HW3
    public enum WhichHand
    {
        Left,
        Right,
    };

    // From HW 3
    public WhichHand whichHand = WhichHand.Right;
    List<UnityEngine.XR.InputDevice> handDevices = new List<UnityEngine.XR.InputDevice>();
    private UnityEngine.XR.InputDevice currentDevice;
    private bool controller_connected = false;

    // points
    public Transform virtual_room_center; // position and orientation
    UnityEngine.Vector3 real_room_center; // the center of the real room
    UnityEngine.Vector3 current_location; // my current location (headset location)
    UnityEngine.Vector3 looking_direction; // my current forward direction (headset location)
    UnityEngine.Vector3 curr_to_real; // vector between headset pos and real center pos
    UnityEngine.Vector3 curr_to_virtual; // vector between headset pos and virtual center pos

    float turning_angle = 0;
    float turning_rate = 0;
    public float turning_speed = 0.01f;


    // unit vectors
    UnityEngine.Vector3 unit_z = new UnityEngine.Vector3(0,0,1);
    UnityEngine.Vector3 unit_y = new UnityEngine.Vector3(0,1,0);

    // transforms
    public Transform head_transform; // transform of the headset
    bool pressed;

    public GameObject notification;
    int counter = 0;

    // starting state
    UnityEngine.Quaternion virtual_start_rotation;
    UnityEngine.Quaternion cameraOffset_start_rotation;
    UnityEngine.Vector3 virtual_start_position;

    public GameObject marker;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetController();
        // real_room_center = marker.transform.position; //debug
        // real_room_center = head_transform.position; // get the current head position
        // marker.transform.position = real_room_center; // set the current head position to the room center
    }

    // Update is called once per frame
    void Update()
    {
        // calibrate where center of physical room is
        // after calibrating, walk to x
        // then, walk around virtual environment, and attempt to guide player towards center of room
        if(controller_connected == false)
        {
            GetController();
            return;
        }

        if(pressed == false)
        {
            if(GetPrimaryButtonPress())
            {
                pressed = true;
                // make calibration text disappear
                // store the center of the room
                real_room_center = head_transform.position; // get the current head position
                marker.transform.position = real_room_center; // set the current head position to the room center
                notification.SetActive(true);
            }
            return;
        }
        if(counter < 100)
        {
            counter += 1;
            return;
        }
        else
        {
            notification.SetActive(false);
        }

        // get our current location 
        current_location = head_transform.position;
        UnityEngine.Vector3 current_location2D = new UnityEngine.Vector3(current_location[0], 0, current_location[2]);

        // Get the current headset looking direction
        looking_direction = head_transform.rotation*unit_z; // take only our z-axis and rotate it by the orinetation of the head
        UnityEngine.Vector3 looking_direction2D = new UnityEngine.Vector3(looking_direction[0], 0, looking_direction[2]);
        
        // Get the vector that points from our current location to the real world center location
        curr_to_real = real_room_center-current_location;
        UnityEngine.Vector3 curr_to_real2D = new UnityEngine.Vector3(curr_to_real[0], 0, curr_to_real[2]);

        // Get the angle between the curr_to_real and our looking direction
        turning_angle = UnityEngine.Vector3.SignedAngle(looking_direction2D,curr_to_real2D,unit_y);
        float sign = Mathf.Sign(turning_angle);
        if((turning_angle < 5 && turning_angle > 0) || (turning_angle > -5 && turning_angle < 0))
        {
            sign = 0;
        }

        Debug.Log(sign);
        turning_rate = Mathf.Abs(turning_angle)/180 * sign;
        UnityEngine.Quaternion quat = UnityEngine.Quaternion.AngleAxis(turning_rate,unit_y);

        curr_to_virtual = virtual_room_center.position-current_location;
        UnityEngine.Vector3 curr_to_virt2D = new UnityEngine.Vector3(curr_to_virtual[0], 0, curr_to_virtual[2]);

        virtual_room_center.rotation = quat*virtual_room_center.rotation;
        UnityEngine.Vector3 final2D_position = quat*curr_to_virt2D + current_location;
        virtual_room_center.position = new UnityEngine.Vector3(final2D_position[0], virtual_room_center.position[1], final2D_position[2]);

        // Done!
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

    // From HW3
    bool GetPrimaryButtonPress()
    {
        bool buttonValue;
        return (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue);
    }
}
