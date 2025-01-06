using System;
using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Structure containing data required for issuing a render call for 
    /// rendering the terrain scan icons.
    /// </summary>
    [Serializable]
    public struct TerrainScanIconsRenderingData
    {
        /// <summary>
        /// World position of the origin of the terrain scan effect.
        /// </summary>
        public Vector3 TerrainScanOrigin;
        /// <summary>
        /// X-Z normalized direction of the terrain scan spreading effect.
        /// </summary>
        public Vector3 TerrainScanDirection;
        /// <summary>
        /// Max visible angle of the terrain scan effect.
        /// </summary>
        public float TerrainScanMaxAngle;
        /// <summary>
        /// Properties of the helper camera used for rendering normals and 
        /// depth of the terrain, as well as ID of special objects.
        /// They're tightly packed into a 3-component vector:
        /// X --> Height of the camera in local space of the terrain scan icons.
        /// Y --> Distance of the near clipping plane of the camera.
        /// Z --> Distance of the far clipping plane of the camera.
        /// </summary>
        public Vector3 TerrainScanIconsCameraProperties;
    }
}