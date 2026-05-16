using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SslHand : MonoBehaviour
{

    public float animation_speed = 10;

    Animator animator;

    float grip_value = 0;
    float grip_target = 0;
    float trigger_value = 0;
    float trigger_target = 0;

    string grip_string = "Grip";
    string trigger_string = "Trigger";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        SslAnimateHand();
    }

    public void SetGrip(float value)
    {
        grip_target = value;
    }

    public void SetTrigger(float value)
    {
        trigger_target = value;
    }

    void SslAnimateHand()
    {
        if(grip_value != grip_target)
        {
            // grip_value = Mathf.MoveTowards(grip_value, grip_target, Time.deltaTime*animation_speed);
            grip_value = grip_target;
            animator.SetFloat(grip_string, grip_value);
        }
        if(trigger_value != trigger_target)
        {
            // trigger_value = Mathf.MoveTowards(trigger_value, trigger_target, Time.deltaTime*animation_speed);
            trigger_value = trigger_target;
            animator.SetFloat(trigger_string, trigger_value);
        }
    }
}
