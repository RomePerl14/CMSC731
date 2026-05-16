using UnityEngine;

public class FloatAndRotate : MonoBehaviour
{
    private UnityEngine.Vector3 starting_vec;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        starting_vec = transform.localPosition; // Get the current starting position so we don't uber auto teleport to [0 0 0]
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(new UnityEngine.Vector3(0, 45, 0)*Time.deltaTime); // rotate at a set speed
        transform.localPosition = (new UnityEngine.Vector3(0,UnityEngine.Mathf.Sin(Time.time/2)/5,0)) + starting_vec; // bob up and down
    }
}
