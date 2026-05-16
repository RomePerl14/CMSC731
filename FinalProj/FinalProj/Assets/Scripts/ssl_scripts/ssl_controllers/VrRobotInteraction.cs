using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics;

public class VrRobotInteraction : MonoBehaviour
{
    SSL.SSLArticulationBody.ArticulationBodyStateWriter writer;
    public GameObject robot;
    public SSL.RobotHomePositions robotHome;
    public SSL.RobotNames robotName = SSL.RobotNames.none;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        if(robot == null)
        {
            UnityEngine.Debug.LogError("You must provide a robot gameobject!");
        }
        writer = new SSL.SSLArticulationBody.ArticulationBodyStateWriter(robot);
        writer.setControlMode(SSL.ControlModes.VR_CONTROL);
        if(robotName == SSL.RobotNames.none)
        {
            UnityEngine.Debug.LogError("You must provide a robot name!");
            return;
        }
        else
        {
            UnityEngine.Debug.Log("[VR Robot Interaction] Robot name: " + robotName.ToString() + "\n");
        }

        robotHome = new SSL.RobotHomePositions();
        writer.setJointPositions(robotHome.GetHomePositionsForRobot(robotName));

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
