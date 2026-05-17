using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Jointstate = RosMessageTypes.Sensor.JointStateMsg;
using Float64Array = RosMessageTypes.SslInterfaces.Float64ArrayMsg;
using SSL;

public class SslHandControllerIKControl : MonoBehaviour
{
    private ROSConnection rosconnect;
    public Transform hand_controller_pose;
    private SSL.SSLArticulationBody.ArticulationBodyStateReader reader;
    double[] matrix_to_vector = new double[16];

    public string topic = "unity/hand_controller_pose";
    // Start is called before the first frame update
    void Start()
    {
        rosconnect = ROSConnection.GetOrCreateInstance();
        rosconnect.RegisterPublisher<Float64Array>(topic);
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        Matrix4x4 tf = Matrix4x4.TRS(hand_controller_pose.position, hand_controller_pose.rotation, UnityEngine.Vector3.one);
        matrix_to_vector[0] = tf[0,0];
        matrix_to_vector[1] = tf[0,1];
        matrix_to_vector[2] = tf[0,2];
        matrix_to_vector[3] = tf[0,3];
        matrix_to_vector[4] = tf[1,0];
        matrix_to_vector[5] = tf[1,1];
        matrix_to_vector[6] = tf[1,2];
        matrix_to_vector[7] = tf[1,3];
        matrix_to_vector[8] = tf[2,0];
        matrix_to_vector[9] = tf[2,1];
        matrix_to_vector[10] = tf[2,2];
        matrix_to_vector[11] = tf[2,3];
        matrix_to_vector[12] = tf[3,0];
        matrix_to_vector[13] = tf[3,1];
        matrix_to_vector[14] = tf[3,2];
        matrix_to_vector[15] = tf[3,3];
        Float64Array controller_state = new Float64Array();
        controller_state.data = matrix_to_vector;
        rosconnect.Publish(topic, controller_state);
    }
}

