// ||                            ||
// ||  Romeo Perlstein           ||
// ||  CMSC731 - Advances in XR  ||
// ||  HW2: 3D Audio             ||
// ||                            ||

using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class MinecraftMover : MonoBehaviour
{
    // PUBLIC VARIABLES
    // Let the user set their own freakin speed
    public float speed = 1; // 1 by default

    // PRIVATE VARIABLES
    private float set_speed;
    // Camera transform on the game object
    private Transform cameraTransform;
    // The rigidbody for applying velocities to
    private Rigidbody body;

    // x displacement
    private float x;
    // y displacement
    private float y;
    // z displacement
    private float z;
    // bool to check if we are currently pressed
    private bool currently_pressed;
    // the current cursor position for the mousey
    private UnityEngine.Vector2 current_mouse_pos;
    // the current rotation of the camera along some axis that isn't actually x
    private float rotX;
    // the current rotation of the camera along some axis that also isn't y
    private float rotY;
    // the stored state of the current rotation of the camera along this "x" direction
    private float currRotX = 0;
    // the stored state of the current rotation of the camera along this "y" direction
    private float currRotY = 0;

    void Start()
    {
        this.body = GetComponent<Rigidbody>(); // Get the rigid body component on the gameobject
        this.cameraTransform = GetComponent<Transform>(); // get the transform component on the gameobject
        this.set_speed = this.speed;
    }

    void FixedUpdate()
    {
        // If we've pressed the spacebar, move up
        if(UnityEngine.InputSystem.Keyboard.current.rKey.isPressed)
        {
            this.speed = 10;
        }
        else{
            this.speed = this.set_speed;
        }
        this.OnRotateCameraMinecraft(); // rotate the camera like minecraft
        this.OnMoveUpAndDownMinecraft(); // move the player like minecraft
        this.cameraTransform.localRotation = UnityEngine.Quaternion.Euler(-this.rotY, this.rotX, 0); // rotate the camera based on the required rotations...
        UnityEngine.Vector3 vec = new UnityEngine.Vector3(this.x*this.speed, this.y*this.speed, this.z*this.speed); // create rotated vector so that our x and z commands are intrinsic
        this.body.linearVelocity = this.cameraTransform.localRotation * vec; // do the thingy
    }

    // Get wasd inputs from the player using the new Input System
    void OnMove(InputValue movementValue)
    {
        UnityEngine.Vector2 movementVector = movementValue.Get<UnityEngine.Vector2>(); // Capture a movement vector from the w/s a/d keys
        this.x = movementVector.x; // if the key is pressed, this is 1 or -1
        this.z = movementVector.y; // if the key is pressed, this is 1 or -1
    }
    
    // Rotater der cameranschnitzel
    void OnRotateCameraMinecraft()
    {
        // get the origin of the screen so we can auto-warp the mouse cursor there before measuring displacement
        UnityEngine.Vector2 origin = new UnityEngine.Vector2(Screen.width/2, Screen.height/2);
        
        // Press and hold to change the camera's orientation
        if(UnityEngine.InputSystem.Mouse.current.rightButton.isPressed)
        {
            if(this.currently_pressed == false) // if we just started pressing, do the following
            {
                // move our cursor to the origin for easy displacement calcs
                UnityEngine.InputSystem.Mouse.current.WarpCursorPosition(origin);
                this.currently_pressed = true; // tell unity we've pressed already
                this.currRotX = this.rotX; // store the current rotx
                this.currRotY = this.rotY; // store the current roty state
            }
            else
            {
                // if we've already pressed, get the current value of the mouse relative to the origin (click and drag, so when you first right click this is [0,0])
                UnityEngine.Vector2 current_delta = UnityEngine.InputSystem.Mouse.current.position.ReadValue() - origin;
                
                // Set the new rotx and roty
                this.rotX = current_delta.x + this.currRotX;
                this.rotY = current_delta.y + this.currRotY;
            }
        }
        else // if we're not pressing the right click on the mouse, we're NOT pressing!
        {
            this.currently_pressed = false;
        }

    }

    // Move like minecraft (who like minecraft)
    void OnMoveUpAndDownMinecraft()
    {
        // If we've pressed the spacebar, move up
        if(UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed)
        {
            this.y = UnityEngine.InputSystem.Keyboard.current.spaceKey.ReadValue(); // this is 1 or 0
        }
        // If we've pressed the shift key, move down
        else if(UnityEngine.InputSystem.Keyboard.current.shiftKey.isPressed)
        {
            this.y = -1*UnityEngine.InputSystem.Keyboard.current.shiftKey.ReadValue(); // this is 1 or 0
        }
        else // don't move at ALL if we haven't pressed anything
        {
            this.y=0f;
        }
    }

}
