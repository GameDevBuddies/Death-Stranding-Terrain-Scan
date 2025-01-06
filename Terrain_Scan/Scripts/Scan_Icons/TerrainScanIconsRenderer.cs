using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameDevBuddies
{
    /// <summary>
    /// Class responsible for rendering terrain scan icons. The rendering of icons is a two-step process.
    /// First, the compute shader is dispatched to calculate the visibility, position and type of icons.
    /// Then, an indirect procedural draw command is issued to render the icons based on the data that the
    /// compute shader has previously filled.
    /// </summary>
    [ExecuteAlways]
    public class TerrainScanIconsRenderer : MonoBehaviour
    {
        private const int KERNEL_NOT_INITIALIZED = -1;

        [Header("References: ")]
        [SerializeField] private Transform _cameraTransform = null;
        [SerializeField] private TerrainScanIconsAnimator _iconsAnimator = null;

        [Header("Rendering References: ")]
        [SerializeField] private Mesh _iconMesh = null;
        [SerializeField] private Material _iconMaterial = null;

        [Header("Compute Shader References: ")]
        [SerializeField] private Camera _iconsDataCamera = null;
        [SerializeField] private ComputeShader _iconsInfoComputeShader = null;
        [SerializeField] private RenderTexture _normalsAndIdRT = null;
        [SerializeField] private RenderTexture _depthRT = null;

        [Header("Options: ")]
        [SerializeField, Range(1, 150)] private int _iconsRowCount = 75;
        [SerializeField, Range(1, 300)] private int _iconsColumnCount = 150;
        [SerializeField, Range(0f, 1f)] private float _iconsSpacingBetweenRows = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _iconsSpacingBetweenColumns = 0.5f;

        [Header("Terrain Types: ")]
        [SerializeField] private float _waterLevelWorldSpace = 0f;
        [SerializeField] private TerrainTypesGeneralInfo _terrainTypesGeneralInfo = default;
        [SerializeField] private TerrainTypes _terrainTypesScriptable = null;

        // General rendering info.
        private TerrainScanIconsRenderingData _iconsRenderingData;
        private bool _iconsRenderingActive = false;
        
        // Compute shader helper variables.
        private int _kernelId = KERNEL_NOT_INITIALIZED;
        private TerrainIconInfo[] _terrainIconsInfoArray = null;
        private ComputeBuffer _terrainIconsInfoBuffer = null;
        private ComputeBuffer _terrainTypesDataBuffer = null;
        private ComputeBuffer _terrainGeneralInfoBuffer = null;

        // Indirect rendering helper variables.
        private GraphicsBuffer _drawArgumentsBuffer = null;
        private GraphicsBuffer.IndirectDrawIndexedArgs[] _drawArguments;

        public int IconsRowCount { get => _iconsRowCount; }
        public int IconsColumnCount { get => _iconsColumnCount; }
        public float IconsSpacingBetweenRows { get => _iconsSpacingBetweenRows; }
        public float IconsSpacingBetweenColumns { get => _iconsSpacingBetweenColumns; }
        public bool IconsRenderingActive { get => _iconsRenderingActive; }
        public float VisibilityRange
        {
            get
            {
                return _iconsRowCount * _iconsSpacingBetweenRows;
            }
        }

        private int KernelId
        {
            get
            {
                if(_kernelId == KERNEL_NOT_INITIALIZED)
                {
                    _kernelId = _iconsInfoComputeShader.FindKernel("CSMain");
                }
                return _kernelId;
            }
        }
        private Vector3 MainCameraPosition
        {
            get
            {
#if UNITY_EDITOR                
                if (!EditorApplication.isPlaying && SceneView.lastActiveSceneView != null)
                {
                    SceneView sceneView = SceneView.currentDrawingSceneView == null ? SceneView.lastActiveSceneView : SceneView.currentDrawingSceneView;
                    if (sceneView != null)
                    {
                        return sceneView.camera.transform.position;
                    }
                }
#endif
                return _cameraTransform.position;
            }
        }

        private void OnDestroy()
        {
            if (!AreAllReferencesAssigned())
            {
                return;
            }

            ReleaseResources();
        }

        private void Update()
        {
            if (!_iconsRenderingActive)
            {
                return;
            }

            if(!AreAllReferencesAssigned())
            {
                return;
            }

            if(ShouldInititializeResources())
            {
                InitializeResources();
            }

            RenderParams renderParams = new RenderParams(_iconMaterial);

            // Setting up correct world bounds for frustum culling of the icons.
            Vector3 boundsCenter = _iconsRenderingData.TerrainScanOrigin;
            Vector3 boundsSize = new Vector3(_iconsRowCount * _iconsSpacingBetweenRows * 2f, _iconsDataCamera.farClipPlane - _iconsDataCamera.nearClipPlane, _iconsColumnCount * _iconsSpacingBetweenColumns);
            renderParams.worldBounds = new Bounds(boundsCenter, boundsSize);

            // Setting up correct layer for correct camera filtering.
            renderParams.layer = gameObject.layer;

            // Send the latest data to the compute shader.
            UpdateIndirectDrawData();

#if UNITY_EDITOR
            UpdateComputeShaderData();

            // Fix for Unity Editor scene save resetting the compute buffer connection with the compute shader.
            if (!EditorApplication.isPlaying)
            {
                TerrainTypeData[] terrainTypesData = _terrainTypesScriptable.GetTerrainTypes();
                _terrainTypesDataBuffer = new ComputeBuffer(terrainTypesData.Length, TerrainTypeData.StructSize);
                _terrainTypesDataBuffer.SetData(terrainTypesData);

                _iconsInfoComputeShader.SetBuffer(KernelId, "_TerrainTypesData", _terrainTypesDataBuffer);
                _iconsInfoComputeShader.SetBuffer(KernelId, "_TerrainTypesGeneralInfo", _terrainGeneralInfoBuffer);
                _iconsInfoComputeShader.SetBuffer(KernelId, "_TerrainIconsData", _terrainIconsInfoBuffer);
                _iconMaterial.SetBuffer("_TerrainIconsData", _terrainIconsInfoBuffer);              

                _iconsInfoComputeShader.SetTexture(KernelId, "_NormalsAndIdRT", _normalsAndIdRT);
                _iconsInfoComputeShader.SetTexture(KernelId, "_DepthRT", _depthRT);
            }
#endif

            // Dispatch compute shader that will calculate the position, rotation and size of each terrain scan icon.            
            _iconsInfoComputeShader.DispatchThreads(KernelId, _iconsColumnCount, _iconsRowCount);

            // Issue indirect draw call for rendering the icons.
            Graphics.RenderMeshIndirect(renderParams, _iconMesh, _drawArgumentsBuffer);
        }

        public void StartIconsRendering(TerrainScanIconsRenderingData renderingData)
        {
            _iconsRenderingData = renderingData;
            _iconsRenderingActive = true;

            if (_terrainIconsInfoBuffer != null && _terrainIconsInfoArray != null)
            {
                // Clearing current icons data buffer to start fresh with new icons.
                int iconsCount = _iconsRowCount * _iconsColumnCount;
                _terrainIconsInfoArray = new TerrainIconInfo[iconsCount];
                _terrainIconsInfoBuffer.SetData(_terrainIconsInfoArray);
                _iconsInfoComputeShader.SetBuffer(KernelId, "_TerrainIconsData", _terrainIconsInfoBuffer);
                _iconMaterial.SetBuffer("_TerrainIconsData", _terrainIconsInfoBuffer);
            }

            _iconsAnimator.StartIconsAnimation(this, _iconsInfoComputeShader, _iconMaterial);
        }

        public void StopIconsRendering()
        {
            _iconsRenderingActive = false;
        }

        private bool AreAllReferencesAssigned()
        {
            return _iconMesh != null && _iconMaterial != null && _cameraTransform != null && _iconsInfoComputeShader != null && _terrainTypesScriptable != null;
        }

        private bool ShouldInititializeResources()
        {
            return _terrainIconsInfoBuffer == null || _terrainTypesDataBuffer == null || _terrainGeneralInfoBuffer == null || _drawArgumentsBuffer == null;
        }

        private void InitializeResources()
        {
            InitializeComputeResources();
            InitializeIndirectDrawResources();
        }

        private void ReleaseResources()
        {
            ReleaseComputeResources();
            ReleaseIndirectDrawResources();
        }

        private void InitializeIndirectDrawResources()
        {
            int iconsCount = _iconsRowCount * _iconsColumnCount;
            _drawArgumentsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            _drawArguments = new GraphicsBuffer.IndirectDrawIndexedArgs[1]
            {
                new GraphicsBuffer.IndirectDrawIndexedArgs
                {
                    baseVertexIndex = 0,
                    startIndex = 0,
                    startInstance = 0,
                    indexCountPerInstance = _iconMesh.GetIndexCount(0),
                    instanceCount = (uint)iconsCount
                }
            };

            // Set the indirect draw data to the arguments buffer.
            _drawArgumentsBuffer.SetData(_drawArguments);
        }

        private void UpdateIndirectDrawData()
        {
            _iconMaterial.SetVector("_MainCameraWorldPosition", MainCameraPosition);
            _iconMaterial.SetVector("_TerrainScanOrigin", _iconsRenderingData.TerrainScanOrigin);
        }

        private void ReleaseIndirectDrawResources()
        {
            _drawArgumentsBuffer?.Dispose();
            _drawArgumentsBuffer?.Release();
            _drawArgumentsBuffer = null;
            _drawArguments = null;
        }

        private void InitializeComputeResources()
        {
            // Initializing the compute buffer with the required number of elements.
            int iconsCount = _iconsRowCount * _iconsColumnCount;
            _terrainIconsInfoArray = new TerrainIconInfo[iconsCount];

            // Terrain types data structure set up.
            TerrainTypeData[] terrainTypesData = _terrainTypesScriptable.GetTerrainTypes();
            _terrainTypesDataBuffer = new ComputeBuffer(terrainTypesData.Length, TerrainTypeData.StructSize);
            _terrainTypesDataBuffer.SetData(terrainTypesData);

            // General terrain info set up.
            _terrainGeneralInfoBuffer = new ComputeBuffer(1, TerrainTypesGeneralInfo.StructSize);
            _terrainGeneralInfoBuffer.SetData(GetTerrainGeneralInfoArray());

            // Terrain scan info structure set up.
            _terrainIconsInfoBuffer = new ComputeBuffer(iconsCount, TerrainIconInfo.StructSize);
            _terrainIconsInfoBuffer.SetData(_terrainIconsInfoArray);

            // Assigning the buffers to the compute shader and the rendering material.
            _iconsInfoComputeShader.SetBuffer(KernelId, "_TerrainTypesData", _terrainTypesDataBuffer);
            _iconsInfoComputeShader.SetBuffer(KernelId, "_TerrainTypesGeneralInfo", _terrainGeneralInfoBuffer);
            _iconsInfoComputeShader.SetBuffer(KernelId, "_TerrainIconsData", _terrainIconsInfoBuffer);
            _iconMaterial.SetBuffer("_TerrainIconsData", _terrainIconsInfoBuffer);

            // Assigning render textures to the compute shader so that the height of the icons could be adjusted.
            if (_normalsAndIdRT != null)
            {
                _iconsInfoComputeShader.SetTexture(KernelId, "_NormalsAndIdRT", _normalsAndIdRT);
                _iconsInfoComputeShader.SetInt("_NormalsAndIdRTWidth", _normalsAndIdRT.width);
                _iconsInfoComputeShader.SetInt("_NormalsAndIdRTHeight", _normalsAndIdRT.height);
            }
            if (_depthRT != null)
            {
                _iconsInfoComputeShader.SetTexture(KernelId, "_DepthRT", _depthRT);
            }

            UpdateComputeShaderData();
        }

        private void UpdateComputeShaderData()
        {
            if(_iconsInfoComputeShader == null)
            {
                return;
            }

            // Calculating the direction of terrain scan spread and its corresponding right vector.
            Vector3 terrainScanOrigin = _iconsRenderingData.TerrainScanOrigin;
            Vector3 scanEffectDirection = _iconsRenderingData.TerrainScanDirection;
            (Vector3 localRightVector, Vector3 _) = CreateOrthogonalAxes(scanEffectDirection);

            // Assigning terrain scan effect properties to the compute shader.
            _iconsInfoComputeShader.SetVector("_TerrainScanSpreadDirection", scanEffectDirection);
            _iconsInfoComputeShader.SetVector("_TerrainScanSpreadRightDirection", localRightVector);
            _iconsInfoComputeShader.SetFloat("_TerrainScanMaxAngle", _iconsRenderingData.TerrainScanMaxAngle);

            // Assigning the icon camera data, used for determining the position of each icon.
            _iconsInfoComputeShader.SetVector("_IconsCameraProperties", _iconsRenderingData.TerrainScanIconsCameraProperties);

            // Updating terrain general information to the GPU.
            _terrainGeneralInfoBuffer.SetData(GetTerrainGeneralInfoArray());
            _iconsInfoComputeShader.SetBuffer(KernelId, "_TerrainTypesGeneralInfo", _terrainGeneralInfoBuffer);

#if UNITY_EDITOR
            // Updating terrain types data to update changes made during testing.
            if(_terrainTypesDataBuffer != null)
            {
                _terrainTypesDataBuffer.Release();
            }
            TerrainTypeData[] terrainTypesData = _terrainTypesScriptable.GetTerrainTypes();
            _terrainTypesDataBuffer = new ComputeBuffer(terrainTypesData.Length, TerrainTypeData.StructSize);
            _terrainTypesDataBuffer.SetData(terrainTypesData);
            _iconsInfoComputeShader.SetBuffer(KernelId, "_TerrainTypesData", _terrainTypesDataBuffer);
#endif

            // Assigning icons distribution properties to the shader.
            _iconsInfoComputeShader.SetFloat("_IconsVisibilityRangeSquared", VisibilityRange * VisibilityRange);
            _iconsInfoComputeShader.SetInt("_IconsRowCount", _iconsRowCount);
            _iconsInfoComputeShader.SetInt("_IconsColumnCount", _iconsColumnCount);
            _iconsInfoComputeShader.SetFloat("_IconsSpacingBetweenRows", _iconsSpacingBetweenRows);
            _iconsInfoComputeShader.SetFloat("_IconsSpacingBetweenColumns", _iconsSpacingBetweenColumns);
        }

        private TerrainTypesGeneralInfo[] GetTerrainGeneralInfoArray()
        {
            // If we want to send a structure to the GPU, we need to pack it into a compute buffer, which
            // requires making an array from it, even though it contains only one element.
            TerrainTypesGeneralInfo[] terrainGeneralInfoArray = new TerrainTypesGeneralInfo[1];
            terrainGeneralInfoArray[0] = _terrainTypesGeneralInfo;

            // Assigning the height of the water in local space.
            terrainGeneralInfoArray[0].WaterHeightLocalSpace = _waterLevelWorldSpace - _iconsRenderingData.TerrainScanOrigin.y;

            return terrainGeneralInfoArray;
        }

        private void ReleaseComputeResources()
        {
            _terrainIconsInfoBuffer?.Dispose();
            _terrainIconsInfoBuffer?.Release();
            _terrainIconsInfoBuffer = null;

            _terrainTypesDataBuffer?.Dispose();
            _terrainTypesDataBuffer?.Release();
            _terrainTypesDataBuffer = null;

            _terrainGeneralInfoBuffer?.Dispose();
            _terrainGeneralInfoBuffer?.Release();
            _terrainGeneralInfoBuffer = null;

            _terrainIconsInfoArray = null;
            _kernelId = KERNEL_NOT_INITIALIZED;
        }

        /// <summary>
        /// Function calculates orthogonal axes for the provided <paramref name="forwardAxis"/>.
        /// </summary>
        /// <param name="forwardAxis">Normalized forward direction for which the orthogonal axis should
        /// be calculated.</param>
        /// <returns>Tuple of (rightAxis, upAxis) that are orthogonal on the provided <paramref name="forwardAxis"/>.</returns>
        protected virtual (Vector3, Vector3) CreateOrthogonalAxes(Vector3 forwardAxis)
        {
            Vector3 rightAxis;
            if (Mathf.Approximately(Mathf.Abs(forwardAxis.y), 1f))
            {
                // When looking straight up/down.
                rightAxis = forwardAxis.y > 0f ? Vector3.right : Vector3.left;
            }
            else
            {
                rightAxis = Vector3.Cross(Vector3.up, forwardAxis).normalized;
            }
            Vector3 upAxis = Vector3.Cross(forwardAxis, rightAxis);
            return (rightAxis, upAxis);
        }
    }
}