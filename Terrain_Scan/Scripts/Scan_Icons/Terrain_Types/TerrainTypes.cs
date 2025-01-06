using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Scriptable object containing all currently supported terrain types.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(TerrainTypes), menuName = "ScriptableObjects/" + nameof(TerrainTypes))]
    public class TerrainTypes: ScriptableObject
    {
        /// <summary>
        /// Collection of supported terrain types in a humanly readable format.
        /// </summary>
        [SerializeField] private ReadableTerrainTypeData[] _terrainTypesData = null;

        /// <summary>
        /// Function returns the supported terrain types in an array in a readable format 
        /// ready for transferring to the GPU.
        /// </summary>
        /// <returns>Array of <see cref="TerrainTypeData"/> elements.</returns>
        public TerrainTypeData[] GetTerrainTypes()
        {
            // Converting from a humanly readable format to the GPU readable format.
            TerrainTypeData[] terrainTypesData = new TerrainTypeData[_terrainTypesData.Length];
            for (int i = 0; i < _terrainTypesData.Length; i++)
            {
                ReadableTerrainTypeData readableTerrainTypeData = _terrainTypesData[i];
                terrainTypesData[i] = readableTerrainTypeData.Data;
            }
            return terrainTypesData;
        }
    }
}