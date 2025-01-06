using System;
using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Structure defining the required data for rendering one terrain scan icon.
    /// Structure is matching the declaration in the compute shader.
    /// </summary>
    [Serializable]
    public struct TerrainIconInfo
    {
        public const int StructSize = sizeof(float) * 6 + sizeof(uint) * 3;

        public Vector3 Position;
        public float Size;
        public float Opacity;
        public float DangerIconSpawnTime;
        public uint DangerIconSpawnTimeRecorded;

        public uint ShapeID;
        public uint ColorID;
    }
}