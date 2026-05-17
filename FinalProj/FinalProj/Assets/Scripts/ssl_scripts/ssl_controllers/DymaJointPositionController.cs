using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Jointstate = RosMessageTypes.Sensor.JointStateMsg;
using Float64Array = RosMessageTypes.SslInterfaces.Float64ArrayMsg;
using SSL;

public class DymaJointPositionController : MonoBehaviour
{

    bool halted = true;
    SSL.SSLArticulationBody.ArticulationBodyStateWriter writer;
    float[] joint_pos = new float[7];
    public GameObject robot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        writer = new SSL.SSLArticulationBody.ArticulationBodyStateWriter(robot);
        writer.SetControlMode(SSL.ControlModes.POSITION_CONTROL);
        joint_pos[0] = 0f;
        joint_pos[1] = 1.57079f/2f;
        joint_pos[2] = 0f;
        joint_pos[3] = 1.57079f;
        joint_pos[4] = 0f;
        joint_pos[5] = 0f;
        joint_pos[6] = 0f;
        writer.SetJointPositions(joint_pos);
        writer.ResetJointLimits();
        ROSConnection.GetOrCreateInstance().Subscribe<Float64Array>("dyma/joint_positions/command", UpdateSimPosition);
    }

    public void Halt(bool value)
    {
        halted = value;
    }

    void UpdateSimPosition(Float64Array joint_state)
    {
        for(int i=0; i<joint_state.data.Length; i+=1)
        {
            joint_pos[i] = (float)joint_state.data[i];
        }
        if(halted == false)
        {
            writer.WriteJointPositions(joint_pos);
        }
    }
}
