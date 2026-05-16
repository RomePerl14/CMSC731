using UnityEngine;

public class XRHandInteract : MonoBehaviour
{
    Rigidbody rb;

    public Transform transform_mimic;

    // UnityEngine.Vector3 previous_pos;
    // UnityEngine.Vector3 previous_vel = new UnityEngine.Vector3(0,0,0);
    // UnityEngine.Vector3 current_pos;
    // UnityEngine.Vector3 current_vel = new UnityEngine.Vector3(0,0,0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        // previous_pos = current_pos;
        // TOOD = check if I messed something up here....
        rb.MovePosition(transform_mimic.position);
        rb.MoveRotation(transform_mimic.rotation);
    }

    // UnityEngine.Vector3 LinearDerivator(UnityEngine.Vector3 curr, UnityEngine.Vector3 prev)
    // {
    //     return (curr - prev)/Time.fixedDeltaTime;
    // }

    // UnityEngine.Quaternion RotationalDerivator(UnityEngine.Vector3 curr, UnityEngine.Vector3 prev)
    // {

    // }
    // Update is called once per frame
    void FixedUpdate()
    {
    //     current_pos = transform_mimic.position;
    //     current_vel = LinearDerivator(current_pos, previous_pos);
    //     UnityEngine.Vector3 accel = LinearDerivator(current_vel, previous_vel);

    //     rb.linearVelocity = current_vel;

    //     previous_pos = current_pos;
    //     previous_vel = current_vel;
        rb.MovePosition(transform_mimic.position);
        rb.MoveRotation(transform_mimic.rotation);
    }
}
