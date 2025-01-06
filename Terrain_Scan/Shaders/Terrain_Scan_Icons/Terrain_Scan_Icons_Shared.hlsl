#ifndef TERRAIN_ICONS_SHARED_DATA_INCLUDED
#define TERRAIN_ICONS_SHARED_DATA_INCLUDED

// Structure containing information about one terrain scan icon.
// All values are expressed in local space.
// This structure is filled by the compute shader and propagated toward
// the rendering shader that reads the data an applies correct icon/color.
struct TerrainIconInfo 
{
    float3 Position;
    float Size;
    float Opacity;
    float DangerIconSpawnTime;
    uint DangerIconSpawnTimeRecorded;
    uint ShapeID;
    uint ColorID;
};

// Structure containing information about different types of the terrain.
// This is filled from the CPU side at the start of the application and
// propagated to the compute shader.
struct TerrainTypeData 
{
    float HeightOffset;
    float Size;
    uint ShapeID;
    uint ColorID;
};

// Structure defining the types of terrain used inside the compute shader to determine
// the subset of terrain types based on water depth and terrain steepness.
struct TerrainTypesGeneralInfo 
{
    // Total number of different terrain types currently supported.
    int TotalTypesCount;

    // Index of the steep terrain type in the buffer.
    int SteepTerrainTypeIndex;

    // Index of the unreachable terrain type in the buffer.
    int UnwalkableTerrainTypeIndex;

    // Maximum steepness of the terrain that can still be marked as normal.
    float NormalTerrainThreshold;

    // Maximum steepness of the terrain that can still be marked as steep.
    // If the steepness goes beyond this threshold, terrain will become unreachable.
    float SteepTerrainThreshold;

    // Index of the shallow water terrain type in the buffer.
    int ShallowWaterTypeIndex;

    // Index of the deep water terrain type in the buffer.
    int DeepWaterTypeIndex;

    // Index of the too deep water terrain type in the buffer.
    int TooDeepWaterTypeIndex;

    // Water level in local space.
    float WaterHeightLocalSpace;

    // Maximum depth of the shallow water (in meters).
    float ShallowWaterDepthThreshold;

    // Maximum depth of the deep water (in meters).
    float DeepWaterDepthThreshold;
};

#endif // TERRAIN_ICONS_SHARED_DATA_INCLUDED