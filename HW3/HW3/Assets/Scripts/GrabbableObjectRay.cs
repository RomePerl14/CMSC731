using UnityEngine;

public class GrabbableObjectRay : MonoBehaviour
{
    bool isGrabbed = false;
    UnityEngine.Vector3 pointDirection = new UnityEngine.Vector3(0,0,1);
    float capture_dist;
    bool first_time = true;
    int count = 0;
    float move_distance_stored;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public float Grab(UnityEngine.Vector3 hand_position, UnityEngine.Quaternion hand_orientation, float move_distance)
    {
        if(first_time == true)
        {
            capture_dist = (this.transform.position - hand_position).magnitude;
            move_distance_stored = 0f;
            first_time = false;
        }
        isGrabbed = true;
        move_distance_stored += move_distance/10;
        float distance = capture_dist - move_distance_stored; // if this doesn't work, get distance from ray hit!
        if(distance < 0.15f)
        {
            distance = 0.15f;
        }
        this.transform.position = hand_position + (hand_orientation*(pointDirection*distance));
        this.transform.rotation = hand_orientation;
        count = 0;
        return move_distance_stored;
    }

    // Update is called once per frame
    void Update()
    {
        if(isGrabbed == true && count == 5)
        {
            isGrabbed = false;
            first_time = true;
            count = 0;
        }
        else
        {
            count += 1;
        }

        if(count > 5)
        {
            count = 5;
        }
    }
}
