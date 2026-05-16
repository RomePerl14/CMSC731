using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSL{
    public enum ControlModes
    {
        /// <summary> Position control enumerator </summary>
        POSITION_CONTROL = 0,

        /// <summary> Velocity control enumerator </summary>
        VELOCITY_CONTROL = 1,

        /// <summary> Effort control enumerator </summary>
        EFFORT_CONTROL = 2,

        /// <summary> VR control enumerator </summary>
        VR_CONTROL = 3
    };

    public enum RobotNames
    {
        nbv,
        ranger7dof,
        dyma,
        ranger8dof,
        exos,
        none,
    };

    public class RobotHomePositions
    {
        public float[] nbvHomePosition = {-3.14159f, 0f, -1.5708f, 0f, 1.5708f, 0f};
        public float[] ranger7DofHomePosPosition = {0.0f, -1.5708f, 0.0f, -3.14159f, 0.0f, 0.0f, 0.0f};
        public float[] ranger8DofHomePosPosition = {0.0f, -1.5708f, 0.0f, -3.14159f, 0.0f, 0.0f, -0.785398f, 0.0f};  
        public float[] exosHomePosPosition = {-0.523599f, 0.0f, -1.8326f, -1.5708f, 0.0f, 1.5708f,  1.5708f, 0.0f};
        public float[] dymaHomePosPosition = {0f, -1.5708f, 0f, 3.12f, 0f, -3.12f, 0.0f};

        public RobotHomePositions() {}
        public float[] GetHomePositionsForRobot(RobotNames name)
        {
            switch(name)
            {
                case RobotNames.nbv:
                    return nbvHomePosition;
                case RobotNames.ranger7dof:
                    return ranger7DofHomePosPosition;
                case RobotNames.ranger8dof:
                    return ranger8DofHomePosPosition;
                case RobotNames.exos:
                    return exosHomePosPosition;
                case RobotNames.dyma:
                    return dymaHomePosPosition;
                default:
                    UnityEngine.Debug.LogError("[RobotHomePositions : GetHomePositionsForRobot] FAILED");
                    float[] fail_val = {0f};
                    return fail_val;
            }
        }
    }
    

    namespace SSLArticulationBody
    {
        public struct GravityStatus // Store the gravity status of an ArticulationBody, A boolean AND a description
        {
            public bool usingGravity;
            public string gravityStatus;
        }
        
        public struct UrdfInteriaStatus // Store the URDF Interial status of each joint
        {
            public bool useUrdfInteria;
            public string urdfIntertiaStatus;
        }

        public struct ColliderStatus // Store the status of colliders on the URDF robot (on or off)
        {
            public bool useColliders;
            public string colliderStatus;
        }
    };

}
