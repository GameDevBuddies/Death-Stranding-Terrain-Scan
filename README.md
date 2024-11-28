![](GitHub%20Images/ReadMe_Image.jpg)
# Death-Stranding-Terrain-Scan
Repository containing files for the Death Stranding Terrain Scan YouTube video.

# Info
The effect was created in <b>Unity 2022.3.38f1</b> with URP version <b>14.0.11</b>.

# Required Packages
This effect relies on DoTween asset for animating the spread effect. Make sure to have it imported in your project before adding the "TerrainScanController.cs" file. (https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)

# How To Use
1. Add the Terrain_Scan_Shader.shader file and the CustomBlendingFunctions.hlsl file into your project (THEY MUST BE NEXT TO EACH OTHER!) and create a material from it.
2. Assign the material to the <b>Full Screen Pass Renderer Feature</b> as the Pass Material. 
3. Add the TerrainScanController.cs script to your project and assign it to an empty object that will serve as a root of the effect.
4. Invoke the "StartAnimation" public method to start the spreading animation. 
