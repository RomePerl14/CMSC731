using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Jointstate = RosMessageTypes.Sensor.JointStateMsg;
using SSL;

public class GetExosJointStates : MonoBehaviour
{
    SSL.SSLArticulationBody.ArticulationBodyStateWriter writer;
    float[] joint_pos = new float[8];
    private float[] exos_home = {-0.523599f, 0.0f, -1.8326f, -1.5708f, 1.5708f, 1.5708f,  1.5708f, 0.0f};
    public GameObject robot;

    // Start is called before the first frame update
    void Start()
    {
        writer = new SSL.SSLArticulationBody.ArticulationBodyStateWriter(robot);
        writer.SetControlMode(SSL.ControlModes.POSITION_CONTROL);
        writer.SetJointPositions(exos_home);
        writer.ResetJointLimits();
        ROSConnection.GetOrCreateInstance().Subscribe<Jointstate>("exos/joint_states", UpdatePosition);
    }

    void UpdatePosition(Jointstate joint_state)
    {
        for(int i=0; i<joint_state.position.Length; i+=1)
        {
            if(joint_state.name[i] == "joint1")
            {
                joint_pos[0] = (float)joint_state.position[i];
            }
            if(joint_state.name[i] == "joint2")
            {
                joint_pos[1] = (float)joint_state.position[i];
            }
            if(joint_state.name[i] == "joint3")
            {
                joint_pos[2] = (float)joint_state.position[i];
            }
            if(joint_state.name[i] == "joint4")
            {
                joint_pos[3] = (float)joint_state.position[i];
            }
            if(joint_state.name[i] == "joint5")
            {
                joint_pos[4] = (float)joint_state.position[i];
            }
            if(joint_state.name[i] == "joint6")
            {
                joint_pos[5] = (float)joint_state.position[i];
            }
            if(joint_state.name[i] == "joint7")
            {
                joint_pos[6] = (float)joint_state.position[i];
            }
            if(joint_state.name[i] == "joint8")
            {
                joint_pos[7] = (float)joint_state.position[i];
            }
        }
        writer.WriteJointPositions(joint_pos);
    }
}
