using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics;

namespace SSL
{

struct Waypoint
{
    float[] joint_positions;
    float time;
    float accel;
}

public class SslVrRobotTrajectoryManager : MonoBehaviour
{
    public GameObject robot;
    public SslHandController hand_controller;

    private List<Waypoint> waypoints;

    public SSL.robot_homePositions robot_home;
    public SSL.robot_names robot_name = SSL.robot_names.none;

    SSL.SSLArticulationBody.ArticulationBodyStateWriter writer; // write to the robot
    SSL.SSLArticulationBody.ArticulationBodyStateReader reader; // read from the robot
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(robot == null)
        {
            UnityEngine.Debug.LogError("[VR Trajectory Manager] Did not specify robot!");
        }
        if(hand == null)
        {
            UnityEngine.Debug.LogError("[VR Trajectory Manager] Did not specify hand!");
        }
        
        // Check robot name
        if(robot_name == SSL.robot_names.none)
        {
            UnityEngine.Debug.LogError("You must provide a robot name!");
            return;
        }
        else
        {
            UnityEngine.Debug.Log("[VR Robot Interaction] Robot name: " + robot_name.ToString() + "\n");
        }

        robot_home = new SSL.robot_homePositions();
        // writer.setJointPositions(robot_home.GetHomePositionsForRobot(robot_name));
    }

    // Update is called once per frame
    void Update()
    {
        // when I press a button, I want to store a waypoint

        // when I press another button, I want to replay the trajectory from whereever the robot currently is

        // I want to specify how long it will take the robot to get there... might need to come back to this!
        
    }
}
}
