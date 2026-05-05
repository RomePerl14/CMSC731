# <b>Exoskeleton (and OSAM-1 Restore) URDFs</b>
This folder contains the URDFs for the MGA Exoskeleton and a low-res model of NASAs OSAM-1 Restore Arm, and the meshes that are required by each of them

## <b>Naming conventions</b>
This folder currently organizes Exoskeleton's meshes by link, rather than by joint. This is necessary because of Exoskeleton's "passive" joints, which are the sections of Exoskeleton that can adjusted manual to fit the person wearing it.

Corresponding link to folder:
- baselink: base_link_ScapulaMount
- joint1: link1_FD22-0043

TODO: romeo finish this

&nbsp;
## <b>Adjusting link lengths of Exoskeleton</b>
Although you shouldn't need to adjust any link lengths for exoskeleton, if required, please follow the instructions listed in either the .URDF file, or the .URDF.Xacro file. Manually adjusting link lengths is much easier in the .Xacro file, however you'll need to run it through ROS's xacro-to-urdf converter, or the one included in this package (borrowed for outside-ros environments)

&nbsp;
## <b>Using the Xacro to URDF parser (for outside ros environments)</b>
If you are accessing this URDF from outside a ROS environment (most likely because it's being used for Unity applications), you'll be provided a xacro2urdf python script that will allow you to edit the xacro urdf and generate a urdf. Because you are mostly likely in the SSLUnityVisualizations repo, you can find the tool in the ```tools/``` directory at SSLUnityVisualization/tools. To run the tool, use the following command ```from the parent directory of SSLUnityVisualization```:

    python ./tools/xacro_tool/scripts/xacro <path-to-urdf_xacro> -o <path-to-desired-output-urdf>

Here's an example (if you need):

    python ./tools/xacro_tool/scripts/xacro ./dev/robots/Ranger/ranger_v2.urdf.xacro -o ./dev/robots/Ranger/ranger_v2.urdf

See this README for more generic info on running the tool.

&nbsp;
## <b>Converting URDF xacro files to URDF files in Unity</b>
See this README for more info


&nbsp;
## <b>IMPORTANT NOTE ON MESH NAMES</b>
The names of the meshes used by Exoskeleton are wrong. Flat out. They do NOT correpsond with their original .prt files (as of 06/30/2023 - Romeo), HOWEVER they <i>are</i> the correct file. Someone in the future (unless romeo beat ya too it) should go through each file and rename them accordingly. If you look at each mesh, you'll notice that several of them share the same name (like "FD22-0097" being shared by different parts). DONT PANIC! This is partly why theyre organized :D 

You'll also notice a folder labeled ```assemblies/```. This folder contains the assemblies of each link, rather than the indiviual models for every part currently used by the URDF. It's possible in the future that we may use some of thess assemblies, but because of the nature of exoskeleton, and the ability for it's link lengths to be changed, its better to continue using each separate part to build the urdf.

&nbsp;
## <b><i>Authors</i></b>
This URDF was constructed by Natalie Condzal, and edited by Nicholas Vandermark and Romeo Perlstein. If there are any issues with the URDF, blame Romeo. 
