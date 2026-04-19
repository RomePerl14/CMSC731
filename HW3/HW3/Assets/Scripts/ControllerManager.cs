using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;


public class ControllerManager : MonoBehaviour
{

    public enum WhichHand
    {
        Left,
        Right,
    };

    public WhichHand whichHand;
    List<UnityEngine.XR.InputDevice> handDevices = new List<UnityEngine.XR.InputDevice>();

    private UnityEngine.XR.InputDevice currentDevice;
    private bool controller_connected = false;

    private GameObject selectedObject = null;
    private GameObject grabbedObject = null;
    private UnityEngine.Vector3 pointingDirection = new Vector3(0,0,1);
    LineRenderer lineRenderer;

    float grab_radius = 1; // 1 meter grab radius for now


    void GetController()
    {
        var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        switch(whichHand)
        {
            case WhichHand.Left:
                desiredCharacteristics = desiredCharacteristics | UnityEngine.XR.InputDeviceCharacteristics.Left;
                UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, handDevices);

                while(handDevices.Count != 1)
                {
                    foreach (var device in handDevices)
                    {
                        Debug.Log(string.Format("Device name '{0}' has characteristics '{1}'", device.name, device.characteristics.ToString()));
                    }
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
                    Debug.LogError("Uhhhhh there are is more than one right hand controller!");
                    break;
                case WhichHand.Right:
                    Debug.LogError("Uhhhhh there are is more than one right hand controller!");
                    break;
            }
        }
    }

    UnityEngine.Vector3 GetPointingDirection(float distance)
    {
        return this.transform.rotation * (this.pointingDirection*distance);
    }

    UnityEngine.Vector3 GetPosition()
    {
        return this.transform.position;
    }

    bool GetButtonPress()
    {
        bool buttonValue;
        return (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonValue) && buttonValue);
    }

    void CastRay()
    {
        RaycastHit hit;
        float distance;
        if(Physics.Raycast(this.GetPosition(), this.GetPointingDirection(1), out hit, Mathf.Infinity) && hit.collider.gameObject.GetComponent<SelectableObject>() != null) // TODO: ADD LAYER MASK
        {
            // Set the color
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            distance = hit.distance;
            
            // highlight the box too
            selectedObject = hit.collider.gameObject;
            selectedObject.GetComponent<SelectableObject>().Highlight();
        }
        else
        {
            // Set the color
            lineRenderer.startColor = Color.blue;
            lineRenderer.endColor = Color.blue;
            distance = 1000f;
            selectedObject = null;
        }
        lineRenderer.SetPosition(0, this.GetPosition());
        lineRenderer.SetPosition(1, this.GetPosition()+this.GetPointingDirection(distance));
    }

    bool GetTriggerPress()
    {
        bool triggerValue;
        return (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue);
    }

    Vector2 GetJoystickValue()
    {
        Vector2 axes;
        if (currentDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out axes))
        {
            return axes;
        }
        return new Vector2(0,0);
    }

    void GrabObject()
    {
        UnityEngine.Vector3 curr_pos = this.GetPosition();
        UnityEngine.Quaternion curr_rot = this.transform.rotation;
        Collider[] hitColliders = Physics.OverlapSphere(curr_pos, this.grab_radius);
        float closest_obj = grab_radius;
        if(hitColliders.Length > 0) // only look if we actually got hits
        {
            foreach (var hitCollider in hitColliders)
            {
                if(hitCollider.gameObject.GetComponent<GrabbableObject>() != null)
                {
                    // get the closest game object
                    float dist = (hitCollider.gameObject.transform.position - curr_pos).magnitude;
                    if(dist <= closest_obj)
                    {
                        grabbedObject = hitCollider.gameObject;
                        closest_obj = dist;
                    }
                }
                
            }
        }
        if(grabbedObject != null)
        {
            grabbedObject.GetComponent<GrabbableObject>().Grab(curr_pos, curr_rot);
        }
    
    }

    void GrabObjectRay()
    {
        // if we've selected an object and 
        if(selectedObject != null && selectedObject.GetComponent<GrabbableObjectRay>() != null)
        {
            // if we've pressed the trigger
            if(GetTriggerPress())
            {
                selectedObject.GetComponent<GrabbableObjectRay>().Grab(GetPosition(),this.transform.rotation, GetJoystickValue()[0]);
            }
        }
    }

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        // Set the width
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        // Set the material
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        // Set the number of vertices
        lineRenderer.positionCount = 2;
        // Set the color
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;

        // Get the controller
        GetController();
    }

    // Update is called once per frame
    void Update()
    {
        if(controller_connected != true)
        {
            GetController();
        }
        else
        {
            lineRenderer.enabled = false;
            if(this.GetButtonPress())
            {
                lineRenderer.enabled = true;
                CastRay();
                GrabObjectRay();
            }
            else if(selectedObject != null)
            {
                selectedObject = null;
            }

            if(this.GetTriggerPress())
            {
                this.GrabObject();
            }
            else
            {
                grabbedObject = null;
            }
        }
    }
}
