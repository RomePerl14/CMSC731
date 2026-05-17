using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Jointstate = RosMessageTypes.Sensor.JointStateMsg;
using Float64Array = RosMessageTypes.SslInterfaces.Float64ArrayMsg;
using SSL;

public class PublishDymaJointStates : MonoBehaviour
{
    private ROSConnection rosconnect;
    private SSL.SSLArticulationBody.ArticulationBodyStateReader reader;

    private double[] position;
    public GameObject robot;
    public string topic = "dyma/unity_joint_states";
    // Start is called before the first frame update
    void Start()
    {
        rosconnect = ROSConnection.GetOrCreateInstance();
        reader = new SSL.SSLArticulationBody.ArticulationBodyStateReader(robot);
        rosconnect.RegisterPublisher<Float64Array>(topic);
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        position = reader.GetJointPositions();

        Float64Array joint_state = new Float64Array();
        joint_state.data = position;
        rosconnect.Publish(topic, joint_state);
    }
}
