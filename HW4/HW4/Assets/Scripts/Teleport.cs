using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;


public class Teleport : MonoBehaviour
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

    // From HW3
    private UnityEngine.XR.InputDevice currentDevice;
    private bool controller_connected = false;

    public Transform handTransform;
    UnityEngine.Vector3 unitz = new UnityEngine.Vector3(0,0,1);
    UnityEngine.Vector3 unity = new UnityEngine.Vector3(0,1,0);
    UnityEngine.Vector3[] trajectory = new UnityEngine.Vector3[1000];

    // Teleport line
    LineRenderer lineRenderer;
    public float input_user = 0;

    // Joystick
    UnityEngine.Vector2 movement;
    UnityEngine.Vector3 movementFinal;

    // XR Origin transform
    public Transform xrTransform;

    public float floor_height = 0f;

    public GameObject highlight_object;
    UnityEngine.Vector3 obj_startpos;
    public GameObject highlight_object1;
    UnityEngine.Vector3 obj_startpos1;


    bool teleported = false;
    float des_rotation = 0;
    UnityEngine.Quaternion rotationoid;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        // Set the width
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        // Set the material
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        // Set the number of vertices
        lineRenderer.positionCount = 1000+1;
        // Set the color
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;

        obj_startpos = highlight_object.transform.position;
        obj_startpos1 = highlight_object1.transform.position;


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

        UnityEngine.Quaternion rot = this.GetHandOrientation();
        UnityEngine.Vector3 pos = this.GetHandPosition();
        lineRenderer.enabled = false;
        highlight_object.SetActive(false);
        highlight_object1.SetActive(false);
        if(GetTriggerPress())
        {
            UnityEngine.Vector2 outtie = ComputeProjectileMotion(rot);
            if(outtie[1] == 1)
            {
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 1000+1;
                lineRenderer.SetPosition(0, pos);
                UnityEngine.Vector3 prev_pos = pos;
                for(int i=0;i<outtie[0];i+=1)
                {
                    UnityEngine.Vector3 final_vec = rot*this.trajectory[i] + pos;

                    RaycastHit hit_out;
                    if(Physics.Linecast(prev_pos, final_vec, out hit_out))
                    {
                        lineRenderer.positionCount = i+1;
                        lineRenderer.SetPosition(i, final_vec);

                        des_rotation += GetPrimaryJoystickValue()[0];
                        rotationoid = UnityEngine.Quaternion.AngleAxis(des_rotation, hit_out.normal);
                        highlight_object.SetActive(true);
                        highlight_object1.SetActive(true);
                        highlight_object.transform.rotation = rotationoid*UnityEngine.Quaternion.FromToRotation(new UnityEngine.Vector3(0,1,0), hit_out.normal);
                        highlight_object.transform.position = highlight_object.transform.rotation*(obj_startpos) + prev_pos;
                        highlight_object1.transform.rotation = UnityEngine.Quaternion.FromToRotation(new UnityEngine.Vector3(0,1,0), hit_out.normal);
                        highlight_object1.transform.position = highlight_object1.transform.rotation*(obj_startpos1) + prev_pos;

                        prev_pos = final_vec;
                        break;
                    }
                    lineRenderer.SetPosition(i+1, final_vec);
                    prev_pos = final_vec;
                }
                if(GetPrimaryButtonPress())
                {
                    if(teleported != true)
                    {
                        teleported = true;
                        xrTransform.position = prev_pos;
                        xrTransform.rotation = highlight_object.transform.rotation;
                        des_rotation = 0f;
                    }
                }
                else
                {
                    teleported = false;
                }
            }
        }
        else
        {
            des_rotation = 0f;
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

    // From HW3
    Vector2 GetPrimaryJoystickValue()
    {
        Vector2 axes;
        if (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out axes))
        {
            return axes;
        }
        return new Vector2(0,0);
    }

    // From HW3
    bool GetPrimaryButtonPress()
    {
        bool buttonValue;
        return (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue);
    }

    bool GetTriggerPress()
    {
        bool triggerValue;
        return (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue);
    }

    UnityEngine.Quaternion GetHandOrientation()
    {
        return this.handTransform.rotation;
    }
    UnityEngine.Vector3 GetHandPosition()
    {
        return this.handTransform.position;
    }

    UnityEngine.Vector2 ComputeProjectileMotion(UnityEngine.Quaternion orientation)
    {
        input_user += GetPrimaryJoystickValue()[1]*0.1f;
        if(input_user <= 0)
        {
            input_user = 0;

            return new UnityEngine.Vector2(0,0);
        }

        UnityEngine.Vector3 v0 = (orientation * unitz); // we only care about the y and z axis for this thang hawk tuah
        // get angle from plane normal:
        float angle = Vector3.SignedAngle(v0, unity, UnityEngine.Vector3.Cross(v0, unity))*Mathf.PI/180;

        trajectory = new UnityEngine.Vector3[1000]; //  reset this array every call
        int t = 0;
        while(t < 1000)
        {
            trajectory[t][0] = 0.0f;
            trajectory[t][1] = input_user*Mathf.Cos(angle)*t/100 -(0.5f)*9.81f*((t/100f)*(t/100f));
            trajectory[t][2] = input_user*Mathf.Sin(angle)*t/100; // lets set this to 0 to start
            t += 1;
        }
        return new UnityEngine.Vector2(t,1);
    }
}
