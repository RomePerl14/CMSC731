using UnityEngine;

public class GrabbableObject : MonoBehaviour
{

    bool isGrabbed = false;
    public enum GrabCondition
    {
        Offset,
        GrabPoint,
    };

    public GrabCondition objectGrabCondition = GrabCondition.Offset;
    UnityEngine.Vector3 hand_offset = new UnityEngine.Vector3(0,0,0.15f);
    UnityEngine.Vector3 captured_grab_point;
    UnityEngine.Quaternion captured_grab_rot;
    bool initial_pos_captured = false;
    int count = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Grab(UnityEngine.Vector3 hand_position, UnityEngine.Quaternion hand_orientation)
    {
        isGrabbed = true;
        count = 0;
        switch(objectGrabCondition)
        {
            case GrabCondition.Offset:
                this.transform.position = hand_position + (hand_orientation*hand_offset);
                this.transform.rotation = hand_orientation;
                break;
            case GrabCondition.GrabPoint:
                objectGrabCondition = GrabCondition.Offset;
                // if(initial_pos_captured == false)
                // {
                //     captured_grab_point = this.transform.position;
                //     captured_grab_rot = this.transform.rotation;
                //     initial_pos_captured = true;
                // }
                // this.transform.rotation = hand_orientation*captured_grab_rot;
                // this.transform.position = hand_position + (this.transform.rotation*(captured_grab_point - hand_position));
                break;
        }
        
    }

    void Update()
    {
        if(isGrabbed == true && count == 10)
        {
            isGrabbed = false;
            initial_pos_captured = false;
            count = 0;
        }
        else
        {
            count += 1;
        }

        if(count > 10)
        {
            count = 10;
        }
    }
}
