using UnityEngine;
using UnityEngine.Rendering;

namespace GameDevBuddies
{
    /// <summary>
    /// Class responsible for controlling the terrain scan icons animation by invoking rendering
    /// commands from the <see cref="TerrainScanIconsRenderer"/> class and reacting to the events
    /// of the <see cref="TerrainScanLinesSpreadController"/>. This class serves as a mediator between the 
    /// terrain scan controller and the icons renderer classes.
    /// 
    /// Also, this class controls the rendering of the helper camera that will render depth, normals
    /// and IDs of special objects into helper textures that will be used by the GPU for correctly
    /// rendering icons above the terrain.
    /// </summary>
    public class TerrainScanIconsController : MonoBehaviour
    {
        [Header("References: ")]
        [SerializeField] private TerrainScanIconsRenderer _iconsRenderer = null;
        [SerializeField] private Camera _iconsDataCamera = null;

        private bool _subscribedToEvents = false;
        private bool _iconsDataReady = false;
        private bool _prewarmingCamera = false;
        private TerrainScan _terrainScan = null;
        private TerrainScanInfo _terrainScanInfo;

        private void Start()
        {
            SubscribeToEvents();
            PrewarmIconsDataCamera();
        }

        private void Update()
        {
            // Whenever the data for rendering icons is ready, notify the icons renderer
            // that it can start with the rendering of the terrain scan icons.
            if (_iconsDataReady)
            {
                _iconsDataReady = false;

                // We need to render with the camera at the start of the application to initialize all hidden data.
                // I didn't want to research why the first render call is always unsuccessful at the start of the application.
                // However, every other call works just fine.
                if (_prewarmingCamera)
                {
                    _prewarmingCamera = false;
                    return;
                }

                // The supporting data for icons creation is ready, start the icons rendering.
                _iconsRenderer.StartIconsRendering(GetIconsRenderingData());
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (_iconsRenderer == null || _iconsDataCamera == null)
            {
                Debug.LogError("Not all references assigned, can't subscribe to required events.");
                return;
            }

            _terrainScan = TerrainScan.Instance;
            _terrainScan.OnTerrainScanStart += RenderRequiredAdditionalData;
            RenderPipelineManager.endCameraRendering += OnCameraRenderCompleted;

            _subscribedToEvents = true;
        }

        private void UnsubscribeFromEvents()
        {
            if (!_subscribedToEvents)
            {
                return;
            }

            if (_terrainScan != null)
            {
                _terrainScan.OnTerrainScanStart -= RenderRequiredAdditionalData;
            }
            RenderPipelineManager.endCameraRendering -= OnCameraRenderCompleted;

            _subscribedToEvents = false;
        }

        private void RenderRequiredAdditionalData(TerrainScanInfo terrainScanInfo)
        {
            if (_iconsDataCamera == null)
            {
                Debug.LogError("Required camera reference hasn't been assigned. " +
                    "Can't render terrain data required for the icons.");
                return;
            }

            // Cache the terrains can information.
            _terrainScanInfo = terrainScanInfo;

            // Stop the current rendering of icons. If it's currently not active, this will do nothing.
            // This prevents previous rendering of icons while the camera writes to the helper data textures.
            _iconsRenderer.StopIconsRendering();

            // Turning on the icons data camera so that it will render required terrain depth and normals
            // into helper textures that will be used by the icons renderer.
            _iconsDataCamera.enabled = true;
        }

        private void OnCameraRenderCompleted(ScriptableRenderContext context, Camera camera)
        {
            if (camera != _iconsDataCamera)
            {
                return;
            }

            // Whenever icons data camera finishes rendering, disable it and notify the icons renderer
            // that the data is ready for being used to display the icons. The icons renderer will be 
            // notified in the next frame to ensure that the data is properly ready on the GPU. This one
            // frame difference will be unnoticeable.
            _iconsDataCamera.enabled = false;
            _iconsDataReady = true;
        }

        private void PrewarmIconsDataCamera()
        {
            _prewarmingCamera = true;
            _iconsDataCamera.enabled = true;
        }

        private TerrainScanIconsRenderingData GetIconsRenderingData()
        {
            TerrainScanIconsRenderingData terrainScanIconsRenderingData = new TerrainScanIconsRenderingData
            {
                TerrainScanOrigin = _terrainScanInfo.Origin,
                TerrainScanDirection = _terrainScanInfo.Direction,
                TerrainScanMaxAngle = _terrainScanInfo.MaxAngle,
                TerrainScanIconsCameraProperties = PackIconsDataCameraProperties()
            };
            return terrainScanIconsRenderingData;
        }

        private Vector3 PackIconsDataCameraProperties()
        {
            return new Vector3(_iconsDataCamera.transform.localPosition.y, _iconsDataCamera.nearClipPlane, _iconsDataCamera.farClipPlane);
        }
    }
}