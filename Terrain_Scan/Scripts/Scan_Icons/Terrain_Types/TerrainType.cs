using System;

namespace GameDevBuddies
{
    /// <summary>
    /// Enumeration specifying all currently supported terrain types.
    /// </summary>
    [Serializable]
    public enum TerrainType : byte
    {
        /// <summary>
        /// Normal, walkable terrain type.
        /// </summary>
        TERRAIN_NORMAL = 0,

        /// <summary>
        /// Steeper terrain than normal, can cause balance loss.
        /// </summary>
        STEEP_TERRAIN = 1,

        /// <summary>
        /// Dangerous, slippery terrain than can cause falling down with higher speeds.
        /// </summary>
        DANGEROUS_TERRAIN = 2,

        /// <summary>
        /// Terrain that can't be walked on.
        /// </summary>
        UNWALKABLE_TERRAIN = 3,

        /// <summary>
        /// Water that can be crossed without any repercussions.
        /// </summary>
        SHALLOW_WATER = 4,

        /// <summary>
        /// Deeper water that causes slower movement speed and reduces stamina.
        /// </summary>
        DEEP_WATER = 5,

        /// <summary>
        /// Water that is to deep for standing in, it will cause falling down and 
        /// being swept away by the current.
        /// </summary>
        TOO_DEEP_WATER = 6,

        /// <summary>
        /// Humanly made surface, ideal for running on.
        /// </summary>
        CONCRETE = 7,

        /// <summary>
        /// Grass taller than waist height, can be used for hiding from enemies.
        /// </summary>
        TALL_GRASS = 8
    }
}