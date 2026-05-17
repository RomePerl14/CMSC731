using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics;
using System;
using RosMessageTypes.Std;

namespace SSL{
    namespace SSLArticulationBody
    {
    
    /// <summary> A class to help with writing commands to ArticulationBody's on a robot </summary>
    public class ArticulationBodyStateWriter
    {

    /*-------- ORGANIZATION VARIABLES --------*/
    
        /// <summary> Dictionary to map the joint names to their respective indicies in the joint array </summary>
        private Dictionary<string, int> jointIndex = new Dictionary<string, int>();

        /// <summary> Dictionary to map the tool drive names to their respective indicies in the tool drive array </summary>
        private Dictionary<string, int> tooldriveIndex = new Dictionary<string, int>();

        /// <summary> Dictionary to map the Articulation Bodies names to their respective indicies in the array </summary>
        private Dictionary<string, int> articulationBodyIndex = new Dictionary<string, int>();

        // -- NOTE: here, a "chain" represents an array. I original saw someone use chain to describe it and it's stuck ever since -- //
        ///<summary> The root articulation body, if it exists (ie if we are on a spacecraft) </summary>
        private ArticulationBody rootArticulationBody;

        ///<summary> A collector chain, to collect all of the articulation bodies (mostly to account for a possible root body) </summary>
        private ArticulationBody[] collectorChain;

        ///<summary> A chain of articulation bodies, gathered from the robot tree in the hiearchy </summary>
        private ArticulationBody[] articulationChain;

        ///<summary> A chain of mesh colliders, from each robot (to be used later) - if colliders are desired to be turned on/off for some reason </summary>
        private MeshCollider[] meshColliderChain;

        ///<summary> The index that the robot ends and the tooldrive begins. Used for iteration (ie for i less than tooldrive_start_index) </summary>
        private int tooldrive_start_index; // 

        /// <summary> The entire length of the articulation chain. Used for iteration </summary>
        private int articulation_chain_length;

    /*-------- FREQUENTLY USED VARIABLES --------*/

        ///<summary> An array ArticulationReducedSpace variables to write to (since you can't write directly to the method on the articulation body, you set it equal to this) </summary>
        ArticulationReducedSpace[] write_positions;

        ///<summary> An array of ArticulationDrive variables to write to (since again, you can't write directly to the method on the ArticulationBody) </summary>
        ArticulationDrive[] joint_drives;

        ///<summary> A single ArticulationReducedSpace variable for writing a single position </summary>
        ArticulationReducedSpace write_position;

        ///<summary> A single ArticulationReducedSpace variable for writing a single velocity </summary>
        ArticulationReducedSpace write_velocity;

        ///<summary> A single ArticulationDrive variable for writing to a single robot's ArticulationDrive property </summary>
        ArticulationDrive joint_drive;
        

    /*-------- CLASS CONSTRUCTOR --------*/

        /// <summary> A constructor for the class (classic coding moment) </summary>
        /// <param name="_robot"> The parent object of the robot you want to drive (not necessarily the root object) - i.e. the very top part of the robot hierarchy in Unity (usually the robot's name) </param>
        /// <Returns> Nothing, however, it does a LOT of setup </Returns>
        public ArticulationBodyStateWriter(GameObject _robot) // Takes in an input of 
        {
            // Get the ArticulationBody information
            collectorChain = _robot.GetComponentsInChildren<ArticulationBody>(); // First, get the root articulation body (if this is a multi-limbed system)
            if(rootArticulationBody = _robot.GetComponentInParent<ArticulationBody>()) // Check to see if it exists, and save it while doing so
            {
                articulationChain = new ArticulationBody[collectorChain.Length + 1]; // Resize our chain array
                articulationChain[0] = rootArticulationBody; // Set the root body as the first articulation body
                for(int i=1;i<articulationChain.Length;i+=1) // Iterate and shove the rest of the _robot into our chain for use
                {
                    articulationChain[i] = collectorChain[i-1]; // make sure to account for the fact that we are starting at int i=1
                }
            }
            else // If we're just a classic old robot arm, do the usual 
            {
                articulationChain = collectorChain;
            }
            
            // Get the length of the articulation chain and save it locally
            articulation_chain_length = articulationChain.Length;
            
            // TODO: IMPLEMENT THE SAME ABOVE SO WE CAN GET THE MESH COLLIDER OF THE ROOT BODY (if there is one)
            // Get the entire meshcollider chain (we shall need this)
            meshColliderChain = _robot.GetComponentsInChildren<MeshCollider>(); // Literally get the chain of mesh colliders on the robot
            for(int i=0;i<articulation_chain_length;i+=1) // Iterate through the entire length of the chain and look for the word "manip" - I know, it sucks, but COPE
            {
                articulationBodyIndex.Add(articulationChain[i].name, i); // Add the names as we go along (so the people can use it)
                if(articulationChain[i].name == "manip")
                {
                    tooldrive_start_index = i+1; // Save the sucker (add one so that we can do the "for i < " thingy)
                }
            }
            
            // Check to see if we found to end of the robot arm
            if(tooldrive_start_index == -1)
            {
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : ArticulationBodyStateWriter] Could not find the end of the robot arm, do you have a joint named manip?\nSetTing the tooldrive start index == to length of robot, this is gonna cause some big issues\n");
                tooldrive_start_index = articulationChain.Length;
            }
            // Build the DICtionaries
            BuildJointDictionary(); 
            BuildToolDriveDictionary();     
            
