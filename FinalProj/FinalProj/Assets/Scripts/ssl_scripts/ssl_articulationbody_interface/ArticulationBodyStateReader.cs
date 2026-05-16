using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Robotics;

namespace SSL{
    namespace SSLArticulationBody
    {

    public class ArticulationBodyStateReader
    {
        // Accessible properties
        public string robotName; // The name of the robot
        public string[] jointNames; // An arry of the names of the joints, in a joint-spaced index (only contains active joints, no baselink or tooldrives)
        public string[] tooldriveNames; // An array of the names of the tool drives, in a tooldrive-spaced index (only contains the tool drive joints)
        public string[] allArticulationBodyNames; // An array of all of the ArticulationBody components listed on the robot
        public int numJoints; // The number of joints
        public int numTooldrives; // The number of tooldrives
        public int numArticulationBodies; // The total number of articulation bodies on the robto
        public Dictionary<string, int> jointIndex = new Dictionary<string, int>(); // Dictionary to map the joint indicies to their respective indicies
        public Dictionary<string, int> tooldriveIndex = new Dictionary<string, int>(); // Dictionary to map the tool drives to their respective indicies
        public Dictionary<string, int> articulationBodyIndex = new Dictionary<string, int>();  // Dictionary to map the entire chain to their respective indicies

        // Non-Accessible properties
        private ArticulationBody rootArticulationBody;
        private ArticulationBody[] collectorChain;
        private ArticulationBody[] articulationChain; // An array of all of the ArticulationBody components on the robot
        private Unity.Robotics.UrdfImporter.UrdfInertial[] UrdfInertialChain; // An array of all of the UrdfInertialData components on the robot
        private MeshCollider[] meshColliderChain; // An array of all of the meshCollider components on the robot

        private int tooldrive_start_index; // The index at which we've reached the end of the robotic arm, and are not at the tool drive
        
        // Constructor
        public ArticulationBodyStateReader(GameObject _robot)
        {
            /*
            Constructor method, builds the object for use in other scripts

            INPUT:
                _robot: A pointer to the GameObject that the class is being called on. This script should be placed at the very top of the robot hierarchy
            
            OUTPUT:
                nothing! (Builds the object)
            */

            // Get the "chains" or arrays of all the things we need on the robot
            collectorChain = _robot.GetComponentsInChildren<ArticulationBody>(); // First, get the root articulation body (this is a multi-limbed system)
            if(rootArticulationBody = _robot.GetComponentInParent<ArticulationBody>()) // Check to see if it exists, and save it while doing so
            {
                articulationChain = new ArticulationBody[collectorChain.Length + 1]; // Resize our chain array
                articulationChain[0] = rootArticulationBody; // Set the root body as the first articulation body
                for(int i=1;i<articulationChain.Length;i+=1) // Iterate and shove the rest of the robot into our chain for use
                {
                    articulationChain[i] = collectorChain[i-1]; // make sure to account for the fact that we are starting at int i=1
                }
            }
            else // If it's no true, do the usual schtuff
            {
                articulationChain = collectorChain;
            }
            UrdfInertialChain = _robot.GetComponentsInChildren<Unity.Robotics.UrdfImporter.UrdfInertial>(); // Get the UrdfInerial components
            meshColliderChain = _robot.GetComponentsInChildren<MeshCollider>(); // Get the MeshCollider components
            allArticulationBodyNames = new string[articulationChain.Length]; //Initialize the array to be the same length as the articulationChain
            numArticulationBodies = articulationChain.Length; // Save the length of the chain for public use

            robotName = _robot.name; // Get the name of the current GameObject

            for(int i=0;i<articulationChain.Length;i++) // Iterate through the entire length of the articulation Chain to search for the "manip" name, and fill in some schtuff along the way
            {
                articulationBodyIndex.Add(articulationChain[i].name, i); // add the name, and the index to the dictionary
                allArticulationBodyNames[i] = articulationChain[i].name; // add the name to the name array
                if(articulationChain[i].name == "manip") // look for the manip name that is so classic at the SSL
                {
                    tooldrive_start_index = i+1; // Store the index that the end effector is at
                }
            }

            jointNames = new string[tooldrive_start_index-1]; // initialize the length of the array to be the length of the number of joints
            if((articulationChain.Length-tooldrive_start_index-1) >= 0)
            {
                tooldriveNames = new string[articulationChain.Length-tooldrive_start_index]; // initialze the length of the array to be the length of the number of tool drives
            }

            getJointNames(); // Get the names of the joints
            getToolDriveNames(); // Get the names of the tooldrives

            numJoints = tooldrive_start_index-1; // The number of joint_positions is the length of the array - 1 (since we don't include the base articulation body)
        }

