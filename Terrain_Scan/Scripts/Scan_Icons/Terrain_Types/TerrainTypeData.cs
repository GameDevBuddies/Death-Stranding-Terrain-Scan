using System;

namespace GameDevBuddies
{
    /// <summary>
    /// Structure defining data required for a specific part of the terrain to be correctly rendered.
    /// It is being packed into a structured buffer and sent to the GPU.
    /// </summary>
    [Serializable]
    public struct TerrainTypeData
    {
        public const int StructSize = sizeof(float) * 2 + sizeof(uint) * 2;

        /// <summary>
        /// How high above the terrain should icon be positioned for this type of the terrain.
        /// </summary>
        public float IconHeightOffset;
        /// <summary>
        /// Render size of the icon.
        /// </summary>
        public float IconSize;
        /// <summary>
        /// ID specifying the shape of the icon in the shape flip book texture.
        /// </summary>
        public uint IconShapeID;
        /// <summary>
        /// ID specifying the color of the icon in the color map texture.
        /// </summary>
        public uint IconColorID;
    }
}