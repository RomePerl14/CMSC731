using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Jointstate = RosMessageTypes.Sensor.JointStateMsg;
using Float64Array = RosMessageTypes.SslInterfaces.Float64ArrayMsg;
using SSL;

public class NbvJointPositionController : MonoBehaviour
{
    SSL.SSLArticulationBody.ArticulationBodyStateWriter writer;
    SSL.SSLArticulationBody.ArticulationBodyStateReader reader;
    float[] joint_pos = new float[6];

    private float[] nbv_home = new float[6];
    public GameObject robot;

    // Start is called before the first frame update
    float deg2rad(double deg)
    {   
        return (float)(deg*Mathf.PI/180);
    }
    void Start()
    {
        writer = new SSL.SSLArticulationBody.ArticulationBodyStateWriter(robot);
        reader = new SSL.SSLArticulationBody.ArticulationBodyStateReader(robot);
        writer.SetControlMode(SSL.ControlModes.POSITION_CONTROL);
        writer.SetJointPositions(new SSL.RobotHomePositions().GetHomePositionsForRobot(SSL.RobotNames.nbv));
        writer.ResetJointLimits();
        ROSConnection.GetOrCreateInstance().Subscribe<Float64Array>("nbv/joint_positions/command", UpdatePosition);

    }
    void UpdatePosition(Float64Array joint_state)
    {
        for(int i=0; i<6; i+=1)
        {
            joint_pos[i] = (float)joint_state.data[i];
        }
        writer.WriteJointPositions(joint_pos);
    }
}
