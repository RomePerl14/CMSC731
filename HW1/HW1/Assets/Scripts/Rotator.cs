// ||                            ||
// ||       Romeo Perlstein      ||
// ||  CMSC731 - Advances in XR  ||
// ||  HW1: Roll-a-ball Tutorial ||
// ||                            ||

using UnityEngine;

public class Rotator : MonoBehaviour
{
    void Update()
    {
        // For all time, rotate the block!
        transform.Rotate(new UnityEngine.Vector3(15, 30, 45)*Time.deltaTime);
    }
}
