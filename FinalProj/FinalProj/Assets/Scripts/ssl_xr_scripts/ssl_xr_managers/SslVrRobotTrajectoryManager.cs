using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics;
using TMPro;

namespace SSL
{

struct Waypoint
{
    public float[] positions;
    public float time;
    public float accel;
}

public enum WaypointState
{
    IDLE,
    START,
    SAVED,
    TIME,
    ACCEL,
    EXIT,
}

public class SslVrRobotTrajectoryManager : MonoBehaviour
{
    public GameObject robot;
    public SslHandController hand_controller_right;
    public SslHandController hand_controller_left;
    public TextMeshPro text_mesh;

    private List<Waypoint> waypoints = new List<Waypoint>();
    private Waypoint current_waypoint;
    WaypointState wp_state;    
    int waypoint_counter = 0;

    public SSL.RobotHomePositions robot_home;
    public SSL.RobotNames robot_name = SSL.RobotNames.none;

    SSL.SSLArticulationBody.ArticulationBodyStateWriter writer; // write to the robot
    SSL.SSLArticulationBody.ArticulationBodyStateReader reader; // read from the robot
    float[] write_positions;

    private bool primary_button_pressed = false;
    private bool secondary_button_pressed = false;
    private bool both_buttons_pressed = false;
    bool executing_traj =false;

    TrapezoidalMotionProfile motion = new TrapezoidalMotionProfile();
    float starting_time;
    float previous_time;
    float[] current_positions;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        writer = new SSL.SSLArticulationBody.ArticulationBodyStateWriter(robot);
        reader = new SSL.SSLArticulationBody.ArticulationBodyStateReader(robot);
        if(robot == null)
        {
            UnityEngine.Debug.LogError("[VR Trajectory Manager] Did not specify robot!");
        }
        if(hand_controller_right == null)
        {
            UnityEngine.Debug.LogError("[VR Trajectory Manager] Did not specify hand!");
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

        robot_home = new SSL.RobotHomePositions();

        current_waypoint.positions = new float[reader.numJoints]; // set the size for our waypoints going forward
        write_positions = new float[reader.numJoints]; // set the size for our waypoints going forward
        current_positions = new float[reader.numJoints];
        current_waypoint.time = 3f; // default to 3 seconds
        current_waypoint.accel = 1f; /// default to 1 m/s/s
        writer.SetJointPositions(robot_home.GetHomePositionsForRobot(robot_name));
        writer.SetControlMode(SSL.ControlModes.VR_CONTROL);

    }