            // build private variables:
            write_positions = new ArticulationReducedSpace[tooldrive_start_index]; // resize to match the size of our arm (not including tooldrives)
            joint_drives = new ArticulationDrive[tooldrive_start_index]; // resize to match the size of arm (not including tool drives)
        }


    /*---------------- PRIVATE FUNCTIONS ----------------*/
        /// <summary> Builds the dictionary of joints mapping names to indicies, so we can use the name of a joint instead of the index (if we want to) </summary>
        /// <Returns> nothing... </Returns>
        private void BuildJointDictionary()
        {
            // Put all of the names of the joints into a map with it's index
            for(int i=1;i<tooldrive_start_index;i+=1)
            {
                jointIndex.Add(articulationChain[i].name, i); // Here, we're saving the articulation chain index because that's what we access down the line
            }
        }

        /// <summary> Builds the dictionary of tooldrives mapping names to indicies, so we can use the name of a joint instead of the index (if we want to) </summary>
        /// <Returns> nothing... </Returns>
        private void BuildToolDriveDictionary()
        {
            // Put all of the names of the tooldrives into a map with it's index
            for(int i=tooldrive_start_index;i<articulationChain.Length;i+=1)
            {
                tooldriveIndex.Add(articulationChain[i].name, i); // Here, we're saving the articulation chain index because that's what we access down the line
            }
        }


    /*---------------- SET JOINT POSITIONS ----------------*/
        public bool SetControlMode(SSL.ControlModes _control_mode)
        {
            switch(_control_mode)
            {
                case ControlModes.POSITION_CONTROL:
                    UnityEngine.Debug.Log("[ArticulationBodyStateWriter : SetControlMode] Control mode set to: " + _control_mode.ToString() + "\n");
                    WriteUniformStiffnessToJoints(10000f);
                    WriteUniformDampingToJoints(1000f);
                    break;
                case ControlModes.VR_CONTROL:
                    UnityEngine.Debug.Log("[ArticulationBodyStateWriter : SetControlMode] Control mode set to: " + _control_mode.ToString() + "\n");
                    WriteUniformStiffnessToJoints(0f); // not stiffness for VR control
                    WriteUniformDampingToJoints(1000f);
                    break;
            }
            return true;
        }

        /// <summary> 
        ///     [WARNING: This function displays instantenous motion - as in, it will reach the set joint positions instanteously and will NOT account for joint limits inherently] - 
        ///     Instantaneously set the joint positions to a desired value (Should only be used for initializing home positions at the start, or recalibrating the arm. Should NOT be used for general position commanding) 
        /// </summary>
        /// <param name="_positions"> An array of floats to send to each joint of the arm (needs to be the same size) - unit is radians </param>
        /// <returns> Returns a 'true' boolean upon success </returns>
        public bool SetJointPositions(float[] _positions)
        {   
            // Check to see if the input array is the same length as our robot arm
            if(_positions.Length != tooldrive_start_index-1) 
            {   // If it's not, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : SetJointPositions] Incorrect size array passed to write function! Array should be of size: " + (tooldrive_start_index-1).ToString() + " Inputted array size: " + _positions.Length.ToString() + "\n");
                return false; // return false
            }
            
            // If the above does not happen continue with writing positions to the arm
            for(int i=1;i<tooldrive_start_index;i+=1)
            {
                write_positions[i-1] = articulationChain[i].jointPosition; // First, save the ArticulationReducedSpace property into a 'local' variable
                joint_drives[i-1] = articulationChain[i].xDrive; // Second, save the ArticulationDrive property into a 'local' variable
                write_positions[i-1][0] = _positions[i-1]; // Set the local ArticulationReducedSpace variable's value (the desired position)
                joint_drives[i-1].target = (float)(_positions[i-1]*(180/System.Math.PI)); // Set the local ArticulationDrive variable's 'target' property value (convert radian input into degrees)
                articulationChain[i].xDrive = joint_drives[i-1]; // Set the current ArticulationBody's xDrive property (an ArticulationDrive type) equal to the local ArticulationDrive value
                articulationChain[i].jointPosition = write_positions[i-1]; // Set the current ArticulationBody's jointPosition property (an ArticulationReducedSpace type) equal to the local ArticulationReducedSpace value
            }
            return true;
        }
        /// <summary> 
        ///     [WARNING: This function displays instantenous motion - as in, it will reach the set joint positions instanteously and will NOT account for joint limits inherently] - 
        ///     Instantaneously set the joint position to a desired value (Should only be used for initializing home positions at the start, or recalibrating the arm. Should NOT be used for general position commanding) 
        /// </summary>
        /// <param name="_joint_name"> The name of the joint </param>
        /// <param name="_position"> A float for position to send to each joint of the arm (needs to be the same size) - unit is radians </param>
        /// <returns> Returns a 'true' boolean upon success </returns>
        public bool SetSingleJointPosition(string _joint_name, float _position) // Write joint positions, in radians
        {
            // Check to make sure the joint exists in the dictionary
            if (jointIndex.ContainsKey(_joint_name) == false)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : SetSingleJointPosition] No joint named '" + _joint_name + "' on the current robot!\n");
                return false; // return false
            }

