using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics;
using TMPro;


namespace SSL
{

public enum WorkspaceState
{
    exos,
    dyma,
    freeflying
}

public class ExosDirectTeleopManager : MonoBehaviour
{

    // I want to be able to reset the scene by pressing a specific button, hide my hands when I'm driving,
    // and require a deadmans switch to actually control the system
    //  |-> want trigger to be deadmans switch, so you have to hold trigger to drive robot, but exoskeleton can still move
    // I also want to be able to switch between Exoskeleton view, Dyma view, and free-view
    //  |-> lets have the primary button do this, and we just cycle between modes. Lets dim the screen time permitting
    // I also need to control the end-effector here
    // we can use "write tool drive velocities" and the thumbstick to control it

    public DymaJointPositionController pos_controller;
    public Transform dyma_spot;
    public Transform free_flying_spot;
    public GameObject XROrigin;
    public GameObject Locomotion;
    WorkspaceState ws_state = WorkspaceState.freeflying; // start in free flying mode
    public GameObject right_hand;
    public GameObject robot;
    public SslHandController hand_controller_right;
    public SslHandController hand_controller_left;
    SSL.SSLArticulationBody.ArticulationBodyStateWriter writer; // write to the robot

    public TextMeshPro title_text;
    public TextMeshPro control_state;
    public TextMeshPro tool_drive_state;

    public SSL.RobotHomePositions robot_home;
    public SSL.RobotNames robot_name = SSL.RobotNames.none;

    bool pressed_primary_button = false;
    bool trigger_pressed = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        title_text.text = "Investigate Mode";
        control_state.text = "Trigger Unpressed, Motion Disabled";
        tool_drive_state.text = "";
        if(robot == null)
        {
            UnityEngine.Debug.LogError("[VR Trajectory Manager] Did not specify robot!");
        }
        writer = new SSL.SSLArticulationBody.ArticulationBodyStateWriter(robot);

        if(hand_controller_right == null)
        {
            UnityEngine.Debug.LogError("[VR Trajectory Manager] Did not specify right hand!");
        }
        if(hand_controller_left == null)
        {
            UnityEngine.Debug.LogError("[VR Trajectory Manager] Did not specify left hand!");
        }
        
        // Check robot name
        if(robot_name == SSL.RobotNames.none)
        {
            UnityEngine.Debug.LogError("You must provide a robot name!");
            return;
        }
        else
        {
            UnityEngine.Debug.Log("[VR Robot Interaction] Robot name: " + robot_name.ToString() + "\n");
        }


    }

    // Update is called once per frame
    void Update()
    {
        if(hand_controller_right.GetPrimaryButtonPress() == true && pressed_primary_button == false)
        {
            pressed_primary_button = true;
            switch(ws_state)
            {
                case WorkspaceState.freeflying:
                    ws_state = WorkspaceState.dyma;
                    // Now, teleport the user
                    // XROrigin.transform.position = dyma_spot.position;
                    // XROrigin.transform.rotation = dyma_spot.rotation;
                    Locomotion.SetActive(false);
                    right_hand.SetActive(false);
                    title_text.text = "In Control";
                    break;
                case WorkspaceState.dyma:
                    ws_state = WorkspaceState.freeflying;
                    XROrigin.transform.position = free_flying_spot.position;
                    XROrigin.transform.rotation = free_flying_spot.rotation;
                    Locomotion.SetActive(true);
                    right_hand.SetActive(true);
                    title_text.text = "Investigate Mode";
                    break;
            }
        }
        else if(hand_controller_right.GetPrimaryButtonPress() == false && pressed_primary_button == true)
        {
            pressed_primary_button = false;
        }

        // When we're in exos control mode
        if(ws_state == WorkspaceState.dyma)
        {
            if(hand_controller_right.GetTriggerPress() == true && trigger_pressed == false)
            {
                control_state.text = "Trigger Pressed, Motion Enabled";
                trigger_pressed = true;
                pos_controller.Halt(false);
            }
            else if(hand_controller_right.GetTriggerPress() == false && trigger_pressed == true)
            {
                control_state.text = "Trigger Unpressed, Motion Disabled";
                trigger_pressed = false;
                pos_controller.Halt(true);
            }
            
            if(trigger_pressed == true)
            {
                if(hand_controller_right.GetGripPress())
                {
                    // open and close the drive
                    // UnityEngine.Vector2 axis = hand_controller_right.GetPrimaryJoystickValue();
                    // float[] drive_positions = {axis[1], -axis[1]};
                    writer.OpenToolDrives();
                    tool_drive_state.text = "Tool Drive Opened";
                }
                else
                {
                    writer.CloseToolDrives();
                    tool_drive_state.text = "Tool Drive Closed";
                }
            }
            
        }
    }
}

};
