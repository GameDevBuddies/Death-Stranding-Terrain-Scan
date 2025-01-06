using UnityEngine;

namespace GameDevBuddies
{
    [ExecuteAlways]
    public class CameraSetup : MonoBehaviour
    {
        [Header("References: ")]
        [SerializeField] private Camera _camera = null;
        [SerializeField] private TerrainScanIconsRenderer _iconsRenderer = null;

        [Header("Options: ")]
        [SerializeField] private float _cameraHeightOffset = 100f;

        private void Update()
        {
            if (!AreAllReferencesAssigned())
            {
                return;
            }

            // Terrain Icons Area.
            float terrainIconsAreaWidth = (_iconsRenderer.IconsColumnCount - 1) * _iconsRenderer.IconsSpacingBetweenColumns;
            float terrainIconsAreaHeight = (_iconsRenderer.IconsRowCount - 1) * _iconsRenderer.IconsSpacingBetweenRows;
            //terrainIconsAreaWidth = Mathf.Min(terrainIconsAreaHeight * 2f, terrainIconsAreaWidth);

            // Position camera directly above the effect.
            Vector3 cameraLocalPosition = new Vector3(0f, _cameraHeightOffset, terrainIconsAreaHeight * 0.5f);
            _camera.transform.localPosition = cameraLocalPosition;

            // Setup the camera size to match the terrain icons area perfectly.
            _camera.orthographic = true;
            _camera.orthographicSize = terrainIconsAreaHeight * 0.5f;

            // Calculating the orthographic camera aspect ratio based on the aspect ratio of the terrain icons area.
            float cameraAspectRatio = terrainIconsAreaWidth / terrainIconsAreaHeight;

            // Setting the orthographic camera viewport size and position.
            _camera.aspect = cameraAspectRatio;
        }

        private bool AreAllReferencesAssigned()
        {
            return _camera != null && _iconsRenderer != null;
        }
    }
}