using System;

namespace GameDevBuddies
{
    /// <summary>
    /// Structure holding information about one recorded frame of terrain scan lines spreading.
    /// </summary>
    [Serializable]
    public struct TerrainSpreadRecordedFrame
    {
        public float Time;
        public float SpreadRange;
        public float GlobalEffectOpacity;
        public float DarkCircleOpacity;
    }
}