// Ignore Spelling: Unwalkable

using System;

namespace GameDevBuddies
{
    /// <summary>
    /// Structure defining the types of terrain used inside the compute shader to determine
    /// the subset of terrain types based on water depth and terrain steepness.
    /// </summary>
    [Serializable]
    public struct TerrainTypesGeneralInfo
    {
        public const int StructSize = sizeof(int) * 6 + sizeof(float) * 5;

        /// <summary>
        /// Total number of different terrain types currently supported.
        /// </summary>
        public int TotalTypesCount;
        /// <summary>
        /// Index of the steep terrain type in the buffer.
        /// </summary>
        public int SteepTerrainTypeIndex;
        /// <summary>
        /// Index of the unreachable terrain type in the buffer.
        /// </summary>
        public int UnwalkableTerrainTypeIndex;
        /// <summary>
        /// Maximum steepness of the terrain that can still be marked as normal.
        /// </summary>
        public float NormalTerrainThreshold;
        /// <summary>
        /// Maximum steepness of the terrain that can still be marked as steep.
        /// If the steepness goes beyond this threshold, terrain will become unreachable.
        /// </summary>
        public float SteepTerrainThreshold;
        /// <summary>
        /// Index of the shallow water terrain type in the buffer.
        /// </summary>
        public int ShallowWaterTypeIndex;
        /// <summary>
        /// Index of the deep water terrain type in the buffer.
        /// </summary>
        public int DeepWaterTypeIndex;
        /// <summary>
        /// Index of the too deep water terrain type in the buffer.
        /// </summary>
        public int TooDeepWaterTypeIndex;
        /// <summary>
        /// Water level in local space.
        /// </summary>
        public float WaterHeightLocalSpace;
        /// <summary>
        /// Maximum depth of the shallow water (in meters)
        /// </summary>
        public float ShallowWaterDepthThreshold;
        /// <summary>
        /// Maximum depth of the deep water (in meters).
        /// </summary>
        public float DeepWaterDepthThreshold;
    }
}