        // Non-accessible methods
        private void getJointNames() // Gets the joint names
        {
            for(int i=1;i<tooldrive_start_index;i++)
            {
                jointNames[i-1] = articulationChain[i].name; // add the name to the name array
                jointIndex.Add(articulationChain[i].name, i); // add the name to the dictionary, with it's index
            }
        }

        private void getToolDriveNames() // Get the tool drive names
        {
            int count = 0; // start a indexer
            for(int i=tooldrive_start_index;i<articulationChain.Length;i++) // go through every tool drive
            {
                UnityEngine.Debug.Log(i);

                tooldriveNames[count] = articulationChain[i].name; // add the name to the array
                tooldriveIndex.Add(articulationChain[i].name, count); // add the name to the dictionary, with it's index
                count = count + 1; // count up!
            }
        }

        // Accessible methods
        public double getSingleJointPositionInDegrees(string _joint_name) // Get a single joint's position based on it's name (in degrees)
        {
            // TODO: add catch statement
            return articulationChain[jointIndex[_joint_name]].jointPosition[0] * (180/System.Math.PI);
        }
        public double getSingleJointPositionInDegrees(int _joint_number) // Get a single joint's position based on it's index (in degrees)
        {
            // TODO, add catch statement
            return articulationChain[_joint_number].jointPosition[0] * (180/System.Math.PI);
        }
        public double getSingleJointPosition(string _joint_name) // Get a single joint's position based on it's name
        {
            // TODO: add catch statment to watch for bad inputs
            return articulationChain[jointIndex[_joint_name]].jointPosition[0];
        }
        public double getSingleJointPosition(int _joint_number) // Get a single joint's position based on it's index
        {
            // TODO: add catch statement to watch for bad inputs
            return articulationChain[_joint_number].jointPosition[0];
        }
        public double[] getJointPositionsInDegrees() // Get an array of joint positions in degrees
        {
            double[] joint_positions = new double[tooldrive_start_index-1]; // Create an array of doubles to pass
            for(int i=1;i<tooldrive_start_index;i++) // Look through the ArticulationBody chain anf get the positions at each joint
            {
                joint_positions[i-1] = articulationChain[i].jointPosition[0] * (180/System.Math.PI); // Convert the position to degrees, and save it
            }
            return joint_positions; // return the array
        }

        public float[] getJointPositionsInDegreesAsFloats() // Get an array of joint positions in degrees
        {
            float[] joint_positions = new float[tooldrive_start_index-1]; // Create an array of doubles to pass
            for(int i=1;i<tooldrive_start_index;i++) // Look through the ArticulationBody chain anf get the positions at each joint
            {
                joint_positions[i-1] = (float)(articulationChain[i].jointPosition[0] * (180/System.Math.PI)); // Convert the position to degrees, and save it
            }
            return joint_positions; // return the array
        }
        public double[] getJointPositions() // Get an array of joint positions in radians
        {
            double[] joint_positions = new double[tooldrive_start_index-1]; // Create an array of doubles to pass
            for(int i=1;i<tooldrive_start_index;i++) // Look through the ArticulationBody chain anf get the positions at each joint
            {
                joint_positions[i-1] = (double)articulationChain[i].jointPosition[0]; // Save the value at the index
            }
            return joint_positions; // return the array
        }
        public float[] getJointPositionsAsFloats() // Get an array of joint positions in radians
        {
            float[] joint_positions = new float[tooldrive_start_index-1]; // Create an array of doubles to pass
            for(int i=1;i<tooldrive_start_index;i++) // Look through the ArticulationBody chain anf get the positions at each joint
            {
                joint_positions[i-1] = (float)articulationChain[i].jointPosition[0]; // Save the value at the index
            }
            return joint_positions; // return the array
        }

