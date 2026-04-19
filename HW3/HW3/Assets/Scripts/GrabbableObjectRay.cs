using UnityEngine;

public class GrabbableObjectRay : MonoBehaviour
{
    bool isGrabbed = false;
    UnityEngine.Vector3 pointDirection = new UnityEngine.Vector3(0,0,1);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Grab(UnityEngine.Vector3 hand_position, UnityEngine.Quaternion hand_orientation, float move_distance)
    {
        isGrabbed = true;
        float distance = (this.transform.position - hand_position).magnitude - move_distance; // if this doesn't work, get distance from ray hit!
        if(distance <= 0f)
        {
            distance = 0f;
        }
        this.transform.position = hand_position + (hand_orientation*(pointDirection*distance));
        this.transform.rotation = hand_orientation;
    }

    // Update is called once per frame
    void Update()
    {
        if(isGrabbed == true)
        {
            isGrabbed = false;
        }
    }
}
