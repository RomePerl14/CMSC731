// ||                            ||
// ||       Romeo Perlstein      ||
// ||  CMSC731 - Advances in XR  ||
// ||  HW1: Roll-a-ball Tutorial ||
// ||                            ||

using UnityEngine;

public class CameraController : MonoBehaviour
{
    // public variables
    public GameObject player; // The game object that has the player controller
    
    // private variables
    private UnityEngine.Vector3 offset; // a static offset to apply to the camera, uses the current offset in the scene window
    
    void Start()
    {
        // Set the offset to the current distance of the camera to the player, in global coordinates
        this.offset = this.transform.position - player.transform.position;
    }


    void LateUpdate()
    {
        // Set the camera transform to the the current player position + the birds-eye-view offset
        this.transform.position = player.transform.position + offset;
    }
    
}