        public double getSingleJointVelocity(string _joint_name) // Get the velocity of a single joint using it's name
        {
            //TODO: add catch statement
            return articulationChain[jointIndex[_joint_name]].jointVelocity[0];
        }
        public double getSingleJointVelocity(int _joint_number) // Get the velocity of a single joint using it's index
        {
            //TODO: add catch statement
            return articulationChain[_joint_number].jointVelocity[0];
        }
        public double[] getJointVelocities() // Get the Joint velocities in reduced coordinates (NOT WORLD SPACE COORDINATES)
        {
            double[] joint_velocities = new double[tooldrive_start_index]; // Initialize an array of doubles
            for(int i=1;i<tooldrive_start_index;i++) // Go through every joint in the chain
            {
                joint_velocities[i-1] = articulationChain[i].jointVelocity[0]; // rads per sec
            }
            return joint_velocities; // return the array
        }

        // NOTE: Doesn't really work? investigate
        public double[] getJointEfforts() // Get the forces acting on each joint in reduced coordinates (NO WORLD SPACE COORDINATES)
        {
            double[] joint_efforts = new double[tooldrive_start_index]; // Initialize an array of doubles
            for(int i=1;i<tooldrive_start_index;i++) // Go through every joint in the chain
            {
                joint_efforts[i-1] = articulationChain[i].jointForce[0]; // N-m
            }
            return joint_efforts;
        }

        /* TOOLDRIVES SECTION - NEED TO REFINE AND FIX AFTER MAKING BETTER TOOLDRIVES*/
        public double[] getToolDrivePositions() // TODO - TEST AND EDIT AND COMMENT (need new end effectors)
        {
            if(tooldrive_start_index != articulationChain.Length)
            {
                double[] tooldrive_positions = new double[articulationChain.Length - tooldrive_start_index];
                int count = 0;
                for(int i=tooldrive_start_index;i<articulationChain.Length;i++)
                {
                    if(articulationChain[i].jointType != ArticulationJointType.FixedJoint)
                    {
                        tooldrive_positions[count] = articulationChain[i].jointPosition[0];
                        count = count + 1;
                    }
                }
                return tooldrive_positions;
            }
            else
            {
                UnityEngine.Debug.LogError("[SSLArticulationBodyStateReader] Error: No tooldrives on the robot!");
                return new double[tooldrive_start_index];
            }
        }

        public double[] getToolDriveVelocities()
        {
            if(tooldrive_start_index != articulationChain.Length)
            {
                double[] tooldrive_velocities = new double[articulationChain.Length - tooldrive_start_index];
                int count = 0;
                for(int i=tooldrive_start_index;i<articulationChain.Length;i++)
                {
                    if(articulationChain[i].jointType != ArticulationJointType.FixedJoint)
                    {
                        tooldrive_velocities[count] = articulationChain[i].jointVelocity[0];
                        count = count + 1;
                    }
                }
                return tooldrive_velocities;
            }
            else
            {
                UnityEngine.Debug.LogError("[SSLArticulationBodyStateReader] Error: No tooldrives on the robot!");
                return new double[tooldrive_start_index];
            }

        }
        /* END TOOLDRIVE SECTION */

        public float[] getJointStiffness()
        {
            float[] joint_stiffnesses = new float[tooldrive_start_index]; // Initialize a new array to use
            for(int i=1;i<tooldrive_start_index;i++)
            {
                // Making an assumption (that should be true) that the drive axis of current and future robots will be the unity x-axis
                joint_stiffnesses[i-1] = articulationChain[i].xDrive.stiffness; // get the stiffness of the axis of rotation
            }
            return joint_stiffnesses; // retrun dem jawns
        }

        public float[] getJointDamping()
        {
            float[] joint_damping = new float[tooldrive_start_index]; // Initialize a new array to use
            for(int i=1;i<tooldrive_start_index;i++)
            {
                // Making an assumption (that should be true) that the drive axis of current and future robots will be the unity x-axis
                joint_damping[i-1] = articulationChain[i].xDrive.damping; // get the damping of the axis of rotation
            }
            return joint_damping; // retrun dem jawns
        }

        public float[] getJointForceLimit()
        {
            float[] joint_forcelimits = new float[tooldrive_start_index]; // Initialize a new array to use
            for(int i=1;i<tooldrive_start_index;i++)
            {
                // Making an assumption (that should be true) that the drive axis of current and future robots will be the unity x-axis
                joint_forcelimits[i-1] = articulationChain[i].xDrive.forceLimit; // get the force limits of the axis of rotation
            }
            return joint_forcelimits; // retrun dem jawns
        }