    // Update is called once per frame
    void Update()
    {
        if(executing_traj == false)
        {
            // construct the state machine
            // capture if we want to store the position
            if(hand_controller_right.GetPrimaryButtonPress() == true && primary_button_pressed == false)
            {
                primary_button_pressed = true;
                switch(wp_state)
                {
                    case WaypointState.IDLE:
                        wp_state = WaypointState.START;
                        text_mesh.text = "IDLE";
                        break;
                    case WaypointState.START:
                        wp_state = WaypointState.SAVED;
                        // text_mesh.text = "SAVED\nWAYPOINT";
                        break;
                    case WaypointState.SAVED:
                        wp_state = WaypointState.TIME;
                        // text_mesh.text = "SAVED\nTIME";

                        break;
                    case WaypointState.TIME:
                        wp_state = WaypointState.ACCEL;
                        // text_mesh.text = "SAVED\nACCEL";

                        break;
                    case WaypointState.EXIT:
                        wp_state = WaypointState.IDLE;
                        // text_mesh.text = "WAYPOINT\nADDED";

                        break;
                }
            }
            else if(hand_controller_right.GetPrimaryButtonPress() == false && primary_button_pressed == true)
            {
                primary_button_pressed = false;
            }

            // // delete a waypoint if we press the secondary button
            // if(hand_controller_left.GetSecondaryButtonPress() == true || secondary_button_pressed == false)
            // {
            //     secondary_button_pressed = true;
            //     if(waypoints.Count > 0)
            //     {
            //         waypoints.RemoveAt(waypoints.Count - 1); // remove it
            //     }
            // }
            // else if(hand_controller_left.GetSecondaryButtonPress() == false || secondary_button_pressed == true)
            // {
            //     secondary_button_pressed = false;
            // }


            switch(wp_state)
            {
                case WaypointState.IDLE:
                        // wp_state = WaypointState.SAVED;
                        text_mesh.text = "Press A To\nStart Waypoint";
                        // do nothing
                        current_waypoint = new Waypoint();
                        current_waypoint.positions = new float[reader.numJoints]; // set the size for our waypoints going forward
                        current_waypoint.accel = 1f;
                        current_waypoint.time = 3f;
                        break;
                case WaypointState.START:
                    current_waypoint.positions = reader.GetJointPositionsAsFloats();
                    text_mesh.text = "SAVED JOINT\nPOSITIONS";
                    // update UI element
                    break;
                case WaypointState.SAVED:
                    UnityEngine.Vector2 time_val = hand_controller_right.GetPrimaryJoystickValue();
                    current_waypoint.time += time_val[1]*0.1f;
                    text_mesh.text = "Traj Time:\n" + current_waypoint.time.ToString();
                    break;
                case WaypointState.TIME:
                    UnityEngine.Vector2 accel_val = hand_controller_right.GetPrimaryJoystickValue();
                    current_waypoint.accel += accel_val[1]*0.1f;
                    text_mesh.text = "Traj Accel:\n " + current_waypoint.accel.ToString();
                    break;
                case WaypointState.ACCEL:
                    waypoints.Add(current_waypoint);
                    text_mesh.text = "Added\nWaypoint!";

                    wp_state = WaypointState.EXIT; // immediately switch states
                    break;
            }

            if(hand_controller_left.GetPrimaryButtonPress() == true && both_buttons_pressed == false)
            {
                // execute trajectory
                if(waypoints.Count > 0)
                {

                    both_buttons_pressed = true;
                    executing_traj = true;
                    starting_time = Time.fixedTime;
                    previous_time = Time.fixedTime;
                    motion.desired_acceleration = waypoints[waypoint_counter].accel;
                    motion.trajectory_time = waypoints[waypoint_counter].time;
                    current_positions = reader.GetJointPositionsAsFloats();
                }
            }
            else if(hand_controller_left.GetPrimaryButtonPress() == false && both_buttons_pressed == true)
            {
                both_buttons_pressed = false;
            }
        }
        else
        {
            float timeSince = Time.fixedTime - starting_time;
            float delta = Time.fixedTime - previous_time;
            previous_time = Time.fixedTime;
            
            for(int i=0;i<reader.numJoints;i+=1)
            {
                // motion.desired_position = waypoints[waypoint_counter].positions[i];
                // write_positions[i] = motion.GenerateTrapezoid(delta);
                write_positions[i] += (-current_positions[i]+waypoints[waypoint_counter].positions[i])/waypoints[waypoint_counter].time * delta;

            }
            UnityEngine.Debug.Log("Time: " + timeSince.ToString());
            writer.WriteJointPositions(write_positions);
            writer.SetControlMode(SSL.ControlModes.POSITION_CONTROL);
            if(timeSince >= motion.trajectory_time)
            {
                waypoint_counter += 1;
                if(waypoint_counter > waypoints.Count-1)
                {
                    executing_traj = false; // we're done
                    waypoint_counter = 0;
                    writer.SetControlMode(SSL.ControlModes.VR_CONTROL);
                    waypoints = new List<Waypoint>();
                    return;
                }
                motion.desired_acceleration = waypoints[waypoint_counter].accel;
                motion.trajectory_time = waypoints[waypoint_counter].time;
                starting_time = Time.fixedTime;
                current_positions = reader.GetJointPositionsAsFloats();
            }


            
        }

        // we should be constantly updating the UI element here

        // execute trajectory

        
        // when I press another button, I want to replay the trajectory from whereever the robot currently is

        // I want to specify how long it will take the robot to get there... might need to come back to this!
        
    }
}
}
