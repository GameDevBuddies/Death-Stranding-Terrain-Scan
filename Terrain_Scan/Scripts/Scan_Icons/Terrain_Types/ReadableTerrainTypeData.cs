using System;

namespace GameDevBuddies
{
    /// <summary>
    /// Structure encapsulating the terrain type data with an enumeration specifying which
    /// terrain this data describes, for easier maintenance.
    /// </summary>
    [Serializable]
    public struct ReadableTerrainTypeData
    {
        public TerrainType TerrainType;
        public TerrainTypeData Data;
    }
}