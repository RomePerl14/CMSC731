// ||                            ||
// ||       Romeo Perlstein      ||
// ||  CMSC731 - Advances in XR  ||
// ||  HW1: Roll-a-ball Tutorial ||
// ||                            ||

using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Diagnostics;

// A class to control our player in this rolling bawl tutorial. Kinda self explanatory? JK not at all!
public class PlayerController : MonoBehaviour
{
    // Private variables
    private Rigidbody rb; // Rigidbody component on the current game object
    private int count; // The score count
    private float movementX; // A float to track our movement in the x-direction
    private float movementY; // A float to track our movement in the y-direction (but actually the unity Z-direction)
    private float movementZ; // A float to apply motion in the z-direction (but actually the unity y-direction)

    private bool jumping;

    // Public variables
    public float speed = 0; // Initialize to zero, and let it be public so we can interact with it in the game editor
    public float jumpForce = 0; // The jump force - set bu the user
    public TextMeshProUGUI countText; // The GUI element to count text values
    public GameObject winTextObj; // The text object for the win case
    public AudioClip collectionAudioClip; // audio clip for collecting elements
    public AudioClip winSound; // audio clip for winning
    public AudioClip loseSound; // audio clip for losing
    public AudioSource audioSource; // audio source

    void Start()
    {
        // Get the rigidbody component from the current gameobject and store it locally
        this.count = 0;
        this.rb = GetComponent<Rigidbody>();
        SetCountText();
        winTextObj.SetActive(false);
    }

    private void FixedUpdate()
    {
        // Create a new 3D vector based on our capture x movement and y movement
        UnityEngine.Vector3 movement = new UnityEngine.Vector3(this.movementX*this.speed, 0.0f, this.movementY*this.speed);
        this.rb.AddForce(movement); // Add the force to our current rigidbody, AND multiply it by the desired speed
        this.rb.AddForce(new UnityEngine.Vector3(0.0f, this.movementZ, 0.0f));
        // UnityEngine.Debug.Log(this.rb.GetAccumulatedForce());
        movementZ = 0.0f; // Always set the z force to 0 ever fixedupdate() call
    }

    void OnMove(InputValue movementValue)
    {
        // Capture keyboard inputs for movement
        UnityEngine.Vector2 movementVector = movementValue.Get<UnityEngine.Vector2>(); // Capture a movement vector from the w/s a/d keys
        this.movementX = movementVector.x; // save it in our x movement property
        this.movementY = movementVector.y; // save it in our y movement property
    }

    void OnJump(InputValue jumpValue)
    {
        if(jumpValue.isPressed == true) // I don't even need this
        {
            this.movementZ = jumpForce; // set the z movement to the set jump force
        }
        
    }

    // Add the OnTrigger callback to run when we bump into a trigger
    void OnTriggerEnter(Collider other)
    {
        // When you collide with an object - capture it and set it inactive
        if(other.gameObject.CompareTag("PickUp")) // check if it has the "pickup" tag
        {
            other.gameObject.SetActive(false); // if it does, make it inactive
            count += 1;
            audioSource.PlayOneShot(collectionAudioClip);
            SetCountText();
        }
        if(other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<AudioSource>().PlayOneShot(loseSound);
            Destroy(this.gameObject);
            winTextObj.gameObject.SetActive(true);
            winTextObj.GetComponent<TextMeshProUGUI>().text = "YOU LOSE! AHAHAHA";
        }
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
        if(count >= 12)
        {
            this.winTextObj.SetActive(true);
            Destroy(GameObject.FindGameObjectWithTag("Enemy"));
            audioSource.PlayOneShot(winSound);
        }
    }
}
