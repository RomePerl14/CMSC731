using UnityEngine;

public class GrabbableObject : MonoBehaviour
{

    bool isGrabbed = false;
    UnityEngine.Vector3 hand_offset = new UnityEngine.Vector3(0,0,0.5f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Grab(UnityEngine.Vector3 hand_position, UnityEngine.Quaternion hand_orientation)
    {
        isGrabbed = true;
        this.transform.position = hand_position + (hand_orientation*hand_offset);
        this.transform.rotation = hand_orientation;
    }

    void Update()
    {
        if(isGrabbed == true)
        {
            isGrabbed = false;
        }
    }
}
