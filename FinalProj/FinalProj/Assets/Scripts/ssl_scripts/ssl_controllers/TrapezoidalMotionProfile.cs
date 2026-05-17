using UnityEngine;

public class TrapezoidalMotionProfile : MonoBehaviour
{
    public float desired_position; // the desired position
    public float desired_acceleration = 1; // the desired acceleraton
    // We can either specify trajectory time, or trajectory velocity, and I think I'm gonna do time
    public float trajectory_time = 10;

    // derived values, to be filled in Start()
    private float t_accel;
    private float v;

    void Start()
    {
        v = desired_position/trajectory_time;
        t_accel = v/desired_acceleration;
    }

    // should take in a dt value, from 0 -> trajectory_time. It is up to the
    // user to determine how the counting number is implemented
    public float GenerateTrapezoid(float t)
    {
        if(t < trajectory_time)
        {
            if(t <= t_accel)
            {
                return (desired_acceleration*t);
            }
            else if(t > trajectory_time-t_accel && t < trajectory_time)
            {
                return (v*t_accel*0.5f + v*(trajectory_time-t_accel) - desired_acceleration*t);
            }
            else
            {
                return (v*t_accel*0.5f + v*t);
            }
        }
        else
        {
            return 0f;
        }
    }
}