            // For detailed description of what's being done in this chunk, see the 'SetJointPositions()' function above
            write_position = articulationChain[jointIndex[_joint_name]].jointPosition;
            joint_drive = articulationChain[jointIndex[_joint_name]].xDrive;
            write_position[0] = _position;
            joint_drive.target = (float)(_position*(180/System.Math.PI));
            articulationChain[jointIndex[_joint_name]].xDrive = joint_drive;
            articulationChain[jointIndex[_joint_name]].jointPosition = write_position;
            return true;
        }
        /// <summary> 
        ///     [WARNING: This function displays instantenous motion - as in, it will reach the set joint positions instanteously and will NOT account for joint limits inherently] - 
        ///     Instantaneously set the joint position to a desired value (Should only be used for initializing home positions at the start, or recalibrating the arm. Should NOT be used for general position commanding) 
        /// </summary>
        /// <param name="_joint_number"> An integer representing the joint number (not joint index) </param>
        /// <param name="_position"> A float for position to send to each joint of the arm (needs to be the same size) - unit is radians </param>
        /// <returns> Returns a 'true' boolean upon success </returns>
        public bool SetSingleJointPosition(int _joint_number, float _position)
        {
            // Check to make sure the joint number exists
            if (_joint_number <= 0 || _joint_number > tooldrive_start_index)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : SetSingleJointPosition] No joint numbered: " + _joint_number.ToString() + " on the robot!");
                return false; // return false
            }
            // For detailed description of what's being done in this chunk, see the 'WriteJointPositions()' function above
            write_position = articulationChain[_joint_number].jointPosition;
            joint_drive = articulationChain[_joint_number].xDrive;
            write_position[0] = _position;
            joint_drive.target = (float)(_position*(180/System.Math.PI));
            articulationChain[_joint_number].xDrive = joint_drive;
            articulationChain[_joint_number].jointPosition = write_position;
            return true;
        }


    /*---------------- WRITE JOINT POSITIONS ----------------*/
        /// <summary> 
        ///     [WARNING: Large jumps in position values do not guarantee a specific velocity] - WriteS position commands to
        ///     each joint on the arm provided by an array of positions, using simulated torque commands and some sort of simulated
        ///     absolute encoder. (Note: the "writePositions" suite of functions should be used for general position control interfaces)
        /// </summary>
        /// <param name="_positions"> An array of floats to send to each joint of the arm (needs to be the same size) - unit is radians </param>
        /// <returns> Returns a 'true' boolean upon success </returns>
        public bool WriteJointPositions(float[] _positions)
        {
            // Check to see if the input array is the same length as our robot arm
            if(_positions.Length != tooldrive_start_index-1)
            {   // If it's not, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : WriteJointPositions] Incorrect size array passed to write function! Array should be of size: " + (tooldrive_start_index-1).ToString() + " Inputted array size: " + _positions.Length.ToString() + "\n");
                return false; // return false
            }
            
            // If the above does not happen continue with writing positions to the arm
            for(int i=1;i<tooldrive_start_index;i+=1)
            {
                joint_drives[i-1] = articulationChain[i].xDrive; // Second, save the ArticulationDrive property into a 'local' variable
                joint_drives[i-1].target = (float)(_positions[i-1]*(180/System.Math.PI)); // Set the local ArticulationDrive variable's 'target' property value (the desired position in radians)
                articulationChain[i].xDrive = joint_drives[i-1]; // Set the current ArticulationBody's xDrive property (an ArticulationDrive type) equal to the local ArticulationDrive value
            }
            return true;
        }
        /// <summary> 
        ///     [WARNING: Large jumps in position values do not guarantee a specific velocity] - WriteS a position command to
        ///     a name-specified joint on the arm, using torque commands and some sort of simulated absolute encoder. 
        ///     (Note: the "writePositions" suite of functions should be used for general position control interfaces) 
        /// </summary>
        /// <param name="_joint_name"> The name of the joint, to be used by the joint dictionary </param>
        /// <param name="_position"> The desired position - unit is radians </param>
        /// <returns> Returns a 'true' boolean upon success </returns>
        public bool WriteSingleJointPosition(string _joint_name, float _position) // Write joint positions, in radians
        {
            // Check to make sure the joint exists in the dictionary
            if (jointIndex.ContainsKey(_joint_name) == false)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : WriteSingleJointPosition] No joint named '" + _joint_name + "' on the current robot!\n");
                return false; // return false
            }

            // For detailed description of what's being done in this chunk, see the 'WriteJointPositions()' function above
            joint_drive = articulationChain[jointIndex[_joint_name]].xDrive;
            joint_drive.target = (float)(_position*(180/System.Math.PI));
            articulationChain[jointIndex[_joint_name]].xDrive = joint_drive;
            return true;
        }
        /// <summary> 
        ///     [WARNING: Large jumps in position values do not guarantee a specific velocity] - WriteS a position command
        ///      to a number-specified joint on the arm, using torque commands and some sort of simulated absolute encoder. 
        ///     (Note: the "writePositions" suite of functions should be used for general position control interfaces) 
        /// </summary>
        /// <param name="_joint_number"> The joint number, from 1 -> n (with n being the number of joints) </param>
        /// <param name="_position"> The desired position - unit is radians</param>
        /// <returns> Returns a 'true' boolean upon success </returns>
        public bool WriteSingleJointPosition(int _joint_number, float _position)
        {
            // Check to make sure the joint number exists
            if (_joint_number <= 0 || _joint_number > tooldrive_start_index)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : WriteSingleJointPosition] No joint numbered: " + _joint_number.ToString() + " on the robot!");
                return false; // return false
            }

            // For detailed description of what's being done in this chunk, see the 'WriteJointPositions()' function above
            joint_drive = articulationChain[_joint_number].xDrive;
            joint_drive.target = (float)(_position*(180/System.Math.PI));
            articulationChain[_joint_number].xDrive = joint_drive;
            return true;
        }


    /*---------------- JOINT LIMITS MODIFICATIONS ----------------*/
        /// <summary> ReSetS the joint limits from whatever the URDF specified to be -360 and 360 degress for the lower and upper limits, respectively</summary>
        /// <returns> Returns a "true" boolean upon success </returns>
        public bool ResetJointLimits()
        {
            for(int i=1;i<tooldrive_start_index;i+=1) //Go through each joint and reset the limits
            {
                joint_drives[i-1] = articulationChain[i].xDrive; // Second, save the ArticulationDrive property into a 'local' variable
                joint_drives[i-1].lowerLimit = -360f; // Change the lower limit
                joint_drives[i-1].upperLimit = 360f; // Change the upper limit
                articulationChain[i].xDrive = joint_drives[i-1]; // Set the current ArticulationBody's xDrive property (an ArticulationDrive type) equal to the local ArticulationDrive value
            }
            return true;
        }
        /// <summary> ReSetS a single joint's limits from whatever the URDF specified to be -360 and 360 degress for the lower and upper limits, respectively</summary>
        ///<param name="_joint_name"> The name of the joint to reset </param>
        /// <returns> Returns a "true" boolean upon success </returns>
        public bool ResetSingleJointLimit(string _joint_name)
        {
            // Check to make sure the joint exists in the dictionary
            if (jointIndex.ContainsKey(_joint_name) == false)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : reSetSingleJointLimit] No joint named '" + _joint_name + "' on the current robot!\n");
                return false; // return false
            }

            // Set the limits of the joint
            joint_drive = articulationChain[jointIndex[_joint_name]].xDrive; // Second, save the ArticulationDrive property into a 'local' variable
            joint_drive.lowerLimit = -360f; // Change the lower limit
            joint_drive.upperLimit = 360f; // Change the upper limit
            articulationChain[jointIndex[_joint_name]].xDrive = joint_drive; // Set the current ArticulationBody's xDrive property (an ArticulationDrive type) equal to the local ArticulationDrive value
            return true;
        }
        /// <summary> ReSetS a single joint's limits from whatever the URDF specified to be -360 and 360 degress for the lower and upper limits, respectively</summary>
        ///<param name="_joint_number"> The joint number to reset (not the joint index) </param>
        /// <returns> Returns a "true" boolean upon success </returns>
        public bool ResetSingleJointLimit(int _joint_number)
        {
            // Check to make sure the joint number exists
            if (_joint_number <= 0 || _joint_number > tooldrive_start_index)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : reSetSingleJointLimit] No joint numbered: " + _joint_number.ToString() + " on the robot!");
                return false; // return false
            }

            // Set the limits of the joint
            joint_drive = articulationChain[_joint_number].xDrive; // Second, save the ArticulationDrive property into a 'local' variable
            joint_drive.lowerLimit = -360f; // Change the lower limit
            joint_drive.upperLimit = 360f; // Change the upper limit
            articulationChain[_joint_number].xDrive = joint_drive; // Set the current ArticulationBody's xDrive property (an ArticulationDrive type) equal to the local ArticulationDrive value
            return true;
        }
        /// <summary> RewriteS the joint limits in Unity to the desired joint limits for each joint in the robot </summary>
        /// <param name="_joint_limits"> A list of float arrays (2 indicies each) that contain the lower and upper limits of each joint at index 0 and 1 respectively </param>
        /// <returns> Returns a "true" boolean upon success </returns>
        public bool RewriteJointLimits(List<float[]> _joint_limits)
        {
            // Make sure that the inputted list includes all of the links
            if (_joint_limits.Count != tooldrive_start_index-1)
            {
                // Throw an error is it's not
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : RewriteJointLimits] The inputted list is not the correct size! Expected size: " + (tooldrive_start_index-1).ToString() + ", Inputted size: " + _joint_limits.Count.ToString());
                return false;
            }
            for(int i=1;i<tooldrive_start_index;i+=1) //Go through and rewrite the limits of each joint in Unity (not in the URDF)
            {
                // Check to make sure we were given two limits per joint
                if (_joint_limits[i].Length != 2)
                {
                    // If each internal float array is not 2 indicies, throw and error and return
                    UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : RewriteJointLimits] Upper and lower joint limits not given at index " + i.ToString() + ". The limits for each joint must be inputted as a 2-index float array! Given size: " + _joint_limits[i].Length.ToString());
                    return false;
                }  
                // Set the limits of the joint
                joint_drive = articulationChain[i].xDrive; // Second, save the ArticulationDrive property into a 'local' variable
                joint_drive.lowerLimit = _joint_limits[i][0]; // Change the lower limit
                joint_drive.upperLimit = _joint_limits[i][1]; // Change the upper limit
                articulationChain[i].xDrive = joint_drive; // Set the current ArticulationBody's xDrive property (an ArticulationDrive type) equal to the local ArticulationDrive value
            }
            return true;
        }
        /// <summary> RewriteS a single joints limits in Unity to the desired inputted joint limits </summary>
        /// <param name="_joint_name"> The name of the joint </param>
        /// <param name="_joint_limits"> A 2-index float array for the lower and upper limits being index 0 and 1 respectively </param>
        /// <returns> Returns a "true" boolean upon success </returns>
        public bool RewriteSingleJointLimit(string _joint_name, float[] _joint_limits)
        {
            // Check to make sure the joint exists in the dictionary
            if (jointIndex.ContainsKey(_joint_name) == false)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : RewriteSingleJointLimits] No joint named '" + _joint_name + "' on the current robot!\n");
                return false; // return false
            }
            // Check to make sure we were given two limits per joint
            if (_joint_limits.Length != 2)
            {
                // If each internal float array is not 2 indicies, throw and error and return
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : RewriteSingleJointLimits] Upper and lower joint limits not given! Joint limits must be inputted as an 2-index float array! Given size: " + _joint_limits.Length.ToString());
                return false;
            }

            // Set the limits of the joint
            joint_drive = articulationChain[jointIndex[_joint_name]].xDrive; // Second, save the ArticulationDrive property into a 'local' variable
            joint_drive.lowerLimit = _joint_limits[0]; // Change the lower limit
            joint_drive.upperLimit = _joint_limits[1]; // Change the upper limit
            articulationChain[jointIndex[_joint_name]].xDrive = joint_drive; // Set the current ArticulationBody's xDrive property (an ArticulationDrive type) equal to the local ArticulationDrive value
            return true;
        }
        /// <summary> RewriteS a single joints limits in Unity to the desired inputted joint limits </summary>
        /// <param name="_joint_number"> The joint number (not index) </param>
        /// <param name="_joint_limits"> A 2-index float array for the lower and upper limits being index 0 and 1 respectively </param>
        /// <returns> Returns a "true" boolean upon success </returns>
        public bool RewriteSingleJointLimit(int _joint_number, float[] _joint_limits)
        {
            // Check to make sure the joint number exists
            if (_joint_number <= 0 || _joint_number > tooldrive_start_index)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : RewriteSingleJointLimit] No joint numbered: " + _joint_number.ToString() + " on the robot!");
                return false; // return false
            }
            // Check to make sure we were given two limits per joint
            if (_joint_limits.Length != 2)
            {
                // If each internal float array is not 2 indicies, throw and error and return
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : RewriteSingleJointLimits] Upper and lower joint limits not given! Joint limits must be inputted as an 2-index float array! Given size: " + _joint_limits.Length.ToString());
                return false;
            }  

            // Set the limits of the joint
            joint_drive = articulationChain[_joint_number].xDrive; // Second, save the ArticulationDrive property into a 'local' variable
            joint_drive.lowerLimit = _joint_limits[0]; // Change the lower limit
            joint_drive.upperLimit = _joint_limits[1]; // Change the upper limit
            articulationChain[_joint_number].xDrive = joint_drive; // Set the current ArticulationBody's xDrive property (an ArticulationDrive type) equal to the local ArticulationDrive value
            return true;
        }


    /*---------------- SET TOOLDRIVE POSITIONS ----------------*/
        /// <summary> [WARNING: This function displays instantenous motion - as in, it will reach the set joint positions instanteously and will NOT account for joint limits inherently] - 
        /// Instantaneously set the joint positions to a desired value (Should only be used for initializing home positions at the start, or recalibrating the arm. Should NOT be used for general position commanding) </summary>
        /// <param name="_positions"> An array of floats to send to each joint of the arm (needs to be the same size) - unit is radians </param>
        /// <returns> Returns a 'true' boolean upon success </returns>
        public bool SetToolDrivePositions(float[] _positions)
        {   
            // Check to see if the input array is the same length as the amount of tooldrives
            if(_positions.Length != articulation_chain_length-tooldrive_start_index-1) 
            {   // If it's not, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : SetToolDrivePositions] Incorrect size array passed to write function! Array should be of size: " + (tooldrive_start_index-1).ToString() + " Inputted array size: " + _positions.Length.ToString() + "\n");
                return false; // return false
            }
            
            // If the above does not happen continue with writing positions to the arm
            for(int i=tooldrive_start_index;i<articulation_chain_length;i+=1)
            {
                write_positions[i-1] = articulationChain[i].jointPosition; // First, save the ArticulationReducedSpace property into a 'local' variable
                joint_drives[i-1] = articulationChain[i].xDrive; // Second, save the ArticulationDrive property into a 'local' variable
                write_positions[i-1][0] = _positions[i-1]; // Set the local ArticulationReducedSpace variable's value (the desired position)
                joint_drives[i-1].target = (float)(_positions[i-1]*(180/System.Math.PI)); // Set the local ArticulationDrive variable's 'target' property value (convert radian input into degrees)
                articulationChain[i].xDrive = joint_drives[i-1]; // Set the current ArticulationBody's xDrive property (an ArticulationDrive type) equal to the local ArticulationDrive value
                articulationChain[i].jointPosition = write_positions[i-1]; // Set the current ArticulationBody's jointPosition property (an ArticulationReducedSpace type) equal to the local ArticulationReducedSpace value
            }
            return true;
        }
        /// <summary> [WARNING: This function displays instantenous motion - as in, it will reach the set joint positions instanteously and will NOT account for joint limits inherently] - 
        /// Instantaneously set the joint position to a desired value (Should only be used for initializing home positions at the start, or recalibrating the arm. Should NOT be used for general position commanding) </summary>
        /// <param name="_joint_name"> The name of the joint </param>
        /// <param name="_position"> A float for position to send to each joint of the arm (needs to be the same size) - unit is radians </param>
        /// <returns> Returns a 'true' boolean upon success </returns>
        public bool SetSingleToolDrivePosition(string _joint_name, float _position) // Write joint positions, in radians
        {
            // Check to make sure the joint exists in the dictionary
            if (tooldriveIndex.ContainsKey(_joint_name) == false)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : SetSingleToolDrivePosition] No tooldrive joint named '" + _joint_name + "' on the current robot!\n");
                return false; // return false
            }

            // For detailed description of what's being done in this chunk, see the 'SetToolDrivePositions()' function above
            write_position = articulationChain[tooldriveIndex[_joint_name]].jointPosition;
            joint_drive = articulationChain[tooldriveIndex[_joint_name]].xDrive;
            write_position[0] = _position;
            joint_drive.target = (float)(_position*(180/System.Math.PI));
            articulationChain[tooldriveIndex[_joint_name]].xDrive = joint_drive;
            articulationChain[tooldriveIndex[_joint_name]].jointPosition = write_position;
            return true;
        }
        /// <summary> [WARNING: This function displays instantenous motion - as in, it will reach the set joint positions instanteously and will NOT account for joint limits inherently] - 
        /// Instantaneously set the joint position to a desired value (Should only be used for initializing home positions at the start, or recalibrating the arm. Should NOT be used for general position commanding) </summary>
        /// <param name="_joint_number"> An integer representing the joint number (not joint index) </param>
        /// <param name="_position"> A float for position to send to each joint of the arm (needs to be the same size) - unit is radians </param>
        /// <returns> Returns a 'true' boolean upon success </returns>
        public bool SetSingleToolDrivePosition(int _joint_number, float _position)
        {
            // Check to make sure the joint number exists
            if (_joint_number <= tooldrive_start_index-1 || _joint_number > articulation_chain_length)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : SetSingleJointPosition] No tooldrive joint numbered: " + _joint_number.ToString() + " on the robot!");
                return false; // return false
            }
            // For detailed description of what's being done in this chunk, see the 'SetToolDrivePositions()' function above
            write_position = articulationChain[_joint_number].jointPosition;
            joint_drive = articulationChain[_joint_number].xDrive;
            write_position[0] = _position;
            joint_drive.target = (float)(_position*(180/System.Math.PI));
            articulationChain[_joint_number].xDrive = joint_drive;
            articulationChain[_joint_number].jointPosition = write_position;
            return true;
        }


    /*---------------- WRITE TOOLDRIVE POSITIONS ----------------*/
        /// <summary> WriteS the tooldrives to a desire position (either translational or rotational) </summary>
        /// <param name="_positions"> The positions to write to each tool drive, in the same order that the tool drives are built </param>
        /// <returns> Returns a "true" boolean upon success </returns>
        public bool WriteToolDrivePositions(float[] _positions)
        {
            // Check to see if the input array is the same length as the amount of tooldrives
            if(_positions.Length != articulation_chain_length-tooldrive_start_index-1) 
            {   // If it's not, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : SetToolDrivePositions] Incorrect size array passed to write function! Array should be of size: " + (articulation_chain_length-tooldrive_start_index-1).ToString() + " Inputted array size: " + _positions.Length.ToString() + "\n");
                return false; // return false
            }
            
            // If the above does not happen continue with writing positions to the arm
            for(int i=1;i<tooldrive_start_index;i+=1)
            {
                joint_drives[i-1] = articulationChain[i].xDrive; // Second, save the ArticulationDrive property into a 'local' variable
                joint_drives[i-1].target = (float)(_positions[i-1]*(180/System.Math.PI)); // Set the local ArticulationDrive variable's 'target' property value (the desired position in radians)
                articulationChain[i].xDrive = joint_drives[i-1]; // Set the current ArticulationBody's xDrive property (an ArticulationDrive type) equal to the local ArticulationDrive value
            }
            return true;
        }
        /// <summary> WriteS a single position to a desired tooldrive defined by it's name </summary>
        /// <param name="_joint_name"> The name of the tooldrive </param>
        /// <param name="_position"> The desired position of the tooldrive </param>
        /// <returns> Returns a "true" boolean upon success </returns>
        public bool WriteSingleToolDrivePosition(string _joint_name, float _position) // Write joint positions, in radians
        {
            // Check to make sure the joint exists in the dictionary
            if (tooldriveIndex.ContainsKey(_joint_name) == false)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : WriteSingleToolDrivePosition] No tooldrive joint named '" + _joint_name + "' on the current robot!\n");
                return false; // return false
            }

            // For detailed description of what's being done in this chunk, see the 'SetToolDrivePositions()' function above
            joint_drive = articulationChain[tooldriveIndex[_joint_name]].xDrive;
            joint_drive.target = (float)(_position*(180/System.Math.PI));
            articulationChain[tooldriveIndex[_joint_name]].xDrive = joint_drive;
            return true;
        }
        /// <summary> WriteS a single position to a desired tooldrive defined by it's joint number (not index) </summary>
        /// <param name="_joint_number"> The joint number (not the index) of the tooldrive </param>
        /// <param name="_position"> The desired position of the tooldrive </param>
        /// <returns> Returns a "true" boolean upon success </returns>
        public bool WriteSingleToolDrivePosition(int _joint_number, float _position)
        {
            // Check to make sure the joint number exists
            if (_joint_number <= tooldrive_start_index-1 || _joint_number > articulation_chain_length)
            {   // If it doesnt' exists, throw an error
                UnityEngine.Debug.LogError("[ArticulationBodyStateWriter : WriteSingleJointPosition] No tooldrive joint numbered: " + _joint_number.ToString() + " on the robot!");
                return false; // return false
            }
            // For detailed description of what's being done in this chunk, see the 'SetToolDrivePositions()' function above
            joint_drive = articulationChain[_joint_number].xDrive;
            joint_drive.target = (float)(_position*(180/System.Math.PI));
            articulationChain[_joint_number].xDrive = joint_drive;
            return true;
        }

        public bool WriteToolDriveVelocities(float velocities)
        {
            return true;
        }

        public bool CloseToolDrives() // Temporary - do better
        {
            for(int i=tooldrive_start_index;i<articulationChain.Length;i+=1)
            {
                ArticulationDrive limit = articulationChain[i].xDrive;
                if(limit.lowerLimit == 0)  
                {
                    limit.target = limit.lowerLimit;
                }
                else if(limit.upperLimit == 0)
                {
                    limit.target = limit.upperLimit;
                }
                articulationChain[i].xDrive = limit;
            }
            return true;
        }

        public bool OpenToolDrives() // Temporary - do better
        {
            for(int i=tooldrive_start_index;i<articulationChain.Length;i+=1)
            {
                ArticulationDrive limit = articulationChain[i].xDrive;
                if(limit.lowerLimit == 0)
                {
                    limit.target = limit.upperLimit;
                }
                else if(limit.upperLimit == 0)
                {
                    limit.target = limit.lowerLimit;
                }
                articulationChain[i].xDrive = limit;
            }
            return true;
        }


        public bool WriteJointTargetVelocities(float[] velocities)
        {
            return true;
        }
        
        public bool WriteSingleJointTargetVelocity(string jointName, float velocity)
        {
            return true;
        }
        public bool WriteSingleJointVelocity(int _joint_number, float _velocity)
        {
            write_velocity = articulationChain[_joint_number].jointVelocity;
            joint_drive = articulationChain[_joint_number].xDrive;
            write_velocity[0] = _velocity;

            joint_drive.targetVelocity = (float)(_velocity);
            articulationChain[_joint_number].jointVelocity = write_velocity;
            articulationChain[_joint_number].xDrive = joint_drive;
            return true;
        }

        public bool WriteJointVelocities(float[] velocities)
        {
            return true;
        }

        public bool WriteSingleJointVelocities(string jointName, float velocity)
        {
            return true;
        }

        public bool WriteStiffnessToJoints(float[] _stiffnesses)
        {
            for(int i=1;i<tooldrive_start_index;i+=1)
            {
                joint_drives[i-1] = articulationChain[i].xDrive;
                joint_drives[i-1].stiffness = _stiffnesses[i-1];
                articulationChain[i].xDrive = joint_drives[i-1];
            }
            return true;
        }
        public bool WriteUniformStiffnessToJoints(float _stiffness)
        {
            for(int i=1;i<tooldrive_start_index;i+=1)
            {
                joint_drives[i-1] = articulationChain[i].xDrive;
                joint_drives[i-1].stiffness = _stiffness;
                articulationChain[i].xDrive = joint_drives[i-1];
            }
            return true;
        }
        public bool WriteStiffnessToSingleJoint(string jointName, float stiffness)
        {
            return true;
        }
        public bool WriteStiffnessToSingleJoint(int jointIndex, float stiffness)
        {
            return true;
        }

        public bool WriteDampingToJoints(float[] _dampings)
        {
            for(int i=1;i<tooldrive_start_index;i+=1)
            {
                joint_drives[i-1] = articulationChain[i].xDrive;
                joint_drives[i-1].damping = _dampings[i-1];
                articulationChain[i].xDrive = joint_drives[i-1];
            }
            return true;
        }
        public bool WriteUniformDampingToJoints(float _damping)
        {
            for(int i=1;i<tooldrive_start_index;i+=1)
            {
                joint_drives[i-1] = articulationChain[i].xDrive;
                joint_drives[i-1].damping = _damping;
                articulationChain[i].xDrive = joint_drives[i-1];
            }
            return true;
        }
        public bool WriteDampingToSingleJoint(string jointName, float stiffness)
        {
            return true;
        }
        public bool WriteDampingToSingleJoint(int jointIndex, float stiffness)
        {
            return true;
        }

        public bool WriteForceLimitsToJoints(float[] _forces)
        {
            for(int i=1;i<tooldrive_start_index;i+=1)
            {
                joint_drives[i-1] = articulationChain[i].xDrive;
                joint_drives[i-1].forceLimit = _forces[i-1];
                articulationChain[i].xDrive = joint_drives[i-1];
            }
            return true;
        }
        public bool WriteUniformForceLimitsToJoints(float _force)
        {
            for(int i=1;i<tooldrive_start_index;i+=1)
            {
                joint_drives[i-1] = articulationChain[i].xDrive;
                joint_drives[i-1].forceLimit = _force;
                articulationChain[i].xDrive = joint_drives[i-1];
            }
            return true;
        }
        public bool WriteForLimitToSingleJoint(string jointName, float stiffness)
        {
            return true;
        }
        public bool WriteForceLimitToSingleJoint(int jointIndex, float stiffness)
        {
            return true;
        }

        public bool UseGravityOnEntire_robot(bool on_off_flag)
        {
            return true;
        }
        
        public bool UseMeshCollidersOnEntire_robot(bool on_off_flag)
        {
            return true;
        }

        public bool SetGravityStatusOnJoint(string jointName, bool on_off_flag)
        {
            return true;
        }

        public bool SetMeshColliderStatusOnJoint(string jointName, bool on_off_flag)
        {
            return true;
        }

    }
    };
}