        public float[] getJointUpperLimits()
        {
            float[] joint_limits = new float[tooldrive_start_index]; // Initialize a new array to use
            for(int i=1;i<tooldrive_start_index;i++)
            {
                // Making an assumption (that should be true) that the drive axis of current and future robots will be the unity x-axis
                joint_limits[i-1] = articulationChain[i].xDrive.upperLimit; // get the damping of the axis of rotation
            }
            return joint_limits; // retrun dem jawns

        }
        public float[] getJointLowerLimits()
        {
            float[] joint_limits = new float[tooldrive_start_index]; // Initialize a new array to use
            for(int i=1;i<tooldrive_start_index;i++)
            {
                // Making an assumption (that should be true) that the drive axis of current and future robots will be the unity x-axis
                joint_limits[i-1] = articulationChain[i].xDrive.lowerLimit; // get the damping of the axis of rotation
            }
            return joint_limits; // retrun dem jawns
        }
        
        // TODO: GET TOOL DRIVE LIMITS:
        //public float[] getToolDriveUpperLimits()
        //public float[] getToolDriveLowerLimits()

        public GravityStatus[] getJointGravityStatus()
        {
            GravityStatus[] gravity_statuses = new GravityStatus[tooldrive_start_index];
            for(int i=1;i<tooldrive_start_index;i++)
            {
                if(articulationChain[i].useGravity == true)
                {
                    gravity_statuses[i-1].usingGravity = articulationChain[i].useGravity;
                    UnityEngine.Vector3 current_gravity = UnityEngine.Physics.gravity;
                    gravity_statuses[i-1].gravityStatus = "Gravity is on for " + robotName + "/" + articulationChain[i].name + " at: " + current_gravity;
                }
                else
                {
                    gravity_statuses[i-1].usingGravity = articulationChain[i].useGravity;
                    gravity_statuses[i-1].gravityStatus = "Gravity is off for " + robotName + "/" + articulationChain[i].name;
                }
            }
            return gravity_statuses;
        }

        // Need to think more about these: URDF wont have inertial data in base link, so the chain will be shorter than the articulation bodies
        // public UrdfInteriaStatus[] getUrdfInteriaStatus() // NOTE: Only checking joints for now
        // {
        //     UrdfInteriaStatus[] urdf_interia_status = new UrdfInteriaStatus[tooldrive_start_index];
        //     for(int i=1;i<tooldrive_start_index;i++)
        //     {
        //         if(UrdfInertialChain[i].useUrdfData == true)
        //         {
        //             urdf_interia_status[i-1].useUrdfInteria = UrdfInertialChain[i].useUrdfData;
        //             urdf_interia_status[i-1].urdfIntertiaStatus = robotName + "/" + jointNames[i-1] + " has URDF interial data active";
        //         }
        //         else
        //         {
        //             urdf_interia_status[i-1].useUrdfInteria = UrdfInertialChain[i].useUrdfData;
        //             urdf_interia_status[i-1].urdfIntertiaStatus = robotName + "/" + jointNames[i-1] + " not using URDF interial data";
        //         }
        //     }
        //     return urdf_interia_status;
        // }

        public ColliderStatus[] getRobotColliderStatus()
        {
            ColliderStatus[] urdf_collider_status = new ColliderStatus[articulationChain.Length];
            for(int i=1;i<articulationChain.Length;i++)
            {
                if(meshColliderChain[i].convex == true)
                {
                    urdf_collider_status[i].useColliders = meshColliderChain[i].convex;
                    urdf_collider_status[i].colliderStatus = "Colliders active for " + robotName + "/" + allArticulationBodyNames[i];
                }
                else
                {
                    urdf_collider_status[i].useColliders = meshColliderChain[i].convex;
                    urdf_collider_status[i].colliderStatus = "Colliders inactive for " + robotName + "/" + allArticulationBodyNames[i];
                }
            }
            return urdf_collider_status;
        }
        // TODO AFTER UPDATEING UNITY: ADD FORCE AND TORQUE SCHTUFF
        

    }
    };
};
